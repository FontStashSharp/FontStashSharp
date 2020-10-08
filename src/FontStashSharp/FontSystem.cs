using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FontStashSharp
{
	unsafe class FontSystem : IDisposable
	{
		class GlyphCollection
		{
			internal readonly Int32Map<FontGlyph> Glyphs = new Int32Map<FontGlyph>();
		}

		readonly Int32Map<GlyphCollection> _glyphs = new Int32Map<GlyphCollection>();

		readonly IFontRasterizationService _fontRasterizationService;
		readonly ITextureCreationService _textureCreationService;

		float _ith;
		float _itw;
		FontAtlas _currentAtlas;
		Point _size;

		public int FontSize { get; set; }

		public readonly int BlurAmount;
		public readonly int StrokeAmount;
		public float Spacing;
		public float LineSpacing = 0f;
		public PointF Scale;
		public bool UseKernings = true;

		public int? DefaultCharacter = ' ';

		public FontAtlas CurrentAtlas
		{
			get
			{
				if (_currentAtlas == null)
				{
					_currentAtlas = new FontAtlas(_size.X, _size.Y, 256);
					Atlases.Add(_currentAtlas);
				}

				return _currentAtlas;
			}
		}

		public List<FontAtlas> Atlases { get; } = new List<FontAtlas>();

		public event EventHandler CurrentAtlasFull;

		public FontSystem(IFontRasterizationService fontRasterizationService, ITextureCreationService textureCreationService, 
			int width, int height, int blurAmount = 0, int strokeAmount = 0)
		{
			if (fontRasterizationService == null)
			{
				throw new ArgumentNullException(nameof(fontRasterizationService));
			}

			if (textureCreationService == null)
			{
				throw new ArgumentNullException(nameof(textureCreationService));
			}

			_fontRasterizationService = fontRasterizationService;
			_textureCreationService = textureCreationService;

			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}

			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}

			if (blurAmount < 0 || blurAmount > 20)
			{
				throw new ArgumentOutOfRangeException(nameof(blurAmount));
			}

			if (strokeAmount < 0 || strokeAmount > 20)
			{
				throw new ArgumentOutOfRangeException(nameof(strokeAmount));
			}

			if (strokeAmount != 0 && blurAmount != 0)
			{
				throw new ArgumentException("Cannot have both blur and stroke.");
			}

			BlurAmount = blurAmount;
			StrokeAmount = strokeAmount;

			_size = new Point(width, height);

			_itw = 1.0f / _size.X;
			_ith = 1.0f / _size.Y;
			ClearState();
		}

		public void Dispose()
		{
			Atlases?.Clear();
			_currentAtlas = null;
			_glyphs?.Clear();
		}

		public void ClearState()
		{
			FontSize = 12;
			Spacing = 0;
		}

		GlyphCollection GetGlyphsCollection(int   size)
		{
			GlyphCollection result;
			if (_glyphs.TryGetValue(size, out result))
			{
				return result;
			}

			result = new GlyphCollection();
			_glyphs[size] = result;
			return result;
		}

		private void PreDraw(string str, out GlyphCollection glyphs, out float ascent, out float lineHeight)
		{
			glyphs = GetGlyphsCollection(FontSize);

			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				var glyph = GetGlyph(glyphs, codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				ascent = glyph.Ascent;
				lineHeight = glyph.LineHeight + LineSpacing;
				break;
			}
		}

		public float DrawText(IRenderingService batch, float x, float y, string str, FssColor color, float depth)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, false);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					q.X0 = (int)(q.X0 * Scale.X);
					q.X1 = (int)(q.X1 * Scale.X);
					q.Y0 = (int)(q.Y0 * Scale.Y);
					q.Y1 = (int)(q.Y1 * Scale.Y);

					var destRect = new Rectangle((int)(x + q.X0),
												(int)(y + q.Y0),
												(int)(q.X1 - q.X0),
												(int)(q.Y1 - q.Y0));

					var sourceRect = new Rectangle((int)(q.S0 * _size.X),
												(int)(q.T0 * _size.Y),
												(int)((q.S1 - q.S0) * _size.X),
												(int)((q.T1 - q.T0) * _size.Y));

					batch.Draw(glyph.Atlas.Texture,
						destRect,
						sourceRect,
						color,
						depth);
				}

				prevGlyph = glyph;
			}

			return x;
		}

		public float DrawText(IRenderingService batch, float x, float y, string str, FssColor[] glyphColors, float depth)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var pos = 0;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					++pos;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, false);
				if (glyph == null)
				{
					++pos;
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					q.X0 = (int)(q.X0 * Scale.X);
					q.X1 = (int)(q.X1 * Scale.X);
					q.Y0 = (int)(q.Y0 * Scale.Y);
					q.Y1 = (int)(q.Y1 * Scale.Y);

					var destRect = new Rectangle((int)(x + q.X0),
												(int)(y + q.Y0),
												(int)(q.X1 - q.X0),
												(int)(q.Y1 - q.Y0));

					var sourceRect = new Rectangle((int)(q.S0 * _size.X),
												(int)(q.T0 * _size.Y),
												(int)((q.S1 - q.S0) * _size.X),
												(int)((q.T1 - q.T0) * _size.Y));

					batch.Draw(glyph.Atlas.Texture,
						destRect,
						sourceRect,
						glyphColors[pos],
						depth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return x;
		}

		private void PreDraw(StringBuilder str, out GlyphCollection glyphs, out float ascent, out float lineHeight)
		{
			glyphs = GetGlyphsCollection(FontSize);

			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				var glyph = GetGlyph(glyphs, codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				ascent = glyph.Ascent;
				lineHeight = glyph.LineHeight + LineSpacing;
				break;
			}
		}

		public float DrawText(IRenderingService batch, float x, float y, StringBuilder str, FssColor color, float depth)
		{
			if (str == null || str.Length == 0) return 0.0f;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, false);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					q.X0 = (int)(q.X0 * Scale.X);
					q.X1 = (int)(q.X1 * Scale.X);
					q.Y0 = (int)(q.Y0 * Scale.Y);
					q.Y1 = (int)(q.Y1 * Scale.Y);

					var destRect = new Rectangle((int)(x + q.X0),
												(int)(y + q.Y0),
												(int)(q.X1 - q.X0),
												(int)(q.Y1 - q.Y0));

					var sourceRect = new Rectangle((int)(q.S0 * _size.X),
												(int)(q.T0 * _size.Y),
												(int)((q.S1 - q.S0) * _size.X),
												(int)((q.T1 - q.T0) * _size.Y));

					batch.Draw(glyph.Atlas.Texture,
						destRect,
						sourceRect,
						color,
						depth);
				}

				prevGlyph = glyph;
			}

			return x;
		}

		public float DrawText(IRenderingService batch, float x, float y, StringBuilder str, FssColor[] glyphColors, float depth)
		{
			if (str == null || str.Length == 0) return 0.0f;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var pos = 0;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					++pos;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, false);
				if (glyph == null)
				{
					++pos;
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					q.X0 = (int)(q.X0 * Scale.X);
					q.X1 = (int)(q.X1 * Scale.X);
					q.Y0 = (int)(q.Y0 * Scale.Y);
					q.Y1 = (int)(q.Y1 * Scale.Y);

					var destRect = new Rectangle((int)(x + q.X0),
												(int)(y + q.Y0),
												(int)(q.X1 - q.X0),
												(int)(q.Y1 - q.Y0));

					var sourceRect = new Rectangle((int)(q.S0 * _size.X),
												(int)(q.T0 * _size.Y),
												(int)((q.S1 - q.S0) * _size.X),
												(int)((q.T1 - q.T0) * _size.Y));

					batch.Draw(glyph.Atlas.Texture,
						destRect,
						sourceRect,
						glyphColors[pos],
						depth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return x;
		}

		public float TextBounds(float x, float y, string str, ref Bounds bounds)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref x, ref y, ref q);
				if (q.X0 < minx)
					minx = q.X0;
				if (x > maxx)
					maxx = x;
				if (q.Y0 < miny)
					miny = q.Y0;
				if (q.Y1 > maxy)
					maxy = q.Y1;

				prevGlyph = glyph;
			}

			maxx += StrokeAmount * 2;

			float advance = x - startx;
			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;

			return advance;
		}

		public float TextBounds(float x, float y, StringBuilder str, ref Bounds bounds)
		{
			if (str == null || str.Length == 0) return 0.0f;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref x, ref y, ref q);
				if (q.X0 < minx)
					minx = q.X0;
				if (x > maxx)
					maxx = x;
				if (q.Y0 < miny)
					miny = q.Y0;
				if (q.Y1 > maxy)
					maxy = q.Y1;

				prevGlyph = glyph;
			}

			maxx += StrokeAmount * 2;

			float advance = x - startx;
			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;

			return advance;
		}

		public List<Rectangle> GetGlyphRects(float x, float y, string str)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (string.IsNullOrEmpty(str)) return Rects;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
			y += ascent;

			float startx = x;

			FontGlyph prevGlyph = null;
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref x, ref y, ref q);

				Rects.Add(new Rectangle((int)q.X0, (int)q.Y0, (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0)));
				prevGlyph = glyph;
			}

			return Rects;
		}

		public List<Rectangle> GetGlyphRects(float x, float y, StringBuilder str)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (str == null || str.Length == 0) return Rects;

			GlyphCollection glyphs;
			float ascent, lineHeight;
			PreDraw(str, out glyphs, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(glyphs, codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, Spacing, ref x, ref y, ref q);

				Rects.Add(new Rectangle((int)q.X0, (int)q.Y0, (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0)));
				prevGlyph = glyph;
			}

			return Rects;
		}

		bool StringBuilderIsSurrogatePair(StringBuilder sb, int index)
		{
			if (index + 1 < sb.Length)
				return char.IsSurrogatePair(sb[index], sb[index + 1]);
			return false;
		}

		int StringBuilderConvertToUtf32(StringBuilder sb, int index)
		{
			if (!char.IsHighSurrogate(sb[index]))
				return sb[index];

			return char.ConvertToUtf32(sb[index], sb[index + 1]);
		}

		public void Reset(int width, int height)
		{
			Atlases.Clear();

			_glyphs.Clear();

			if (width == _size.X && height == _size.Y)
				return;

			_size = new Point(width, height);
			_itw = 1.0f / _size.X;
			_ith = 1.0f / _size.Y;
		}

		public void Reset()
		{
			Reset(_size.X, _size.Y);
		}

		FontGlyph GetGlyphWithoutBitmap(GlyphCollection collection, int codepoint)
		{
			FontGlyph glyph = null;
			if (collection.Glyphs.TryGetValue(codepoint, out glyph))
			{
				return glyph;
			}

			Font font;
			var g = GetCodepointIndex(codepoint, out font);
			if (g == 0)
			{
				return null;
			}

			int advance = 0, lsb = 0, x0 = 0, y0 = 0, x1 = 0, y1 = 0;
			font.BuildGlyphBitmap(g, font.Scale, ref advance, ref lsb, ref x0, ref y0, ref x1, ref y1);

			var pad = Math.Max(FontGlyph.PadFromBlur(BlurAmount), FontGlyph.PadFromBlur(StrokeAmount));
			var gw = x1 - x0 + pad * 2;
			var gh = y1 - y0 + pad * 2;
			var offset = FontGlyph.PadFromBlur(BlurAmount);

			glyph = new FontGlyph
			{
				Font = font,
				Codepoint = codepoint,
				Size = FontSize,
				Index = g,
				Bounds = new Rectangle(0, 0, gw, gh),
				XAdvance = (int)(font.Scale * advance * 10.0f),
				XOffset = x0 - offset,
				YOffset = y0 - offset
			};

			collection.Glyphs[codepoint] = glyph;

			return glyph;
		}

		FontGlyph GetGlyphInternal(GlyphCollection glyphs, int codepoint, bool withoutBitmap)
		{
			var glyph = GetGlyphWithoutBitmap(glyphs, codepoint);
			if (glyph == null)
			{
				return null;
			}

			if (withoutBitmap || glyph.Atlas != null)
				return glyph;

			var currentAtlas = CurrentAtlas;
			int gx = 0, gy = 0;
			var gw = glyph.Bounds.Width;
			var gh = glyph.Bounds.Height;
			if (!currentAtlas.AddRect(gw, gh, ref gx, ref gy))
			{
				CurrentAtlasFull?.Invoke(this, EventArgs.Empty);

				// This code will force creation of new atlas
				_currentAtlas = null;
				currentAtlas = CurrentAtlas;

				// Try to add again
				if (!currentAtlas.AddRect(gw, gh, ref gx, ref gy))
				{
					throw new Exception(string.Format("Could not add rect to the newly created atlas. gw={0}, gh={1}", gw, gh));
				}
			}

			glyph.Bounds.X = gx;
			glyph.Bounds.Y = gy;

			currentAtlas.RenderGlyph(_textureCreationService, _fontRasterizationService, glyph, BlurAmount, StrokeAmount);

			glyph.Atlas = currentAtlas;

			return glyph;
		}

		FontGlyph GetGlyph(GlyphCollection glyphs, int codepoint, bool withoutBitmap)
		{
			var result = GetGlyphInternal(glyphs, codepoint, withoutBitmap);
			if (result == null && DefaultCharacter != null)
			{
				result = GetGlyphInternal(glyphs, DefaultCharacter.Value, withoutBitmap);
			}

			return result;
		}

		private void GetQuad(FontGlyph glyph, FontGlyph prevGlyph, float spacing, ref float x, ref float y, ref FontGlyphSquad q)
		{
			if (prevGlyph != null)
			{
				float adv = 0;
				if (UseKernings && glyph.Font == prevGlyph.Font)
				{
					adv = prevGlyph.Font.GetGlyphKernAdvance(prevGlyph.Index, glyph.Index) * glyph.Font.Scale;
				}

				x += (int)(adv + spacing + 0.5f);
			}

			float rx = x + glyph.XOffset;
			float ry = y + glyph.YOffset;
			q.X0 = rx;
			q.Y0 = ry;
			q.X1 = rx + glyph.Bounds.Width;
			q.Y1 = ry + glyph.Bounds.Height;
			q.S0 = glyph.Bounds.X * _itw;
			q.T0 = glyph.Bounds.Y * _ith;
			q.S1 = glyph.Bounds.Right * _itw;
			q.T1 = glyph.Bounds.Bottom * _ith;

			x += (int)(glyph.XAdvance / 10.0f + 0.5f);
		}
	}
}

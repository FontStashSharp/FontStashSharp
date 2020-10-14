using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#else
using System.Drawing;
using Vector2 = System.Drawing.PointF;
#endif

namespace FontStashSharp
{
	public class DynamicSpriteFont
	{
		internal static readonly Vector2 DefaultScale = new Vector2(1.0f, 1.0f);
		private readonly Int32Map<FontGlyph> _glyphs = new Int32Map<FontGlyph>();

		public FontSystem FontSystem { get; private set; }

		public int FontSize { get; private set; }

		internal DynamicSpriteFont(FontSystem system, int size)
		{
			if (system == null)
			{
				throw new ArgumentNullException(nameof(system));
			}

			FontSystem = system;
			FontSize = size;
		}

		private FontGlyph GetGlyphWithoutBitmap(int codepoint)
		{
			FontGlyph glyph = null;
			if (_glyphs.TryGetValue(codepoint, out glyph))
			{
				return glyph;
			}

			IFontSource font;
			var g = FontSystem.GetCodepointIndex(codepoint, out font);
			if (g == null)
			{
				return null;
			}

			int advance = 0, x0 = 0, y0 = 0, x1 = 0, y1 = 0;
			font.GetGlyphMetrics(g.Value, FontSize, out advance, out x0, out y0, out x1, out y1);

			var pad = Math.Max(FontGlyph.PadFromBlur(FontSystem.BlurAmount), FontGlyph.PadFromBlur(FontSystem.StrokeAmount));
			var gw = x1 - x0 + pad * 2;
			var gh = y1 - y0 + pad * 2;
			var offset = FontGlyph.PadFromBlur(FontSystem.BlurAmount);

			glyph = new FontGlyph
			{
				Codepoint = codepoint,
				Id = g.Value,
				Size = FontSize,
				Font = font,
				Bounds = new Rectangle(0, 0, gw, gh),
				XAdvance = advance,
				XOffset = x0 - offset,
				YOffset = y0 - offset
			};

			_glyphs[codepoint] = glyph;

			return glyph;
		}

		private FontGlyph GetGlyphInternal(int codepoint, bool withoutBitmap)
		{
			var glyph = GetGlyphWithoutBitmap(codepoint);
			if (glyph == null)
			{
				return null;
			}

			if (withoutBitmap || glyph.Atlas != null)
				return glyph;

			FontSystem.RenderGlyphOnAtlas(glyph);

			return glyph;
		}

		FontGlyph GetGlyph(int codepoint, bool withoutBitmap)
		{
			var result = GetGlyphInternal(codepoint, withoutBitmap);
			if (result == null && FontSystem.DefaultCharacter != null)
			{
				result = GetGlyphInternal(FontSystem.DefaultCharacter.Value, withoutBitmap);
			}

			return result;
		}

		private void PreDraw(string str, out float ascent, out float lineHeight)
		{
			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				float descent;
				glyph.Font.GetMetricsForSize(FontSize, out ascent, out descent, out lineHeight);
				lineHeight += FontSystem.LineSpacing;
				break;
			}
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color color, Vector2 scale, float depth = 0.0f)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var destRect = new Rectangle((int)(x + q.X0), (int)(y + q.Y0), (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0));
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

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

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color color, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, color, DefaultScale, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					++pos;
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var destRect = new Rectangle((int)(x + q.X0), (int)(y + q.Y0), (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0));
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

					batch.Draw(glyph.Atlas.Texture,
						destRect,
						sourceRect,
						colors[pos],
						depth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return x;
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color[] colors, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, colors, DefaultScale, depth);
		}

		private void PreDraw(StringBuilder str, out float ascent, out float lineHeight)
		{
			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				float descent;
				glyph.Font.GetMetricsForSize(FontSize, out ascent, out descent, out lineHeight);
				lineHeight += FontSystem.LineSpacing;
				break;
			}
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color color, Vector2 scale, float depth = 0.0f)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var destRect = new Rectangle((int)(x + q.X0), (int)(y + q.Y0), (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0));
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

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

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color color, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, color, DefaultScale, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color[] glyphColors, Vector2 scale, float depth = 0.0f)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					++pos;
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var destRect = new Rectangle((int)(x + q.X0), (int)(y + q.Y0), (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0));
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

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

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color[] colors, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, colors, DefaultScale, depth);
		}

		public float TextBounds(float x, float y, string str, ref Bounds bounds, Vector2 scale)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);
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

			maxx += FontSystem.StrokeAmount * 2;

			float advance = x - startx;
			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;

			return advance;
		}

		public float TextBounds(float x, float y, string str, ref Bounds bounds)
		{
			return TextBounds(x, y, str, ref bounds, DefaultScale);
		}

		public float TextBounds(float x, float y, StringBuilder str, ref Bounds bounds, Vector2 scale)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);
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

			maxx += FontSystem.StrokeAmount * 2;

			float advance = x - startx;
			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;

			return advance;
		}

		public float TextBounds(float x, float y, StringBuilder str, ref Bounds bounds)
		{
			return TextBounds(x, y, str, ref bounds, DefaultScale);
		}

		public List<Rectangle> GetGlyphRects(float x, float y, string str, Vector2 scale)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (string.IsNullOrEmpty(str)) return Rects;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);

				Rects.Add(new Rectangle((int)q.X0, (int)q.Y0, (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0)));
				prevGlyph = glyph;
			}

			return Rects;
		}

		public List<Rectangle> GetGlyphRects(float x, float y, string str)
		{
			return GetGlyphRects(x, y, str, DefaultScale);
		}

		public List<Rectangle> GetGlyphRects(float x, float y, StringBuilder str, Vector2 scale)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (str == null || str.Length == 0) return Rects;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);

				Rects.Add(new Rectangle((int)q.X0, (int)q.Y0, (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0)));
				prevGlyph = glyph;
			}

			return Rects;
		}

		public List<Rectangle> GetGlyphRects(float x, float y, StringBuilder str)
		{
			return GetGlyphRects(x, y, str, DefaultScale);
		}

		public Vector2 MeasureString(string text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(0, 0, text, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(string text)
		{
			return MeasureString(text, DefaultScale);
		}

		public Vector2 MeasureString(StringBuilder text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(0, 0, text, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			return MeasureString(text, DefaultScale);
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

		private void GetQuad(FontGlyph glyph, FontGlyph prevGlyph, Vector2 scale, ref float x, ref float y, ref FontGlyphSquad q)
		{
			if (prevGlyph != null)
			{
				float adv = 0;
				if (FontSystem.UseKernings && glyph.Font == prevGlyph.Font)
				{
					adv = prevGlyph.Font.GetGlyphKernAdvance(prevGlyph.Id, glyph.Id, glyph.Size) * scale.X;
				}

				x += (int)(adv + FontSystem.CharacterSpacing + 0.5f);
			}

			float rx = x + glyph.XOffset;
			float ry = y + glyph.YOffset;
			q.X0 = rx * scale.X;
			q.Y0 = ry * scale.Y;
			q.X1 = (rx + glyph.Bounds.Width) * scale.X;
			q.Y1 = (ry + glyph.Bounds.Height) * scale.Y;
			q.S0 = glyph.Bounds.X;
			q.T0 = glyph.Bounds.Y;
			q.S1 = glyph.Bounds.Right;
			q.T1 = glyph.Bounds.Bottom;

			x += glyph.XAdvance;
		}
	}
}
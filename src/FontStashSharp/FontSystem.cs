using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using PointF = Microsoft.Xna.Framework.Vector2;
#else
using System.Drawing;
#endif

namespace FontStashSharp
{
	public class FontSystem : IDisposable
	{
		readonly List<IFont> _fonts = new List<IFont>();
		readonly Int32Map<DynamicSpriteFont> _dynamicFonts = new Int32Map<DynamicSpriteFont>();

		readonly IFontLoader _fontLoader;
		readonly ITexture2DCreator _textureCreator;

		FontAtlas _currentAtlas;
		Point _size;

		public readonly int BlurAmount;
		public readonly int StrokeAmount;
		public float CharacterSpacing = 0f;
		public float LineSpacing = 0f;
		public PointF Scale = new PointF(1.0f, 1.0f);
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

		public FontSystem(IFontLoader fontLoader, ITexture2DCreator textureCreator, int width, int height, int blurAmount = 0, int strokeAmount = 0)
		{
			if (fontLoader == null)
			{
				throw new ArgumentNullException(nameof(fontLoader));
			}

			if (textureCreator == null)
			{
				throw new ArgumentNullException(nameof(textureCreator));
			}

			_fontLoader = fontLoader;
			_textureCreator = textureCreator;

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

			ClearState();
		}

		public void Dispose()
		{
			if (_fonts != null)
			{
				foreach (var font in _fonts)
					font.Dispose();
				_fonts.Clear();
			}

			Atlases?.Clear();
			_currentAtlas = null;
			_dynamicFonts.Clear();
		}

		public void ClearState()
		{
			CharacterSpacing = 0;
		}

		public void AddFont(byte[] data)
		{
			var font = _fontLoader.Load(data);
			_fonts.Add(font);
		}

		public DynamicSpriteFont GetFontBySize(int fontSize)
		{
			DynamicSpriteFont result;
			if (_dynamicFonts.TryGetValue(fontSize, out result))
			{
				return result;
			}

			result = new DynamicSpriteFont(this, fontSize);
			_dynamicFonts[fontSize] = result;
			return result;
		}

		public void Reset(int width, int height)
		{
			Atlases.Clear();
			_dynamicFonts.Clear();

			if (width == _size.X && height == _size.Y)
				return;

			_size = new Point(width, height);
		}

		public void Reset()
		{
			Reset(_size.X, _size.Y);
		}

		internal int? GetCodepointIndex(int codepoint, out IFont font)
		{
			font = null;

			var g = default(int?);
			foreach (var f in _fonts)
			{
				g = f.GetGlyphId(codepoint);
				if (g != null)
				{
					font = f;
					break;
				}
			}

			return g;
		}

		internal void RenderGlyphOnAtlas(FontGlyph glyph)
		{
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

			currentAtlas.RenderGlyph(_textureCreator, glyph, BlurAmount, StrokeAmount);

			glyph.Atlas = currentAtlas;
		}

		internal void GetQuad(FontGlyph glyph, FontGlyph prevGlyph, ref float x, ref float y, ref FontGlyphSquad q)
		{
			if (prevGlyph != null)
			{
				float adv = 0;
				if (UseKernings && glyph.Font == prevGlyph.Font)
				{
					adv = prevGlyph.Font.GetGlyphKernAdvance(prevGlyph.Id, glyph.Id, glyph.Size);
				}

				x += (int)(adv + CharacterSpacing + 0.5f);
			}

			float rx = x + glyph.XOffset;
			float ry = y + glyph.YOffset;
			q.X0 = rx * Scale.X;
			q.Y0 = ry * Scale.Y;
			q.X1 = (rx + glyph.Bounds.Width) * Scale.X;
			q.Y1 = (ry + glyph.Bounds.Height) * Scale.Y;
			q.S0 = glyph.Bounds.X;
			q.T0 = glyph.Bounds.Y;
			q.S1 = glyph.Bounds.Right;
			q.T1 = glyph.Bounds.Bottom;

			x += glyph.XAdvance;
		}
	}
}

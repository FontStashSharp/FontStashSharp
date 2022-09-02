using System;
using System.Collections.Generic;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp.RichText
{
	public class TextChunk : BaseChunk
	{
		protected string _text;
		protected readonly SpriteFontBase _font;
		protected Point _size;

		public List<GlyphInfo> Glyphs { get; } = new List<GlyphInfo>();

		public int Count { get { return _text.Length(); } }
		public string Text { get { return _text; } }
		public override Point Size { get { return _size; } }

		public SpriteFontBase Font => _font;

		public TextChunk(SpriteFontBase font, string text, Point size, Point? startPos)
		{
			if (font == null)
			{
				throw new ArgumentNullException("font");
			}

			_font = font;
			_text = text;
			_size = size;

			if (startPos != null)
			{
				CalculateGlyphs(startPos.Value);
			}
		}

		private void CalculateGlyphs(Point startPos)
		{
			if (string.IsNullOrEmpty(_text))
			{
				return;
			}

			var rects = _font.GetGlyphRects(_text, Vector2.Zero);

			Glyphs.Clear();
			for (var i = 0; i < rects.Count; ++i)
			{
				var rect = rects[i];
				rect.Offset(startPos);
				Glyphs.Add(new GlyphInfo
				{
					TextChunk = this,
					Character = _text[i],
					Index = i,
					Bounds = rect,
					LineTop = startPos.Y
				});
			}
		}

		public GlyphInfo GetGlyphInfoByIndex(int index)
		{
			if (string.IsNullOrEmpty(_text) || index < 0 || index >= _text.Length)
			{
				return null;
			}

			return Glyphs[index];
		}

		public int? GetGlyphIndexByX(int x)
		{
			if (Glyphs.Count == 0 || x < 0)
			{
				return null;
			}

			var i = 0;
			for (; i < Glyphs.Count; ++i)
			{
				var glyph = Glyphs[i];
				var right = glyph.Bounds.Right;
				if (i < Glyphs.Count - 1)
				{
					right = Glyphs[i + 1].Bounds.X;
				}

				if (glyph.Bounds.X <= x && x <= right)
				{
					if (x - glyph.Bounds.X >= glyph.Bounds.Width / 2)
					{
						++i;
					}

					break;
				}
			}

			if (i - 1 >= 0 && i - 1 < Glyphs.Count && Glyphs[i - 1].Character == '\n')
			{
				--i;
			}

			return i;
		}

		public override void Draw(FSRenderContext context, Vector2 position, Color color)
		{
			context.DrawText(Text, Font, position, color);
		}
	}
}
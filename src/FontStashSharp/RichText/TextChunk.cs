using System;
using System.Collections.Generic;

#if MONOGAME || FNA || KNI || XNA
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
	/// <summary>
	/// Represents a chunk of text in rich text layout with associated font and styling.
	/// </summary>
	public class TextChunk : BaseChunk
	{
		internal Point _size;

		/// <summary>
		/// Gets the list of glyphs in this text chunk.
		/// </summary>
		public List<TextChunkGlyph> Glyphs { get; } = new List<TextChunkGlyph>();

		/// <summary>
		/// Gets the number of characters in this text chunk.
		/// </summary>
		public int Count { get; }
		/// <summary>
		/// Gets or sets the text content of this chunk.
		/// </summary>
		public string Text { get; internal set; }
		/// <summary>
		/// Gets the size of this text chunk in pixels.
		/// </summary>
		public override Point Size => _size;

		/// <summary>
		/// Gets the sprite font used to render this text chunk.
		/// </summary>
		public SpriteFontBase Font { get; }
		/// <summary>
		/// Gets or sets the text style (underline, strikethrough) for this chunk.
		/// </summary>
		public TextStyle Style { get; set; }
		/// <summary>
		/// Gets or sets the visual effect (blur, stroke) to apply to this chunk.
		/// </summary>
		public FontSystemEffect Effect { get; set; }
		/// <summary>
		/// Gets or sets the strength of the applied effect.
		/// </summary>
		public int EffectAmount { get; set; }

		/// <summary>
		/// Initializes a new instance of the TextChunk class.
		/// </summary>
		/// <param name="font">The sprite font to use for rendering.</param>
		/// <param name="text">The text content.</param>
		/// <param name="size">The size of the chunk in pixels.</param>
		/// <param name="startPos">The starting position for glyph calculation, or null to skip glyph calculation.</param>
		public TextChunk(SpriteFontBase font, string text, Point size, Point? startPos)
		{
			if (font == null)
			{
				throw new ArgumentNullException("font");
			}

			Font = font;
			Text = text;
			_size = size;
			Count = TextSource.CalculateLength(text);

			if (startPos != null)
			{
				CalculateGlyphs(startPos.Value);
			}
		}

		private void CalculateGlyphs(Point startPos)
		{
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}

			var glyphs = Font.GetGlyphs(Text, Vector2.Zero);

			Glyphs.Clear();
			for (var i = 0; i < glyphs.Count; ++i)
			{
				var glyph = glyphs[i];
				var bounds = glyph.Bounds;
				bounds.Offset(startPos);
				Glyphs.Add(new TextChunkGlyph
				{
					TextChunk = this,
					LineTop = startPos.Y,
					Index = glyph.Index,
					Codepoint = glyph.Codepoint,
					Bounds = bounds,
					XAdvance = glyph.XAdvance
				});
			}
		}

		/// <summary>
		/// Gets the glyph information at the specified index.
		/// </summary>
		/// <param name="index">The index of the glyph to retrieve</param>
		/// <returns>The glyph information at the specified index</returns>
		public TextChunkGlyph? GetGlyphInfoByIndex(int index)
		{
			if (string.IsNullOrEmpty(Text) || index < 0 || index >= Text.Length)
			{
				return null;
			}

			return Glyphs[index];
		}

		/// <summary>
		/// Gets the glyph index at the specified X coordinate.
		/// </summary>
		/// <param name="x">The X coordinate to search for</param>
		/// <returns>The glyph index at the specified X coordinate</returns>
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
				var width = glyph.XAdvance;
				var right = glyph.Bounds.X + width;

				if (glyph.Bounds.X <= x && x <= right)
				{
					if (x - glyph.Bounds.X >= width / 2)
					{
						++i;
					}

					break;
				}
			}

			if (i - 1 >= 0 && i - 1 < Glyphs.Count && Glyphs[i - 1].Codepoint == '\n')
			{
				--i;
			}

			return i;
		}

		/// <summary>
		/// Draws the text chunk using the specified rendering context.
		/// </summary>
		/// <param name="context">The rendering context to use</param>
		/// <param name="position">The position to draw at</param>
		/// <param name="color">The color to render the text in</param>
		public override void Draw(FSRenderContext context, Vector2 position, Color color)
		{
			context.DrawText(Text, Font, position, color, Style, Effect, EffectAmount);
		}
	}
}
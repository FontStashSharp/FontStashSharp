using System.Collections.Generic;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Represents a single line of text in rich text layout.
	/// </summary>
	public class TextLine
	{
		/// <summary>
		/// Gets the number of glyphs in this line.
		/// </summary>
		public int Count { get; internal set; }

		/// <summary>
		/// Gets or sets the size of the line in pixels (width and height).
		/// </summary>
		public Point Size;

		/// <summary>
		/// Gets the index of this line in the document.
		/// </summary>
		public int LineIndex { get; internal set; }

		/// <summary>
		/// Gets the starting index of the text content in this line.
		/// </summary>
		public int TextStartIndex { get; internal set; }

		/// <summary>
		/// Gets the list of chunks (text and image) that compose this line.
		/// </summary>
		public List<BaseChunk> Chunks { get; } = new List<BaseChunk>();

		/// <summary>
		/// Gets the glyph information at the specified index within this line.
		/// </summary>
		/// <param name="index">The index of the glyph to retrieve.</param>
		/// <returns>The glyph information, or null if the index is out of range.</returns>
		public TextChunkGlyph? GetGlyphInfoByIndex(int index)
		{
			foreach (var chunk in Chunks)
			{
				var textChunk = chunk as TextChunk;
				if (textChunk == null) continue;

				if (index >= textChunk.Count)
				{
					index -= textChunk.Count;
				}
				else
				{
					return textChunk.GetGlyphInfoByIndex(index);
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the glyph index at the specified X coordinate within this line.
		/// </summary>
		/// <param name="startX">The X coordinate within the line.</param>
		/// <returns>The glyph index at the specified position, or null if no glyph is found.</returns>
		public int? GetGlyphIndexByX(int startX)
		{
			if (Chunks.Count == 0)
			{
				return null;
			}

			var x = startX;
			for (var i = 0; i < Chunks.Count; ++i)
			{
				var chunk = (TextChunk)Chunks[i];

				if (x >= chunk.Size.X)
				{
					x -= chunk.Size.X;
				}
				else
				{
					if (chunk.Glyphs.Count > 0 && x < chunk.Glyphs[0].Bounds.X)
					{
						// Before first glyph
						return 0;
					}

					return chunk.GetGlyphIndexByX(x);
				}
			}

			// Use last chunk
			x = startX;
			return ((TextChunk)Chunks[Chunks.Count - 1]).GetGlyphIndexByX(startX);
		}
	}
}

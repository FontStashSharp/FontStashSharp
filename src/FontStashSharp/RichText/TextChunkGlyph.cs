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
	/// Represents a glyph within a text chunk in rich text layout.
	/// </summary>
	public struct TextChunkGlyph
	{
		/// <summary>
		/// The index of the glyph in the font.
		/// </summary>
		public int Index;
		/// <summary>
		/// The Unicode codepoint of the character.
		/// </summary>
		public int Codepoint;
		/// <summary>
		/// The bounding rectangle of the glyph.
		/// </summary>
		public Rectangle Bounds;
		/// <summary>
		/// The horizontal advance width in pixels.
		/// </summary>
		public int XAdvance;
		/// <summary>
		/// The Y position of the top of the line containing this glyph.
		/// </summary>
		public int LineTop;
		/// <summary>
		/// The text chunk containing this glyph.
		/// </summary>
		public TextChunk TextChunk;
	}
}

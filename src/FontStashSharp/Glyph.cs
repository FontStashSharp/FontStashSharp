#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Represents a character glyph with its bounds and layout information.
	/// </summary>
	public struct Glyph
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
		/// The bounding rectangle of the glyph in pixel coordinates.
		/// </summary>
		public Rectangle Bounds;
		/// <summary>
		/// The horizontal advance width in pixels.
		/// </summary>
		public int XAdvance;
	}
}

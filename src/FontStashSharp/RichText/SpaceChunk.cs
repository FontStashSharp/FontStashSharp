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
	/// Represents a space or gap in rich text layout.
	/// </summary>
	public class SpaceChunk : BaseChunk
	{
		private readonly int _width;

		/// <summary>
		/// Gets the size of this space chunk.
		/// </summary>
		public override Point Size => new Point(_width, 0);

		/// <summary>
		/// Initializes a new instance of the SpaceChunk class with the specified width.
		/// </summary>
		/// <param name="width">The width of the space in pixels.</param>
		public SpaceChunk(int width)
		{
			_width = width;
		}

		/// <summary>
		/// Draws this space chunk (which is invisible).
		/// </summary>
		/// <param name="context">The rendering context.</param>
		/// <param name="position">The position to draw at.</param>
		/// <param name="color">The color to apply (ignored for spaces).</param>
		public override void Draw(IFSRenderContext context, Vector2 position, Color color)
		{
		}
	}
}

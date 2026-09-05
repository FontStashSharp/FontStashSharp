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
	/// Represents a renderable element in rich text layout.
	/// </summary>
	public interface IRenderable
	{
		/// <summary>
		/// Gets the size of the renderable element in pixels.
		/// </summary>
		Point Size { get; }

		/// <summary>
		/// Draws the renderable element at the specified position with the specified color.
		/// </summary>
		/// <param name="context">The rendering context to use for drawing.</param>
		/// <param name="position">The position to draw the element at.</param>
		/// <param name="color">The color to apply to the rendered element.</param>
		void Draw(FSRenderContext context, Vector2 position, Color color);
	}
}

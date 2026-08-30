using System;

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
	/// Represents an image or texture chunk in rich text layout.
	/// </summary>
	public class ImageChunk : BaseChunk
	{
		private readonly IRenderable _renderable;

		/// <summary>
		/// Gets the size of the image chunk.
		/// </summary>
		public override Point Size => _renderable.Size;

		/// <summary>
		/// Initializes a new instance of the ImageChunk class with a renderable object.
		/// </summary>
		/// <param name="renderable">The renderable object representing the image.</param>
		/// <exception cref="ArgumentNullException">Thrown when renderable is null.</exception>
		public ImageChunk(IRenderable renderable)
		{
			if (renderable == null)
			{
				throw new ArgumentNullException(nameof(renderable));
			}

			_renderable = renderable;
		}

		/// <summary>
		/// Draws the image chunk at the specified position.
		/// </summary>
		/// <param name="context">The rendering context.</param>
		/// <param name="position">The position to draw at.</param>
		/// <param name="color">The color to apply.</param>
		public override void Draw(FSRenderContext context, Vector2 position, Color color)
		{
			_renderable.Draw(context, position, color);
		}
	}
}

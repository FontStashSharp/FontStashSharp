using System;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Numerics;
using System.Drawing;
using Texture2D = System.Object;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Represents a renderable image or texture fragment in rich text.
	/// </summary>
	public class TextureFragment : IRenderable
	{
		/// <summary>
		/// Gets the texture containing the image.
		/// </summary>
		public Texture2D Texture { get; private set; }
		/// <summary>
		/// Gets the rectangular region within the texture.
		/// </summary>
		public Rectangle Region { get; private set; }

		/// <summary>
		/// Gets the scaled size of the texture fragment in pixels.
		/// </summary>
		public Point Size
		{
			get
			{
				return new Point((int)(Region.Width * Scale.X + 0.5f), (int)(Region.Height * Scale.Y + 0.5f));
			}
		}

		/// <summary>
		/// Gets or sets the scale factors for the width and height of the texture fragment.
		/// </summary>
		public Vector2 Scale = Vector2.One;

		/// <summary>
		/// Initializes a new instance of the TextureFragment class with a texture and region.
		/// </summary>
		/// <param name="texture">The texture to render.</param>
		/// <param name="region">The rectangular region within the texture to render.</param>
		/// <exception cref="ArgumentNullException">Thrown when texture is null.</exception>
		public TextureFragment(Texture2D texture, Rectangle region)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			Texture = texture;
			Region = region;
		}

#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Initializes a new instance of the TextureFragment class with an entire texture.
		/// </summary>
		/// <param name="texture">The texture to render.</param>
		public TextureFragment(Texture2D texture) :
			this(texture, new Rectangle(0, 0, texture.Width, texture.Height))
		{
		}
#endif

		/// <summary>
		/// Draws the texture fragment at the specified position.
		/// </summary>
		/// <param name="context">The rendering context.</param>
		/// <param name="position">The position to draw at.</param>
		/// <param name="color">The color to apply (this parameter is ignored; white is always used).</param>
		public void Draw(FSRenderContext context, Vector2 position, Color color)
		{
			context.DrawImage(Texture, Region, position, Scale, Color.White);
		}
	}
}
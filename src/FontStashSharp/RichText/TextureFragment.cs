using System;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp.RichText
{
	public class TextureFragment : IRenderable
	{
		public Texture2D Texture { get; private set; }
		public Rectangle Region { get; private set; }

		public Point Size => new Point(Region.Width, Region.Height);

		public TextureFragment(Texture2D texture, Rectangle region)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			Texture = texture;
			Region = region;
		}

#if MONOGAME || FNA || STRIDE
		public TextureFragment(Texture2D texture): 
			this(texture, new Rectangle(0, 0, texture.Width, texture.Height))
		{
		}
#endif

		public void Draw(IFontStashRenderer renderer, Vector2 position, Color color, Vector2 scale, float rotation, float layerDepth)
		{
			renderer.Draw(Texture, position, Region, color, rotation, scale, layerDepth);
		}
	}
}
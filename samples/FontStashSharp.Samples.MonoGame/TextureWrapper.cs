using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp.Samples.MonoGame
{
	class TextureWrapper: ITexture
	{
		public Texture2D Texture { get; private set; }

		public TextureWrapper(Texture2D texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			Texture = texture;
		}

		public void Dispose()
		{
			if (Texture != null)
			{
				Texture.Dispose();
				Texture = null;
			}
		}

		public void SetData(System.Drawing.Rectangle bounds, FssColor[] data)
		{
			Texture.SetData(0, 0, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), data, 0, bounds.Width * bounds.Height);
		}
	}
}

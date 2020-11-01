using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp
{
	class Texture2DWrapper: ITexture2D
	{
		public Texture2D Texture { get; private set; }

		public Texture2DWrapper(Texture2D texture)
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

		public void SetData(System.Drawing.Rectangle bounds, byte[] data)
		{
			Texture.SetData(0, 0, new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), data, 0, bounds.Width * bounds.Height * 4);
		}
	}
}

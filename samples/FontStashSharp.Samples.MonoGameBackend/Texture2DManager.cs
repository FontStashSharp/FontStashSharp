using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;

namespace FontStashSharp
{
	class Texture2DManager : ITexture2DManager
	{
		readonly GraphicsDevice _device;

		public Texture2DManager(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			_device = device;
		}

		public object CreateTexture(int width, int height)
		{
			return new Texture2D(_device, width, height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{
			var mgTexture = (Texture2D)texture;
			mgTexture.SetData(0, 0, new Microsoft.Xna.Framework.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), 
				data, 0, bounds.Width * bounds.Height * 4);
		}
	}
}

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp.Samples.MonoGame
{
	class TextureCreator : ITextureCreator
	{
		readonly GraphicsDevice _device;

		public TextureCreator(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			_device = device;
		}

		public ITexture Create(int width, int height)
		{
			var texture2d = new Texture2D(_device, width, height);
			return new TextureWrapper(texture2d);
		}
	}
}

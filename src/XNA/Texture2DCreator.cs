using FontStashSharp.Interfaces;
using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#endif

namespace FontStashSharp
{
	internal class Texture2DCreator : ITexture2DCreator
	{
		readonly GraphicsDevice _device;

		public Texture2DCreator(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			_device = device;
		}

		public ITexture2D Create(int width, int height)
		{
#if MONOGAME || FNA
			var texture2d = new Texture2D(_device, width, height);
#elif STRIDE
			var texture2d = Texture2D.New2D(_device, width, height, false, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource);
#endif

			return new Texture2DWrapper(texture2d);
		}
	}
}

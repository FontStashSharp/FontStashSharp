using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp
{
	class Texture2DCreator : ITexture2DCreator
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
			var texture2d = new Texture2D(_device, width, height);
			return new Texture2DWrapper(texture2d);
		}
	}
}

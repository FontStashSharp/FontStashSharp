using FontStashSharp.Interfaces;
using Silk.NET.OpenGL;
using System;
using System.Drawing;
using TrippyGL;

namespace FontStashSharp.Platform
{
	internal class Texture2DManager : ITexture2DManager
	{
		public GraphicsDevice GraphicsDevice { get; }

		public Texture2DManager(GraphicsDevice device)
		{
			GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
		}

		public object CreateTexture(int width, int height) => new Texture2D(GraphicsDevice, (uint)width, (uint)height);

		public Point GetTextureSize(object texture)
		{
			var xnaTexture = (Texture2D)texture;

			return new Point((int)xnaTexture.Width, (int)xnaTexture.Height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{
			var xnaTexture = (Texture2D)texture;

			xnaTexture.SetData<byte>(data, bounds.X, bounds.Y, (uint)bounds.Width, (uint)bounds.Height, PixelFormat.Rgba);
		}
	}
}

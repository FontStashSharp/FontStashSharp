using FontStashSharp.Interfaces;
using Silk.NET.OpenGL;
using System;
using System.Drawing;

using Texture = Tutorial.Texture;

namespace FontStashSharp.Platform
{
	internal class Texture2DManager : ITexture2DManager
	{
		public GL Gl { get; }

		public Texture2DManager(GL gl)
		{
			if (gl == null)
				throw new ArgumentNullException(nameof(gl));

			Gl = gl;
		}

		public object CreateTexture(int width, int height) => new Texture(Gl, width, height);

		public Point GetTextureSize(object texture)
		{
			var t = (Texture)texture;
			return new Point(t.Width, t.Height);
		}

		public void SetTextureData(object texture, Rectangle bounds, byte[] data)
		{
			var t = (Texture)texture;
			t.SetData(bounds, data);
		}
	}
}
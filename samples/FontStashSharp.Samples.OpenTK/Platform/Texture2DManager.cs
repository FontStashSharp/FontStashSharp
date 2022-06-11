using FontStashSharp.Interfaces;
using System.Drawing;

namespace FontStashSharp.Platform
{
	internal class Texture2DManager : ITexture2DManager
	{
		public Texture2DManager()
		{
		}

		public object CreateTexture(int width, int height) => new Texture(width, height);

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
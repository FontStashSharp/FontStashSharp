#if MONOGAME || FNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Graphics;
#endif

namespace FontStashSharp
{
	public static class FontSystemFactory
	{
		private static FontSystem InternalCreate(GraphicsDevice graphicsDevice, int textureWidth, int textureHeight, int blurAmount, int strokeAmount, bool premultiplyAlpha)
		{
			var result = new FontSystem(StbTrueTypeSharpFontLoader.Instance, graphicsDevice, textureWidth, textureHeight, blurAmount, strokeAmount, premultiplyAlpha);

			return result;
		}

		public static FontSystem Create(GraphicsDevice graphicsDevice, int textureWidth = 1024, int textureHeight = 1024, bool premultiplyAlpha = true)
		{
			return InternalCreate(graphicsDevice, textureWidth, textureHeight, 0, 0, premultiplyAlpha);
		}

		public static FontSystem CreateBlurry(GraphicsDevice graphicsDevice, int blurAmount, int textureWidth = 1024, int textureHeight = 1024, bool premultiplyAlpha = true)
		{
			return InternalCreate(graphicsDevice, textureWidth, textureHeight, blurAmount, 0, premultiplyAlpha);
		}
		public static FontSystem CreateStroked(GraphicsDevice graphicsDevice, int strokeAmount, int textureWidth = 1024, int textureHeight = 1024, bool premultiplyAlpha = true)
		{
			return InternalCreate(graphicsDevice, textureWidth, textureHeight, 0, strokeAmount, premultiplyAlpha);
		}
	}
}

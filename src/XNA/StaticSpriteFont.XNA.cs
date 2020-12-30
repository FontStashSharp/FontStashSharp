using System;
using System.IO;

#if MONOGAME || FNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Graphics;
#endif

namespace FontStashSharp
{
	partial class StaticSpriteFont
	{
		public static StaticSpriteFont FromBMFont(string data, Func<string, Stream> imageStreamOpener, GraphicsDevice graphicsDevice)
		{
			var textureCreator = new Texture2DCreator(graphicsDevice);

			return FromBMFont(data, imageStreamOpener, textureCreator);
		}
	}
}
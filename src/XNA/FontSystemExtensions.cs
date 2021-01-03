using System.Collections;
using System.Collections.Generic;

#if MONOGAME || FNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Texture2D = Stride.Graphics.Texture;
#endif

namespace FontStashSharp
{
	public static class FontSystemExtensions
	{
		private struct TextureEnumerator : IEnumerable<Texture2D>
		{
			readonly FontSystem _font;

			public TextureEnumerator(FontSystem font)
			{
				_font = font;
			}

			public IEnumerator<Texture2D> GetEnumerator()
			{
				foreach (var atlas in _font.Atlases)
				{
					var xnaTexture = atlas.Texture;
					yield return xnaTexture;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public static IEnumerable<Texture2D> EnumerateTextures(this FontSystem fontSystem)
		{
			return new TextureEnumerator(fontSystem);
		}
	}
}

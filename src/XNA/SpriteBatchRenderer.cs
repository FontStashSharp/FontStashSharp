using FontStashSharp.Interfaces;
using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#endif

namespace FontStashSharp
{
	internal class SpriteBatchRenderer : IFontStashRenderer
	{
		public static readonly SpriteBatchRenderer Instance = new SpriteBatchRenderer();

		private SpriteBatch _batch;

		public SpriteBatch Batch
		{
			get
			{
				return _batch;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_batch = value;
			}
		}

		private SpriteBatchRenderer()
		{
		}

		public void Draw(ITexture2D texture, Rectangle dest, Rectangle source, Color color, float depth)
		{
			var textureWrapper = (Texture2DWrapper)texture;

#if MONOGAME || FNA
			_batch.Draw(textureWrapper.Texture,
				dest,
				source,
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				depth);
#elif STRIDE
			_batch.Draw(textureWrapper.Texture,
				dest,
				source,
				color,
				0f,
				Vector2.Zero,
				SpriteEffects.None,
				ImageOrientation.AsIs,
				depth);
#endif
		}
	}
}

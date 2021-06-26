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

		public GraphicsDevice GraphicsDevice => _batch.GraphicsDevice;

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

		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, 
			float rotation, Vector2 origin, Vector2 scale, float depth)
		{
#if MONOGAME || FNA
			_batch.Draw(texture,
				position,
				sourceRectangle,
				color,
				rotation,
				origin,
				scale,
				SpriteEffects.None,
				depth);
#elif STRIDE
			_batch.Draw(texture,
				position,
				sourceRectangle,
				color,
				rotation,
				origin,
				scale,
				SpriteEffects.None,
				ImageOrientation.AsIs,
				depth);
#endif
		}
	}
}

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Numerics;

namespace FontStashSharp
{
	class Renderer : IFontStashRenderer
	{
		SpriteBatch _batch;

		public Renderer(SpriteBatch batch)
		{
			if (batch == null)
			{
				throw new ArgumentNullException(nameof(batch));
			}

			_batch = batch;
		}

		public void Draw(object texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
		{
			var textureWrapper = (Texture2D)texture;

			_batch.Draw(textureWrapper,
				position.ToXNA(),
				sourceRectangle == null?default(Microsoft.Xna.Framework.Rectangle?):sourceRectangle.Value.ToXNA(),
				color.ToXNA(),
				rotation,
				origin.ToXNA(),
				scale.ToXNA(),
				SpriteEffects.None,
				depth);
		}
	}
}

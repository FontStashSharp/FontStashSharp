using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;

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

		public void Draw(ITexture2D texture, PointF position, Rectangle? sourceRectangle, Color color, float rotation, PointF origin, PointF scale, float depth)
		{
			var textureWrapper = (Texture2DWrapper)texture;

			_batch.Draw(textureWrapper.Texture,
				position.ToVector2(),
				sourceRectangle == null?default(Microsoft.Xna.Framework.Rectangle?):sourceRectangle.Value.ToRectangle(),
				color.ToColor(),
				rotation,
				origin.ToVector2(),
				scale.ToVector2(),
				SpriteEffects.None,
				depth);
		}
	}
}

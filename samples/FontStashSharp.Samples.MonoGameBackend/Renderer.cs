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

		public void Draw(ITexture2D texture, Rectangle dest, Rectangle source, System.Drawing.Color color, float depth)
		{
			var textureWrapper = (Texture2DWrapper)texture;

			var rdest = dest.ToRectangle();
			_batch.Draw(textureWrapper.Texture,
				rdest,
				source.ToRectangle(),
				color.ToColor(),
				0f,
				Microsoft.Xna.Framework.Vector2.Zero,
				SpriteEffects.None,
				depth);
		}
	}
}

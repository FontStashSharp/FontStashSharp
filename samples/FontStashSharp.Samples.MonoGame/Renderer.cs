using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;

namespace FontStashSharp.Samples.MonoGame
{
	class Renderer : IRenderer
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

		public void Draw(ITexture texture, Rectangle dest, Rectangle source, FssColor color, float depth)
		{
			var textureWrapper = (TextureWrapper)texture;

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

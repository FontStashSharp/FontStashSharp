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
		Texture2DManager _textureManager;

		public ITexture2DManager TextureManager => _textureManager;

		public Renderer(SpriteBatch batch)
		{
			if (batch == null)
			{
				throw new ArgumentNullException(nameof(batch));
			}

			_batch = batch;
			_textureManager = new Texture2DManager(batch.GraphicsDevice);
		}

		public void Draw(object texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 scale, float depth)
		{
			var textureWrapper = (Texture2D)texture;

			_batch.Draw(textureWrapper,
				position.ToXNA(),
				sourceRectangle == null?default(Microsoft.Xna.Framework.Rectangle?):sourceRectangle.Value.ToXNA(),
				color.ToXNA(),
				rotation,
				Microsoft.Xna.Framework.Vector2.Zero,
				scale.ToXNA(),
				SpriteEffects.None,
				depth);
		}
	}
}

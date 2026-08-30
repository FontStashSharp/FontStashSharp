#if MONOGAME || FNA

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp
{
	public class SDFTextBatch : IFontStashRenderer, IDisposable
	{
		private readonly SpriteBatch _spriteBatch;

		public GraphicsDevice GraphicsDevice => _spriteBatch.GraphicsDevice;

		public SDFTextBatch(GraphicsDevice graphicsDevice)
		{
			_spriteBatch = new SpriteBatch(graphicsDevice);
		}

		public void Dispose()
		{
			_spriteBatch.Dispose();
			GC.SuppressFinalize(this);
		}

		public void Begin()
		{
			var effect = Resources.GetEffect(_spriteBatch.GraphicsDevice);
			_spriteBatch.Begin(effect: effect, blendState: BlendState.NonPremultiplied);
		}

		public void End()
		{
			_spriteBatch.End();
		}

		public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None)
		{
			font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
		}

		public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
		{
			_spriteBatch.Draw(texture,
				pos,
				src,
				color,
				rotation,
				Vector2.Zero,
				scale,
				SpriteEffects.None,
				depth);
		}
	}
}

#endif
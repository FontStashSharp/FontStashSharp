using System.Text;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#endif

namespace FontStashSharp
{
	public static class SpriteBatchExtensions
	{
		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color color, Vector2 scale, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;
			return font.DrawText(renderer, pos.X, pos.Y, text, color, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color color, float depth = 0.0f)
		{
			return DrawString(batch, font, text, pos, color, DynamicSpriteFont.DefaultScale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return font.DrawText(renderer, pos.X, pos.Y, text, colors, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color[] colors, float depth = 0.0f)
		{
			return DrawString(batch, font, text, pos, colors, DynamicSpriteFont.DefaultScale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color color, Vector2 scale, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return font.DrawText(renderer, pos.X, pos.Y, text, color, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color color, float depth = 0.0f)
		{
			return DrawString(batch, font, text, pos, color, DynamicSpriteFont.DefaultScale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return font.DrawText(renderer, pos.X, pos.Y, text, colors, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color[] colors, float depth = 0.0f)
		{
			return DrawString(batch, font, text, pos, colors, DynamicSpriteFont.DefaultScale, depth);
		}
	}
}
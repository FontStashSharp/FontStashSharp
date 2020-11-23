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
		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color color, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, color, scale, origin, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color color, Vector2 scale, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, color, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color color, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, color, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color[] colors, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, colors, scale, origin, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, colors, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 pos, Color[] colors, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, colors, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color color, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, color, scale, origin, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color color, Vector2 scale, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, color, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color color, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, color, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color[] colors, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, colors, scale, origin, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, colors, scale, depth);
		}

		public static float DrawString(this SpriteBatch batch, DynamicSpriteFont font, StringBuilder text, Vector2 pos, Color[] colors, float depth = 0.0f)
		{
			return font.DrawText(batch, text, pos, colors, depth);
		}
	}
}
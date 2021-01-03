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
	partial class SpriteFontBase
	{
		public float DrawText(SpriteBatch batch, string text, Vector2 pos, Color color, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;
			return DrawText(renderer, text, pos, color, scale, origin, depth);
		}

		public float DrawText(SpriteBatch batch, string text, Vector2 pos, Color color, Vector2 scale, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, color, scale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, string text, Vector2 pos, Color color, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, color, DefaultScale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, string text, Vector2 pos, Color[] colors, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return DrawText(renderer, text, pos, colors, scale, origin, depth);
		}

		public float DrawText(SpriteBatch batch, string text, Vector2 pos, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, colors, scale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, string text, Vector2 pos, Color[] colors, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, colors, DefaultScale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 pos, Color color, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return DrawText(renderer, text, pos, color, scale, origin, depth);
		}

		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 pos, Color color, Vector2 scale, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, color, scale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 pos, Color color, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, color, DefaultScale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 pos, Color[] colors, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return DrawText(renderer, text, pos, colors, scale, origin, depth);
		}

		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 pos, Color[] colors, Vector2 scale, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, colors, scale, DefaultOrigin, depth);
		}

		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 pos, Color[] colors, float depth = 0.0f)
		{
			return DrawText(batch, text, pos, colors, DefaultScale, DefaultOrigin, depth);
		}
	}
}
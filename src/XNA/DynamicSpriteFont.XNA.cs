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
		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, string text, Vector2 position, Color color, Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;
			return DrawText(renderer, text, position, color, scale, rotation, origin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, string text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, string text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, string text, Vector2 position, Color[] colors, Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return DrawText(renderer, text, position, colors, scale, rotation, origin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, string text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, colors, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, string text, Vector2 position, Color[] colors, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return DrawText(renderer, text, position, color, scale, rotation, origin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 position, Color[] colors, Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;

			return DrawText(renderer, text, position, colors, scale, rotation, origin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, colors, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="batch">A SpriteBatch.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(SpriteBatch batch, StringBuilder text, Vector2 position, Color[] colors, float layerDepth = 0.0f)
		{
			return DrawText(batch, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
		}
	}
}
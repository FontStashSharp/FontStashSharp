using System;
using System.Text;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Extension methods for SpriteBatch to draw text with FontStashSharp fonts
	/// </summary>
	public static class SpriteBatchExtensions
	{
		/// <summary>
		/// Draws a text using the specified font.
		/// </summary>
		/// <param name="batch">The sprite batch</param>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The width of the drawn text</returns>
		public static float DrawString(this SpriteBatch batch, SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			return font.DrawText(batch, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		/// <summary>
		/// Draws a text using the specified font, with per-glyph colors.
		/// </summary>
		/// <param name="batch">The sprite batch</param>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The width of the drawn text</returns>
		public static float DrawString(this SpriteBatch batch, SpriteFontBase font, string text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			return font.DrawText(batch, text, position, colors, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		/// <summary>
		/// Draws a text using the specified font.
		/// </summary>
		/// <param name="batch">The sprite batch</param>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The width of the drawn text</returns>
		public static float DrawString(this SpriteBatch batch, SpriteFontBase font, StringSegment text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			return font.DrawText(batch, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		/// <summary>
		/// Draws a text using the specified font, with per-glyph colors.
		/// </summary>
		/// <param name="batch">The sprite batch</param>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The width of the drawn text</returns>
		public static float DrawString(this SpriteBatch batch, SpriteFontBase font, StringSegment text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			return font.DrawText(batch, text, position, colors, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		/// <summary>
		/// Draws a text using the specified font.
		/// </summary>
		/// <param name="batch">The sprite batch</param>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The width of the drawn text</returns>
		public static float DrawString(this SpriteBatch batch, SpriteFontBase font, StringBuilder text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			return font.DrawText(batch, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		/// <summary>
		/// Draws a text using the specified font, with per-glyph colors.
		/// </summary>
		/// <param name="batch">The sprite batch</param>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The width of the drawn text</returns>
		public static float DrawString(this SpriteBatch batch, SpriteFontBase font, StringBuilder text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			return font.DrawText(batch, text, position, colors, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}
	}
}
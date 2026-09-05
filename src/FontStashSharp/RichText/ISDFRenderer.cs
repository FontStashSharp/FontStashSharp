#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using Texture2D = System.Object;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Provides methods for rendering text with signed distance field (SDF) effects such as shadows and strokes.
	/// </summary>
	public interface ISDFTextRenderer
	{
		/// <summary>
		/// Draws a string of text without any SDF effect applied.
		/// </summary>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="rotation">The rotation angle in radians.</param>
		/// <param name="origin">The origin point used for positioning and rotation.</param>
		/// <param name="scale">The scale factors for the X and Y axes, or null to use (1, 1).</param>
		/// <param name="layerDepth">The depth value used for layering the text.</param>
		/// <param name="characterSpacing">The additional spacing between characters.</param>
		/// <param name="lineSpacing">The additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2? scale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle);

		/// <summary>
		/// Draws a string of text with a shadow effect.
		/// </summary>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="rotation">The rotation angle in radians.</param>
		/// <param name="origin">The origin point used for positioning and rotation.</param>
		/// <param name="scale">The scale factors for the X and Y axes, or null to use (1, 1).</param>
		/// <param name="layerDepth">The depth value used for layering the text.</param>
		/// <param name="characterSpacing">The additional spacing between characters.</param>
		/// <param name="lineSpacing">The additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="shadowColor">The color of the shadow, or null to use the default shadow color.</param>
		/// <param name="shadowOffsetX">The horizontal offset of the shadow.</param>
		/// <param name="shadowOffsetY">The vertical offset of the shadow.</param>
		void DrawShadowString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2? scale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, Color? shadowColor, float shadowOffsetX, float shadowOffsetY);

		/// <summary>
		/// Draws a string of text with a stroke (outline) effect.
		/// </summary>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="rotation">The rotation angle in radians.</param>
		/// <param name="origin">The origin point used for positioning and rotation.</param>
		/// <param name="scale">The scale factors for the X and Y axes, or null to use (1, 1).</param>
		/// <param name="layerDepth">The depth value used for layering the text.</param>
		/// <param name="characterSpacing">The additional spacing between characters.</param>
		/// <param name="lineSpacing">The additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="strokeColor">The color of the stroke, or null to use the default stroke color.</param>
		/// <param name="strokeThickness">The thickness of the stroke.</param>
		/// <param name="strokeSmoothness">The smoothness of the stroke edges.</param>
		void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2? scale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, Color? strokeColor, float strokeThickness, float strokeSmoothness);

		/// <summary>
		/// Draws a sprite directly, bypassing any currently applied SDF effect.
		/// </summary>
		/// <param name="texture">The texture to draw.</param>
		/// <param name="pos">The position to draw at.</param>
		/// <param name="src">The source rectangle within the texture, or null to use the entire texture.</param>
		/// <param name="color">The color to apply to the sprite.</param>
		/// <param name="rotation">The rotation angle in radians.</param>
		/// <param name="scale">The scale factors for X and Y axes.</param>
		/// <param name="depth">The depth value for layering.</param>
		void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth);
	}
}

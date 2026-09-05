#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using Color = FontStashSharp.FSColor;
using Texture2D = System.Object;
#endif

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Provides methods used by rich text chunks to draw text and images.
	/// </summary>
	public interface IFSRenderContext
	{
		/// <summary>
		/// Draws a string of text using the specified font and styling.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="pos">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The visual effect (blur, stroke) to apply.</param>
		/// <param name="effectAmount">The strength of the applied effect.</param>
		void DrawText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle, FontSystemEffect effect, int effectAmount);

		/// <summary>
		/// Draws a string of text using signed distance field rendering without any effect.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="pos">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="textStyle">The text style to apply.</param>
		void DrawSDFText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle);

		/// <summary>
		/// Draws a string of text using signed distance field rendering with a shadow effect.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="pos">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="shadowColor">The color of the shadow.</param>
		/// <param name="shadowOffset">The offset of the shadow relative to the text.</param>
		void DrawSDFShadowText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle, Color shadowColor, Vector2 shadowOffset);

		/// <summary>
		/// Draws a string of text using signed distance field rendering with a stroke effect.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="font">The font to use for rendering.</param>
		/// <param name="pos">The position at which to draw the text.</param>
		/// <param name="color">The color to apply to the text.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="strokeColor">The color of the stroke.</param>
		/// <param name="strokeThickness">The thickness of the stroke.</param>
		/// <param name="strokeSmoothness">The smoothness of the stroke edges.</param>
		void DrawSDFStrokeText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle, Color strokeColor, float strokeThickness, float strokeSmoothness);

		/// <summary>
		/// Draws an image using the specified source region, position, scale, and color.
		/// </summary>
		/// <param name="texture">The texture to draw.</param>
		/// <param name="sourceRegion">The source region within the texture to draw.</param>
		/// <param name="position">The position at which to draw the image.</param>
		/// <param name="scale">The scale factors for the X and Y axes.</param>
		/// <param name="color">The color to apply to the image.</param>
		void DrawImage(Texture2D texture, Rectangle sourceRegion, Vector2 position, Vector2 scale, Color color);
	}

}

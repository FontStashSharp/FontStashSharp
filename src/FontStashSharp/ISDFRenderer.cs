
#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Provides methods for rendering text with signed distance field (SDF) effects such as shadows and strokes.
	/// </summary>
	public interface ISDFTextRenderer
	{
		/// <summary>
		/// Resets the currently applied SDF effect, so subsequent draws use plain text rendering.
		/// </summary>
		void ResetEffect();

		/// <summary>
		/// Configures the renderer to draw a shadow behind the text using the specified color and offset.
		/// </summary>
		/// <param name="color">The color of the shadow.</param>
		/// <param name="shadowOffset">The offset of the shadow from the text, in pixels.</param>
		void SetShadowEffect(Color color, Vector2 shadowOffset);

		/// <summary>
		/// Configures the renderer to draw an outlined (stroked) version of the text using the specified color, thickness and smoothness.
		/// </summary>
		/// <param name="color">The color of the stroke.</param>
		/// <param name="thickness">The thickness of the stroke, expressed in normalized SDF-space units.</param>
		/// <param name="smoothness">The smoothness of the stroke's edges, expressed in normalized SDF-space units.</param>
		void SetStrokeEffect(Color color, float thickness, float smoothness);

		/// <summary>
		/// Draws text using the specified font with the currently applied SDF effect.
		/// </summary>
		/// <param name="font">The font used to render the text.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">The color of the text.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1).</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		/// <param name="characterSpacing">A character spacing.</param>
		/// <param name="lineSpacing">A line spacing.</param>
		/// <param name="textStyle">The text style to apply.</param>
		void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None);

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

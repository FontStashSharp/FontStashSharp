using System.Text;
using System.Collections.Generic;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
using Texture2D = System.Object;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Specifies the mode used to rasterize glyph bitmaps
	/// </summary>
	public enum FontRasterizationMode
	{
		/// <summary>
		/// Standard anti-aliased rasterization
		/// </summary>
		Standard,

		/// <summary>
		/// Signed distance field (SDF) rasterization
		/// </summary>
		SDF
	}

	/// <summary>
	/// Represents the base class for sprite fonts that can measure and draw text.
	/// </summary>
	public abstract partial class SpriteFontBase
	{
		private static Texture2D _white;

		/// <summary>
		/// User-specified name of the font, which can be used for debugging or informational purposes.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a user-defined tag object associated with this font.
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Gets the line height in pixels of this font.
		/// </summary>
		public int LineHeight { get; }

		/// <summary>
		/// Gets the font rasterization mode used to render this font.
		/// </summary>
		public abstract FontRasterizationMode FontRasterizationMode { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteFontBase"/> class.
		/// </summary>
		/// <param name="lineHeight">The line height in pixels.</param>
		public SpriteFontBase(int lineHeight)
		{
			LineHeight = lineHeight;
		}

		internal abstract float GetKerning(FontGlyph glyph, FontGlyph prevGlyph);

		/// <summary>
		/// Returns the name of the font.
		/// </summary>
		/// <returns>The font name.</returns>
		public override string ToString() => Name;

		#region IFontStashRenderer Draw Methods

		internal abstract float InternalDrawText(IFontStashRenderer renderer, TextColorSource source, Vector2 position,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount);

		/// <summary>
		/// Draws a text string using the specified renderer with a uniform color.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask applied to all characters.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText(renderer, new TextColorSource(text, color), position, rotation, origin, scale,
					layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text string using the specified renderer with per-character colors.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">An array of colors applied per character.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text segment using the specified renderer with a uniform color.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text segment to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask applied to all characters.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer renderer, StringSegment text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text segment using the specified renderer with per-character colors.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text segment to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">An array of colors applied per character.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer renderer, StringSegment text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a <see cref="StringBuilder"/> text using the specified renderer with a uniform color.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask applied to all characters.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a <see cref="StringBuilder"/> text using the specified renderer with per-character colors.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">An array of colors applied per character.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		#endregion

		#region IFontStashRenderer2 Draw Methods

		internal abstract float InternalDrawText2(IFontStashRenderer2 renderer, TextColorSource source,
			Vector2 position, float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount);

		/// <summary>
		/// Draws a text string using the specified renderer with a uniform color.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask applied to all characters.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText2(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text string using the specified renderer with per-character colors.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">An array of colors applied per character.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText2(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text segment using the specified renderer with a uniform color.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text segment to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask applied to all characters.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer2 renderer, StringSegment text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText2(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text segment using the specified renderer with per-character colors.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text segment to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">An array of colors applied per character.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer2 renderer, StringSegment text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText2(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a <see cref="StringBuilder"/> text using the specified renderer with a uniform color.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask applied to all characters.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText2(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a <see cref="StringBuilder"/> text using the specified renderer with per-character colors.
		/// </summary>
		/// <param name="renderer">The font stash renderer.</param>
		/// <param name="text">The text to draw.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">An array of colors applied per character.</param>
		/// <param name="rotation">The rotation of the text in radians.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="layerDepth">The depth of the layer for this text.</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="textStyle">The text style to apply.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The height of the drawn text.</returns>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				InternalDrawText2(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		#endregion

		#region Measurement Methods

		internal abstract Bounds InternalTextBounds(TextSource source, Vector2 position,
			float characterSpacing, float lineSpacing,
			FontSystemEffect effect, int effectAmount);

		/// <summary>
		/// Measures the bounds of the specified text string.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <param name="position">The drawing position of the text.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The bounds of the drawn text.</returns>
		public Bounds TextBounds(string text, Vector2 position, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = InternalTextBounds(new TextSource(text), position, characterSpacing, lineSpacing, effect, effectAmount);

			var realScale = scale ?? Utility.DefaultScale;
			bounds.ApplyScale(realScale);
			return bounds;
		}

		/// <summary>
		/// Measures the bounds of the specified <see cref="StringBuilder"/> text.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <param name="position">The drawing position of the text.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The bounds of the drawn text.</returns>
		public Bounds TextBounds(StringBuilder text, Vector2 position, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = InternalTextBounds(new TextSource(text), position, characterSpacing, lineSpacing, effect, effectAmount);

			var realScale = scale ?? Utility.DefaultScale;
			bounds.ApplyScale(realScale);
			return bounds;
		}

		/// <summary>
		/// Measures the size of the specified text string.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The size of the measured text.</returns>
		public Vector2 MeasureString(string text, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = TextBounds(text, Utility.Vector2Zero, scale, characterSpacing, lineSpacing, effect, effectAmount);
			return new Vector2(bounds.X2, bounds.Y2);
		}

		/// <summary>
		/// Measures the size of the specified <see cref="StringBuilder"/> text.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The size of the measured text.</returns>
		public Vector2 MeasureString(StringBuilder text, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = TextBounds(text, Utility.Vector2Zero, scale, characterSpacing, lineSpacing, effect, effectAmount);
			return new Vector2(bounds.X2, bounds.Y2);
		}

		internal abstract void InternalGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2? sourceScale,
			float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount,
			List<Glyph> result);

		/// <summary>
		/// Gets a list of glyphs that would be used to draw the specified text string.
		/// </summary>
		/// <param name="text">The text to get glyphs for.</param>
		/// <param name="position">The drawing position of the text.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The list of glyphs for the text.</returns>
		public List<Glyph> GetGlyphs(string text, Vector2 position,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			List<Glyph> result = new List<Glyph>();

			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

			return result;
		}

		/// <summary>
		/// Gets a list of glyphs that would be used to draw the specified <see cref="StringBuilder"/> text.
		/// </summary>
		/// <param name="text">The text to get glyphs for.</param>
		/// <param name="position">The drawing position of the text.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		/// <returns>The list of glyphs for the text.</returns>
		public List<Glyph> GetGlyphs(StringBuilder text, Vector2 position,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			List<Glyph> result = new List<Glyph>();

			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

			return result;
		}

		/// <summary>
		/// Fills the provided list with glyphs that would be used to draw the specified text string.
		/// </summary>
		/// <param name="text">The text to get glyphs for.</param>
		/// <param name="position">The drawing position of the text.</param>
		/// <param name="result">The list to fill with glyphs.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		public void GetGlyphs(string text, Vector2 position, List<Glyph> result,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

		/// <summary>
		/// Fills the provided list with glyphs that would be used to draw the specified <see cref="StringBuilder"/> text.
		/// </summary>
		/// <param name="text">The text to get glyphs for.</param>
		/// <param name="position">The drawing position of the text.</param>
		/// <param name="result">The list to fill with glyphs.</param>
		/// <param name="origin">The center of rotation.</param>
		/// <param name="scale">The scaling factor. <c>null</c> defaults to (1, 1).</param>
		/// <param name="characterSpacing">Additional spacing between characters.</param>
		/// <param name="lineSpacing">Additional spacing between lines.</param>
		/// <param name="effect">The font system effect to apply.</param>
		/// <param name="effectAmount">The amount of the effect to apply.</param>
		public void GetGlyphs(StringBuilder text, Vector2 position, List<Glyph> result,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

		#endregion

#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Gets or creates a 1x1 white texture for rendering.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <returns>A 1x1 white texture.</returns>
		public static Texture2D GetWhite(GraphicsDevice graphicsDevice)
#else
		/// <summary>
		/// Gets or creates a 1x1 white texture for rendering.
		/// </summary>
		/// <param name="textureManager">The texture manager.</param>
		/// <returns>A 1x1 white texture.</returns>
		public static Texture2D GetWhite(ITexture2DManager textureManager)
#endif
		{
			if (_white != null)
			{
				return _white;
			}

#if MONOGAME || FNA || KNI || XNA || STRIDE
			_white = Texture2DManager.CreateTexture(graphicsDevice, 1, 1);
			Texture2DManager.SetTextureData(_white, new Rectangle(0, 0, 1, 1), new byte[] { 255, 255, 255, 255 });
#else
			_white = textureManager.CreateTexture(1, 1);
			textureManager.SetTextureData(_white, new Rectangle(0, 0, 1, 1), new byte[] { 255, 255, 255, 255 });
#endif

			return _white;
		}

	}
}

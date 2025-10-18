using FontStashSharp.Interfaces;
using System.Text;
using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp
{
	partial class SpriteFontBase
	{
		private void RenderStyle(IFontStashRenderer renderer, TextStyle textStyle, Vector2 pos,
			int lineHeight, int ascent, Color color, ref Matrix transformation, float rotation, Vector2 scale, float layerDepth)
		{
			if (textStyle == TextStyle.None || pos.X == 0)
			{
				return;
			}

#if MONOGAME || FNA || STRIDE
			var white = GetWhite(renderer.GraphicsDevice);
#else
			var white = GetWhite(renderer.TextureManager);
#endif

			var start = Vector2.Zero;
			if (textStyle == TextStyle.Strikethrough)
			{
				start.Y = pos.Y - ascent + lineHeight / 2 - (FontSystemDefaults.TextStyleLineHeight / 2) * RenderFontSizeMultiplicator;
			}
			else
			{
				start.Y = pos.Y + RenderFontSizeMultiplicator;
			}

			start = start.Transform(ref transformation);

			scale.X *= pos.X;
			scale.Y *= (FontSystemDefaults.TextStyleLineHeight * RenderFontSizeMultiplicator);

			renderer.Draw(white, start, null, color, rotation, scale, layerDepth);
		}

		private float DrawText(IFontStashRenderer renderer, TextColorSource source, Vector2 position,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			if (source.IsNull) return 0.0f;

			var dynamicFont = this as DynamicSpriteFont;
			if (dynamicFont != null && dynamicFont.FontSystem.UseTextShaping)
			{
				return DrawShapedText(renderer, source, position, rotation, origin, sourceScale,
					layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, rotation, origin, ref scale, out transformation);

			int ascent, lineHeight;
			PreDraw(source.TextSource, effect, effectAmount, out ascent, out lineHeight);

			var pos = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			Color? firstColor = null;
			while (true)
			{
				int codepoint;
				Color color;
				if (!source.GetNextCodepoint(out codepoint))
					break;

				if (codepoint == '\n')
				{
					if (textStyle != TextStyle.None && firstColor != null)
					{
						RenderStyle(renderer, textStyle, pos,
							lineHeight, ascent, firstColor.Value, ref transformation,
							rotation, scale, layerDepth);
					}
					pos.X = 0.0f;
					pos.Y += lineHeight + lineSpacing;
					prevGlyph = null;
					continue;
				}

#if MONOGAME || FNA || STRIDE
				var glyph = GetGlyph(renderer.GraphicsDevice, codepoint, effect, effectAmount);
#else
				var glyph = GetGlyph(renderer.TextureManager, codepoint, effect, effectAmount);
#endif

				if (glyph == null)
				{
					continue;
				}

				if (prevGlyph != null)
				{
					pos.X += characterSpacing;
					pos.X += GetKerning(glyph, prevGlyph);
				}

				if (!glyph.IsEmpty)
				{
					color = source.GetNextColor();
					firstColor = color;

					var p = pos + new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y);
					p = p.Transform(ref transformation);

					renderer.Draw(glyph.Texture,
						p,
						glyph.TextureRectangle,
						color,
						rotation,
						scale,
						layerDepth);
				}

				pos.X += glyph.XAdvance;
				prevGlyph = glyph;
			}

			if (textStyle != TextStyle.None && firstColor != null)
			{
				RenderStyle(renderer, textStyle, pos,
					lineHeight, ascent, firstColor.Value, ref transformation,
					rotation, scale, layerDepth);
			}

			return position.X + pos.X;
		}

		private float DrawShapedText(IFontStashRenderer renderer, TextColorSource source, Vector2 position,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			var dynamicFont = this as DynamicSpriteFont;
			if (dynamicFont == null)
			{
				throw new InvalidOperationException("Text shaping is only supported with DynamicSpriteFont");
			}

			var text = source.TextSource.StringText.String ?? source.TextSource.StringBuilderText?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return 0.0f;
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, rotation, origin, ref scale, out transformation);

			var lines = text.Split('\n');

			int ascent = 0, lineHeight = 0;
			if (dynamicFont.FontSystem.FontSources.Count > 0)
			{
				int descent, lh;
				dynamicFont.FontSystem.FontSources[0].GetMetricsForSize(FontSize * dynamicFont.FontSystem.FontResolutionFactor, out ascent, out descent, out lh);
				lineHeight = lh;
			}

			var pos = new Vector2(0, ascent);
			float maxX = 0;
			Color? firstColor = null;

			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var line = lines[lineIndex];

				if (lineIndex > 0)
				{
					if (textStyle != TextStyle.None && firstColor != null)
					{
						RenderStyle(renderer, textStyle, pos,
							lineHeight, ascent, firstColor.Value, ref transformation,
							rotation, scale, layerDepth);
					}

					pos.X = 0.0f;
					pos.Y += lineHeight + lineSpacing;
					firstColor = null;
				}

				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				var shapedText = dynamicFont.GetShapedText(line, FontSize * dynamicFont.FontSystem.FontResolutionFactor);

				float lineStartX = pos.X;
				for (int i = 0; i < shapedText.Glyphs.Length; i++)
				{
					var shapedGlyph = shapedText.Glyphs[i];

					if (i > 0 && characterSpacing > 0)
					{
						pos.X += characterSpacing;
					}

#if MONOGAME || FNA || STRIDE
					var glyph = dynamicFont.GetGlyphByGlyphId(renderer.GraphicsDevice, shapedGlyph.GlyphId, shapedGlyph.FontSourceIndex, effect, effectAmount);
#else
					var glyph = dynamicFont.GetGlyphByGlyphId(renderer.TextureManager, shapedGlyph.GlyphId, shapedGlyph.FontSourceIndex, effect, effectAmount);
#endif

					if (glyph != null && !glyph.IsEmpty)
					{
						var color = source.GetNextColor();
						firstColor = color;

						// Apply HarfBuzz positioning
						var glyphPos = pos + new Vector2(
							glyph.RenderOffset.X + (shapedGlyph.XOffset / 64.0f),
							glyph.RenderOffset.Y + (shapedGlyph.YOffset / 64.0f)
						);

						glyphPos = glyphPos.Transform(ref transformation);

						renderer.Draw(glyph.Texture,
							glyphPos,
							glyph.TextureRectangle,
							color,
							rotation,
							scale,
							layerDepth);
					}

					if (glyph != null)
					{
						pos.X += glyph.XAdvance;
						pos.Y += (shapedGlyph.YAdvance / 64.0f);
					}
					else
					{
						// Fallback
						pos.X += (shapedGlyph.XAdvance / 64.0f);
						pos.Y += (shapedGlyph.YAdvance / 64.0f);
					}
				}

				if (pos.X > maxX)
				{
					maxX = pos.X;
				}
			}

			if (textStyle != TextStyle.None && firstColor != null)
			{
				RenderStyle(renderer, textStyle, pos,
					lineHeight, ascent, firstColor.Value, ref transformation,
					rotation, scale, layerDepth);
			}

			return position.X + maxX;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				DrawText(renderer, new TextColorSource(text, color), position, rotation, origin, scale,
					layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				DrawText(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer renderer, StringSegment text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				DrawText(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer renderer, StringSegment text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				DrawText(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				DrawText(renderer, new TextColorSource(text, color), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
				DrawText(renderer, new TextColorSource(text, colors), position, rotation, origin, scale, layerDepth,
					characterSpacing, lineSpacing, textStyle, effect, effectAmount);
	}
}
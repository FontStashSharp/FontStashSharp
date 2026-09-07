using FontStashSharp.Interfaces;
using System.Text;
using System;


#if MONOGAME || FNA || KNI || XNA
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
	partial class RealFontBase
	{
		/// <summary>
		/// Renders text styling effects such as underline or strikethrough.
		/// </summary>
		/// <param name="renderer">The font stash renderer</param>
		/// <param name="textStyle">The text style to apply</param>
		/// <param name="pos">The position of the text</param>
		/// <param name="lineHeight">The height of a line</param>
		/// <param name="ascent">The ascent of the font</param>
		/// <param name="color">The color to render the style in</param>
		/// <param name="transformation">The transformation matrix</param>
		/// <param name="rotation">The rotation angle</param>
		/// <param name="scale">The scale factors</param>
		/// <param name="layerDepth">The layer depth</param>
		protected void RenderStyle(IFontStashRenderer renderer, TextStyle textStyle, Vector2 pos,
			int lineHeight, int ascent, Color color, ref Matrix transformation, float rotation, Vector2 scale, float layerDepth)
		{
			if (textStyle == TextStyle.None || pos.X == 0)
			{
				return;
			}

#if MONOGAME || FNA || KNI || XNA || STRIDE
			var white = GetWhite(renderer.GraphicsDevice);
#else
			var white = GetWhite(renderer.TextureManager);
#endif

			var start = Vector2.Zero;
			if (textStyle == TextStyle.Strikethrough)
			{
				start.Y = pos.Y - ascent + lineHeight / 2 - (FontSystemDefaults.TextStyleLineHeight / 2);
			}
			else
			{
				start.Y = pos.Y;
			}

			start = start.Transform(ref transformation);

			scale.X *= pos.X;
			scale.Y *= FontSystemDefaults.TextStyleLineHeight;

			renderer.Draw(white, start, null, color, rotation, scale, layerDepth);
		}

		internal override float InternalDrawText(IFontStashRenderer renderer, TextColorSource source, Vector2 position,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || KNI || XNA || STRIDE
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

#if MONOGAME || FNA || KNI || XNA || STRIDE
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
	}
}
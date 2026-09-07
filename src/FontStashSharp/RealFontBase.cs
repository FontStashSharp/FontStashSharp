using FontStashSharp.Interfaces;
using System.Collections.Generic;

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
	/// Represents the base class for fonts that render glyphs from a real (rasterized) font at a fixed point size.
	/// </summary>
	public abstract partial class RealFontBase: SpriteFontBase
	{
		/// <summary>
		/// Gets the font size in points.
		/// </summary>
		public float FontSize { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteFontBase"/> class.
		/// </summary>
		/// <param name="fontSize">The font size in points.</param>
		/// <param name="lineHeight">The line height in pixels.</param>
		protected RealFontBase(float fontSize, int lineHeight): base(lineHeight)
		{
			FontSize = fontSize;
		}

#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Gets a glyph for the specified codepoint with optional effects applied.
		/// </summary>
		/// <param name="device">The graphics device</param>
		/// <param name="codepoint">The Unicode codepoint for the character</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The font glyph for the specified codepoint</returns>
		protected internal abstract FontGlyph GetGlyph(GraphicsDevice device, int codepoint, FontSystemEffect effect, int effectAmount);
#else
		/// <summary>
		/// Gets a glyph for the specified codepoint with optional effects applied.
		/// </summary>
		/// <param name="device">The texture manager</param>
		/// <param name="codepoint">The Unicode codepoint for the character</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The font glyph for the specified codepoint</returns>
		protected internal abstract FontGlyph GetGlyph(ITexture2DManager device, int codepoint, FontSystemEffect effect, int effectAmount);
#endif

		internal abstract void PreDraw(TextSource str, FontSystemEffect effect, int effectAmount, out int ascent, out int lineHeight);

		/// <summary>
		/// Prepares the transformation matrix for text rendering.
		/// </summary>
		/// <param name="position">The drawing position</param>
		/// <param name="rotation">The rotation in radians</param>
		/// <param name="origin">The center of rotation</param>
		/// <param name="scale">The scale factor</param>
		/// <param name="transformation">The resulting transformation matrix</param>
		protected void Prepare(Vector2 position, float rotation, Vector2 origin, ref Vector2 scale, out Matrix transformation)
		{
			origin *= scale;

			Utility.BuildTransform(position, rotation, origin, scale, out transformation);
		}

		internal override Bounds InternalTextBounds(TextSource source, Vector2 position,
			float characterSpacing, float lineSpacing,
			FontSystemEffect effect, int effectAmount)
		{
			if (source.IsNull) return Bounds.Empty;

			int ascent, lineHeight;
			PreDraw(source, effect, effectAmount, out ascent, out lineHeight);

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			while (true)
			{
				int codepoint;
				if (!source.GetNextCodepoint(out codepoint))
					break;

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight + lineSpacing;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(null, codepoint, effect, effectAmount);
				if (glyph == null)
				{
					continue;
				}

				if (prevGlyph != null)
				{
					x += characterSpacing;
					x += GetKerning(glyph, prevGlyph);
				}

				var x0 = x + glyph.RenderOffset.X;
				if (x0 < minx)
					minx = x0;
				x += glyph.XAdvance;
				if (x > maxx)
					maxx = x;

				var y0 = y + glyph.RenderOffset.Y;
				var y1 = y0 + glyph.Size.Y;
				if (y0 < miny)
					miny = y0;
				if (y1 > maxy)
					maxy = y1;

				prevGlyph = glyph;
			}

			return new Bounds(minx, miny, maxx, maxy);
		}

		internal override void InternalGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2? sourceScale,
			float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount,
			List<Glyph> result)
		{
			if (source.IsNull)
			{
				return;
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, 0, origin, ref scale, out transformation);

			int ascent, lineHeight;
			PreDraw(source, effect, effectAmount, out ascent, out lineHeight);

			var pos = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			var i = 0;
			while (true)
			{
				int codepoint;
				if (!source.GetNextCodepoint(out codepoint))
				{
					break;
				}

				var rect = new Rectangle((int)pos.X, (int)pos.Y - LineHeight, 0, LineHeight);
				var xAdvance = 0;
				if (codepoint == '\n')
				{
					pos.X = 0;
					pos.Y += lineHeight + lineSpacing;
					prevGlyph = null;
				}
				else
				{
					var glyph = GetGlyph(null, codepoint, effect, effectAmount);
					if (glyph != null)
					{
						if (prevGlyph != null)
						{
							pos.X += characterSpacing;
							pos.X += GetKerning(glyph, prevGlyph);
						}

						rect = glyph.RenderRectangle;
						rect.Offset((int)pos.X, (int)pos.Y);

						xAdvance = glyph.XAdvance;
						pos.X += xAdvance;
						prevGlyph = glyph;
					}
				}

				// Apply transformation to rect
				var p = new Vector2(rect.X, rect.Y);
				p = p.Transform(ref transformation);
				var s = new Vector2(rect.Width * scale.X, rect.Height * scale.Y);

				var glyphInfo = new Glyph
				{
					Index = i,
					Codepoint = codepoint,
					Bounds = new Rectangle((int)p.X, (int)p.Y, (int)s.X, (int)s.Y),
					XAdvance = (int)(xAdvance * scale.X)
				};

				// Add to the result
				result.Add(glyphInfo);
				++i;
			}
		}
	}
}
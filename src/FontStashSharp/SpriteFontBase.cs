using System.Collections.Generic;
using System.Text;
using System;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
#endif

namespace FontStashSharp
{
	public abstract partial class SpriteFontBase
	{
		internal static readonly Vector2 DefaultScale = new Vector2(1.0f, 1.0f);
		internal static readonly Vector2 DefaultOrigin = new Vector2(0.0f, 0.0f);

		/// <summary>
		/// Font Size
		/// </summary>
		public int FontSize { get; private set; }

		/// <summary>
		/// Line Height in pixels
		/// </summary>
		public int LineHeight { get; private set; }

		protected float RenderFontSizeMultiplicator { get; set; } = 1f;

		protected SpriteFontBase(int fontSize, int lineHeight)
		{
			FontSize = fontSize;
			LineHeight = lineHeight;
		}

#if MONOGAME || FNA || STRIDE
		protected internal abstract FontGlyph GetGlyph(GraphicsDevice device, int codepoint);
#else
		protected internal abstract FontGlyph GetGlyph(ITexture2DManager device, int codepoint);
#endif

		internal abstract void PreDraw(TextSource str, out int ascent, out int lineHeight);

		private void Prepare(Vector2 position, ref Vector2 scale, float rotation, Vector2 origin, out Matrix transformation)
		{
			scale /= RenderFontSizeMultiplicator;

			// This code had been borrowed from MonoGame's SpriteBatch.DrawString
			transformation = Matrix.Identity;

			float offsetX, offsetY;
			if (rotation == 0)
			{
				transformation.M11 = scale.X;
				transformation.M22 = scale.Y;
				offsetX = position.X - (origin.X * transformation.M11);
				offsetY = position.Y - (origin.Y * transformation.M22);
			}
			else
			{
				var cos = (float)Math.Cos(rotation);
				var sin = (float)Math.Sin(rotation);
				transformation.M11 = scale.X * cos;
				transformation.M12 = scale.X * sin;
				transformation.M21 = scale.Y * -sin;
				transformation.M22 = scale.Y * cos;
				offsetX = position.X - (origin.X * transformation.M11) - (origin.Y * transformation.M21);
				offsetY = position.Y - (origin.X * transformation.M12) - (origin.Y * transformation.M22);
			}

#if MONOGAME || FNA || STRIDE
			transformation.M41 = offsetX;
			transformation.M42 = offsetY;
#else
			transformation.M31 = offsetX;
			transformation.M32 = offsetY;
#endif
		}

		internal virtual Bounds InternalTextBounds(TextSource source, Vector2 position, float characterSpacing, float lineSpacing)
		{
			if (source.IsNull) return Bounds.Empty;

			int ascent, lineHeight;
			PreDraw(source, out ascent, out lineHeight);

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

				var glyph = GetGlyph(null, codepoint);
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

		public Bounds TextBounds(string text, Vector2 position, Vector2? scale = null, float characterSpacing = 0.0f, float lineSpacing = 0.0f)
		{
			var bounds = InternalTextBounds(new TextSource(text), position, characterSpacing, lineSpacing);

			var realScale = scale ?? DefaultScale;
			bounds.ApplyScale(realScale / RenderFontSizeMultiplicator);
			return bounds;
		}

		public Bounds TextBounds(StringBuilder text, Vector2 position, Vector2? scale = null, float characterSpacing = 0.0f, float lineSpacing = 0.0f)
		{
			var bounds = InternalTextBounds(new TextSource(text), position, characterSpacing, lineSpacing);

			var realScale = scale ?? DefaultScale;
			bounds.ApplyScale(realScale / RenderFontSizeMultiplicator);
			return bounds;
		}

		private List<Rectangle> GetGlyphRects(TextSource source, Vector2 position, Vector2 origin, Vector2? sourceScale, float characterSpacing, float lineSpacing)
		{
			List<Rectangle> rects = new List<Rectangle>();
			if (source.IsNull) return rects;

			Matrix transformation;

			var scale = sourceScale ?? DefaultScale;
			Prepare(position, ref scale, 0, origin, out transformation);

			int ascent, lineHeight;
			PreDraw(source, out ascent, out lineHeight);

			var pos = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			while (true)
			{
				int codepoint;
				if (!source.GetNextCodepoint(out codepoint))
				{
					break;
				}

				var rect = new Rectangle((int)pos.X, (int)pos.Y - LineHeight, 0, LineHeight);
				if (codepoint == '\n')
				{
					pos.X = 0;
					pos.Y += lineHeight + lineSpacing;
					prevGlyph = null;
				}
				else
				{
					var glyph = GetGlyph(null, codepoint);
					if (glyph != null)
					{
						if (prevGlyph != null)
						{
							pos.X += characterSpacing;
							pos.X += GetKerning(glyph, prevGlyph);
						}

						rect = glyph.RenderRectangle;
						rect.Offset((int)pos.X, (int)pos.Y);

						pos.X += glyph.XAdvance;
						prevGlyph = glyph;
					}
				}

				// Apply transformation to rect
				var p = new Vector2(rect.X, rect.Y);
				p = p.Transform(ref transformation);
				var s = new Vector2(rect.Width * scale.X, rect.Height * scale.Y);

				// Add to the result
				rects.Add(new Rectangle((int)p.X, (int)p.Y, (int)s.X, (int)s.Y));
			}

			return rects;
		}

		public List<Rectangle> GetGlyphRects(string text, Vector2 position,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
			GetGlyphRects(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing);

		public List<Rectangle> GetGlyphRects(StringBuilder text, Vector2 position,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
			GetGlyphRects(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing);

		public Vector2 MeasureString(string text, Vector2? scale = null, float characterSpacing = 0.0f, float lineSpacing = 0.0f)
		{
			var bounds = TextBounds(text, Utility.Vector2Zero, scale, characterSpacing, lineSpacing);
			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(StringBuilder text, Vector2? scale = null, float characterSpacing = 0.0f, float lineSpacing = 0.0f)
		{
			var bounds = TextBounds(text, Utility.Vector2Zero, scale, characterSpacing, lineSpacing);
			return new Vector2(bounds.X2, bounds.Y2);
		}

		internal abstract float GetKerning(FontGlyph glyph, FontGlyph prevGlyph);
	}
}
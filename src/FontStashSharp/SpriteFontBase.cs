using FontStashSharp.Interfaces;
using System.Collections.Generic;
using System.Text;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp
{
	public abstract partial class SpriteFontBase
	{
		internal static readonly Vector2 DefaultScale = new Vector2(1.0f, 1.0f);
		internal static readonly Vector2 DefaultOrigin = new Vector2(0.0f, 0.0f);

		/// <summary>
		/// Line height in pixels
		/// </summary>
		public int FontSize { get; private set; }

		protected SpriteFontBase(int fontSize)
		{
			FontSize = fontSize;
		}

		protected internal abstract FontGlyph GetGlyph(int codepoint, bool withoutBitmap);
		protected abstract void PreDraw(string str, out float ascent, out float lineHeight);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (string.IsNullOrEmpty(text)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var q = new FontGlyphSquad();
			for (int i = 0; i < text.Length; i += char.IsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(text, i);
				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

					renderer.Draw(glyph.Texture,
						position,
						sourceRect,
						color,
						rotation,
						origin - q.Offset,
						scale,
						layerDepth);
				}

				prevGlyph = glyph;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (string.IsNullOrEmpty(text)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var pos = 0;
			var q = new FontGlyphSquad();
			for (int i = 0; i < text.Length; i += char.IsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(text, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					++pos;
					continue;
				}

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					++pos;
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

					renderer.Draw(glyph.Texture,
						position,
						sourceRect,
						colors[pos],
						rotation,
						origin - q.Offset,
						scale,
						layerDepth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, string text, Vector2 position, Color[] colors, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		protected abstract void PreDraw(StringBuilder str, out float ascent, out float lineHeight);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (text == null || text.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var q = new FontGlyphSquad();
			for (int i = 0; i < text.Length; i += StringBuilderIsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(text, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

					renderer.Draw(glyph.Texture,
						position,
						sourceRect,
						color,
						rotation,
						origin - q.Offset,
						scale,
						layerDepth);
				}

				prevGlyph = glyph;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors,
													Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
		{
			if (text == null || text.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(text, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var pos = 0;
			var q = new FontGlyphSquad();
			for (int i = 0; i < text.Length; i += StringBuilderIsSurrogatePair(text, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(text, i);

				if (codepoint == '\n')
				{
					originX = 0.0f;
					originY += lineHeight;
					prevGlyph = null;
					++pos;
					continue;
				}

				var glyph = GetGlyph(codepoint, false);
				if (glyph == null)
				{
					++pos;
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref originX, ref originY, ref q);
				if (!glyph.IsEmpty)
				{
					var sourceRect = new Rectangle((int)q.S0, (int)q.T0, (int)(q.S1 - q.S0), (int)(q.T1 - q.T0));

					renderer.Draw(glyph.Texture,
						position,
						sourceRect,
						colors[pos],
						rotation,
						origin - q.Offset,
						scale,
						layerDepth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer renderer, StringBuilder text, Vector2 position, Color[] colors, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		public virtual float TextBounds(string str, Vector2 position, ref Bounds bounds, Vector2 scale)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var x = position.X;
			var y = position.Y;
			var q = new FontGlyphSquad();
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);
				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);
				if (q.X0 < minx)
					minx = q.X0;
				var scaledX = x * scale.X;
				if (scaledX > maxx)
					maxx = scaledX;

				if (q.Y0 < miny)
					miny = q.Y0;
				if (q.Y1 > maxy)
					maxy = q.Y1;

				prevGlyph = glyph;
			}

			float advance = x - startx;
			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;

			return advance;
		}

		public float TextBounds(string str, Vector2 position, ref Bounds bounds)
		{
			return TextBounds(str, position, ref bounds, DefaultScale);
		}

		public virtual float TextBounds(StringBuilder str, Vector2 position, ref Bounds bounds, Vector2 scale)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var q = new FontGlyphSquad();

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);
				if (q.X0 < minx)
					minx = q.X0;
				var scaledX = x * scale.X;
				if (scaledX > maxx)
					maxx = scaledX;

				if (q.Y0 < miny)
					miny = q.Y0;
				if (q.Y1 > maxy)
					maxy = q.Y1;

				prevGlyph = glyph;
			}

			float advance = x - startx;
			bounds.X = minx;
			bounds.Y = miny;
			bounds.X2 = maxx;
			bounds.Y2 = maxy;

			return advance;
		}

		public float TextBounds(StringBuilder str, Vector2 position, ref Bounds bounds)
		{
			return TextBounds(str, position, ref bounds, DefaultScale);
		}

		public List<Rectangle> GetGlyphRects(string str, Vector2 position, Vector2 scale)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (string.IsNullOrEmpty(str)) return Rects;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var q = new FontGlyphSquad();

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float startx = x;

			FontGlyph prevGlyph = null;
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);

				Rects.Add(new Rectangle((int)q.X0, (int)q.Y0, (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0)));
				prevGlyph = glyph;
			}

			return Rects;
		}

		public List<Rectangle> GetGlyphRects(string str, Vector2 position)
		{
			return GetGlyphRects(str, position, DefaultScale);
		}

		public List<Rectangle> GetGlyphRects(StringBuilder str, Vector2 position, Vector2 scale)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (str == null || str.Length == 0) return Rects;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var q = new FontGlyphSquad();

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(codepoint, true);
				if (glyph == null)
				{
					continue;
				}

				GetQuad(glyph, prevGlyph, scale, ref x, ref y, ref q);

				Rects.Add(new Rectangle((int)q.X0, (int)q.Y0, (int)(q.X1 - q.X0), (int)(q.Y1 - q.Y0)));
				prevGlyph = glyph;
			}

			return Rects;
		}

		public List<Rectangle> GetGlyphRects(StringBuilder str, Vector2 position)
		{
			return GetGlyphRects(str, position, DefaultScale);
		}

		public Vector2 MeasureString(string text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(text, Utility.Vector2Zero, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(string text)
		{
			return MeasureString(text, DefaultScale);
		}

		public Vector2 MeasureString(StringBuilder text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(text, Utility.Vector2Zero, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			return MeasureString(text, DefaultScale);
		}


		protected static bool StringBuilderIsSurrogatePair(StringBuilder sb, int index)
		{
			if (index + 1 < sb.Length)
				return char.IsSurrogatePair(sb[index], sb[index + 1]);
			return false;
		}

		protected static int StringBuilderConvertToUtf32(StringBuilder sb, int index)
		{
			if (!char.IsHighSurrogate(sb[index]))
				return sb[index];

			return char.ConvertToUtf32(sb[index], sb[index + 1]);
		}

		internal virtual void GetQuad(FontGlyph glyph, FontGlyph prevGlyph, Vector2 scale, ref float x, ref float y, ref FontGlyphSquad q)
		{
			float rx = x + glyph.XOffset;
			float ry = y + glyph.YOffset;
			q.X0 = rx * scale.X;
			q.Y0 = ry * scale.Y;
			q.X1 = (rx + glyph.Bounds.Width) * scale.X;
			q.Y1 = (ry + glyph.Bounds.Height) * scale.Y;
			q.S0 = glyph.Bounds.X;
			q.T0 = glyph.Bounds.Y;
			q.S1 = glyph.Bounds.Right;
			q.T1 = glyph.Bounds.Bottom;
			q.Offset = new Vector2(rx, ry);

			x += glyph.XAdvance;
		}
	}
}
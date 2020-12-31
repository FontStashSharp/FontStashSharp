using FontStashSharp.Interfaces;
using System.Collections.Generic;
using System.Text;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using Vector2 = System.Drawing.PointF;
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

		protected abstract FontGlyph GetGlyph(int codepoint, bool withoutBitmap);
		protected abstract void PreDraw(string str, out float ascent, out float lineHeight);

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color color, Vector2 scale,
			Vector2 origin, float depth = 0.0f)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);
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

					batch.Draw(glyph.Texture,
						new Vector2(x + q.X0, y + q.Y0),
						sourceRect,
						color,
						0,
						origin,
						scale,
						depth);
				}

				prevGlyph = glyph;
			}

			return x;
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color color, Vector2 scale, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, color, scale, DefaultOrigin, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color color, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, color, DefaultScale, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color[] colors, Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var pos = 0;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

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

					batch.Draw(glyph.Texture,
						new Vector2(x + q.X0, y + q.Y0),
						sourceRect,
						colors[pos],
						0,
						origin,
						scale,
						depth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return x;
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color[] colors, Vector2 scale,
			float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, colors, scale, DefaultOrigin, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, string str, Color[] colors, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, colors, DefaultScale, depth);
		}

		protected abstract void PreDraw(StringBuilder str, out float ascent, out float lineHeight);

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color color, Vector2 scale,
			Vector2 origin, float depth = 0.0f)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

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

					batch.Draw(glyph.Texture,
						new Vector2(x + q.X0, y + q.Y0),
						sourceRect,
						color,
						0,
						origin,
						scale,
						depth);
				}

				prevGlyph = glyph;
			}

			return x;
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color color, Vector2 scale, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, color, scale, DefaultOrigin, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color color, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, color, DefaultScale, DefaultOrigin, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color[] glyphColors,
			Vector2 scale, Vector2 origin, float depth = 0.0f)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			float originX = 0.0f;
			float originY = 0.0f;

			originY += ascent;

			FontGlyph prevGlyph = null;
			var pos = 0;
			var q = new FontGlyphSquad();
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

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

					batch.Draw(glyph.Texture,
						new Vector2(x + q.X0, y + q.Y0),
						sourceRect,
						glyphColors[pos],
						0,
						origin,
						scale,
						depth);
				}

				prevGlyph = glyph;
				++pos;
			}

			return x;
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color[] colors, Vector2 scale,
			float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, colors, scale, DefaultOrigin, depth);
		}

		public float DrawText(IFontStashRenderer batch, float x, float y, StringBuilder str, Color[] colors, float depth = 0.0f)
		{
			return DrawText(batch, x, y, str, colors, DefaultScale, DefaultOrigin, depth);
		}

		public virtual float TextBounds(float x, float y, string str, ref Bounds bounds, Vector2 scale)
		{
			if (string.IsNullOrEmpty(str)) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

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

		public float TextBounds(float x, float y, string str, ref Bounds bounds)
		{
			return TextBounds(x, y, str, ref bounds, DefaultScale);
		}

		public virtual float TextBounds(float x, float y, StringBuilder str, ref Bounds bounds, Vector2 scale)
		{
			if (str == null || str.Length == 0) return 0.0f;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
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

		public float TextBounds(float x, float y, StringBuilder str, ref Bounds bounds)
		{
			return TextBounds(x, y, str, ref bounds, DefaultScale);
		}

		public List<Rectangle> GetGlyphRects(float x, float y, string str, Vector2 scale)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (string.IsNullOrEmpty(str)) return Rects;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
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

		public List<Rectangle> GetGlyphRects(float x, float y, string str)
		{
			return GetGlyphRects(x, y, str, DefaultScale);
		}

		public List<Rectangle> GetGlyphRects(float x, float y, StringBuilder str, Vector2 scale)
		{
			List<Rectangle> Rects = new List<Rectangle>();
			if (str == null || str.Length == 0) return Rects;

			float ascent, lineHeight;
			PreDraw(str, out ascent, out lineHeight);

			var q = new FontGlyphSquad();
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

		public List<Rectangle> GetGlyphRects(float x, float y, StringBuilder str)
		{
			return GetGlyphRects(x, y, str, DefaultScale);
		}

		public Vector2 MeasureString(string text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(0, 0, text, ref bounds, scale);

			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(string text)
		{
			return MeasureString(text, DefaultScale);
		}

		public Vector2 MeasureString(StringBuilder text, Vector2 scale)
		{
			Bounds bounds = new Bounds();
			TextBounds(0, 0, text, ref bounds, scale);

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

			x += glyph.XAdvance;
		}
	}
}
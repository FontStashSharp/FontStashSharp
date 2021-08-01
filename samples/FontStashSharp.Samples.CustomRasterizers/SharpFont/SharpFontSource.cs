using FontStashSharp.Interfaces;
using SharpFont;
using System;

namespace FontStashSharp.SharpFont
{
	public class SharpFontSource : IFontSource
	{
		private const float Dpi = 96;
		private const float PointsPerInch = 72;

		private static readonly Library _library = new Library();
		private Face _face;
		private float AscentBase, DescentBase, LineHeightBase;

		public SharpFontSource(byte[] data)
		{
			_face = new Face(_library, data, 0);

			var fh = _face.Ascender - _face.Descender;
			AscentBase = _face.Ascender / (float)fh;
			DescentBase = _face.Descender / (float)fh;
			LineHeightBase = _face.Height / (float)fh;
		}

		~SharpFontSource()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_face != null)
				{
					_face.Dispose();
					_face = null;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public int? GetGlyphId(int codepoint) => (int?)_face.GetCharIndex((uint)codepoint);

		public int GetGlyphKernAdvance(int previousGlyphId, int glyphId, int fontSize)
		{
			return 0;
		}

		public void GetGlyphMetrics(int glyphId, int fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
		{
			_face.SetPixelSizes(0, (uint)fontSize);
			_face.LoadGlyph((uint)glyphId, LoadFlags.Default, LoadTarget.Normal);

			var glyph = _face.Glyph;
			advance = glyph.Advance.X.Value >> 6;
			x0 = glyph.Metrics.HorizontalBearingX.Value >> 6;
			y0 = -glyph.Metrics.HorizontalBearingY.Value >> 6;
			x1 = x0 + (glyph.Metrics.Width.Value >> 6);
			y1 = y0 + (glyph.Metrics.Height.Value >> 6);
		}

		public void GetMetricsForSize(int fontSize, out float ascent, out float descent, out float lineHeight)
		{
			ascent = fontSize * AscentBase;
			descent = fontSize * DescentBase;
			lineHeight = fontSize * LineHeightBase;
		}

		public unsafe void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
		{
			_face.SetPixelSizes(0, (uint)fontSize);
			_face.LoadGlyph((uint)glyphId, LoadFlags.Default, LoadTarget.Normal);
			_face.Glyph.RenderGlyph(RenderMode.Normal);

			var ftbmp = _face.Glyph.Bitmap;

			fixed (byte* bptr = buffer)
			{
				for (var y = 0; y < outHeight; ++y)
				{
					var pos = (y * outStride) + startIndex;

					byte* dst = bptr + pos;
					byte* src = (byte*)ftbmp.Buffer + y * ftbmp.Pitch;
					for (var x = 0; x < outWidth; ++x)
					{
						*dst++ = *src++;
					}
				}
			}
		}
	}
}
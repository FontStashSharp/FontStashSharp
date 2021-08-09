using FontStashSharp.Interfaces;
using System;
using static StbTrueTypeSharp.StbTrueType;

namespace FontStashSharp
{
	internal unsafe class StbTrueTypeSharpFontSource: IFontSource
	{
		private int? _lastSize;
		private readonly Int32Map<int> _kernings = new Int32Map<int>();
		private readonly StbTrueTypeSharpSettings _settings;
		private int _ascent, _descent, _lineHeight;

		public stbtt_fontinfo _font;

		public StbTrueTypeSharpFontSource(byte[] data, StbTrueTypeSharpSettings settings)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			_font = CreateFont(data, 0);
			if (_font == null)
				throw new Exception("stbtt_InitFont failed");

			_settings = settings;

			int ascent, descent, lineGap;
			stbtt_GetFontVMetrics(_font, &ascent, &descent, &lineGap);

			_ascent = ascent;
			_descent = descent;
			_lineHeight = ascent - descent + lineGap;
		}

		~StbTrueTypeSharpFontSource()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _font != null)
			{
				_font.Dispose();
				_font = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void GetMetrics(out float ascent, out float descent, out float lineHeight)
		{
			ascent = _ascent;
			descent = _descent;
			lineHeight = _lineHeight;
		}

		public int? GetGlyphId(int codepoint)
		{
			var result = stbtt_FindGlyphIndex(_font, codepoint);
			if (result == 0)
			{
				return null;
			}

			return result;
		}

		public void GetGlyphMetrics(int glyphId, int fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
		{
			int advanceTemp, lsbTemp;
			stbtt_GetGlyphHMetrics(_font, glyphId, &advanceTemp, &lsbTemp);

			var scale = stbtt_ScaleForPixelHeight(_font, fontSize);
			advance = (int)(advanceTemp * scale + 0.5f);

			int x0Temp, y0Temp, x1Temp, y1Temp;
			stbtt_GetGlyphBitmapBox(_font, glyphId, scale, scale, &x0Temp, &y0Temp, &x1Temp, &y1Temp);
			x0 = x0Temp;
			y0 = y0Temp;
			x1 = x1Temp + _settings.KernelWidth;
			y1 = y1Temp + _settings.KernelHeight;
		}

		public void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
		{
			fixed (byte* output = &buffer[startIndex])
			{
				var scale = stbtt_ScaleForPixelHeight(_font, fontSize);
				stbtt_MakeGlyphBitmap(_font, output, outWidth, outHeight, outStride, scale, scale, glyphId);
				if(_settings.KernelWidth > 0)
					stbtt__v_prefilter(output, outWidth, outHeight, outStride, (uint)_settings.KernelWidth);
				if(_settings.KernelHeight > 0)
					stbtt__h_prefilter(output, outWidth, outHeight, outStride, (uint)_settings.KernelHeight);
			}
		}

		public int GetGlyphKernAdvance(int glyph1, int glyph2, int fontSize)
		{
			var key = ((glyph1 << 16) | (glyph1 >> 16)) ^ glyph2;
			int result;
			if (!_kernings.TryGetValue(key, out result))
			{
				result = stbtt_GetGlyphKernAdvance(_font, glyph1, glyph2);
				_kernings[key] = result;
			}

			var scale = stbtt_ScaleForPixelHeight(_font, fontSize);
			return (int)(result * scale);
		}
	}
}

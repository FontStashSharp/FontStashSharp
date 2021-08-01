using FontStashSharp.Interfaces;
using StbTrueTypeNative;
using System;

namespace FontStashSharp.StbTrueTypeNative
{
  public class StbTrueTypeNativeSource: IFontSource
  {
		private int? _lastSize;
		private float AscentBase, DescentBase, LineHeightBase;
		private readonly Int32Map<int> _kernings = new Int32Map<int>();
		private readonly FontSystemSettings _settings;

		public float Ascent { get; private set; }
		public float Descent { get; private set; }
		public float LineHeight { get; private set; }
		public float Scale { get; private set; }

		private NativeFont _font;

		public StbTrueTypeNativeSource(byte[] data, FontSystemSettings settings)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_font = new NativeFont(data);
			_settings = settings;
		}

		~StbTrueTypeNativeSource()
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

		public void UpdateSize(int size)
		{
			if (_lastSize == size)
			{
				return;
			}

			Ascent = AscentBase * size;
			Descent = DescentBase * size;
			LineHeight = LineHeightBase * size;
			Scale = _font.ScaleForPixelHeight(size);
			_lastSize = size;
		}

		public void GetMetricsForSize(int fontSize, out float ascent, out float descent, out float lineHeight)
		{
			UpdateSize(fontSize);

			ascent = Ascent;
			descent = Descent;
			lineHeight = LineHeight;
		}

		public int? GetGlyphId(int codepoint)
		{
			var result = _font.FindGlyphIndex(codepoint);
			if (result == 0)
			{
				return null;
			}

			return result;
		}

		public void GetGlyphMetrics(int glyphId, int fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
		{
			UpdateSize(fontSize);

			int advanceTemp, lsbTemp;
			_font.GetGlyphHMetrics(glyphId, out advanceTemp, out lsbTemp);
			advance = (int)(advanceTemp * Scale + 0.5f);

			int x0Temp, y0Temp, x1Temp, y1Temp;
			_font.GetGlyphBitmapBox(glyphId, Scale, Scale, out x0Temp, out y0Temp, out x1Temp, out y1Temp);
			x0 = x0Temp;
			y0 = y0Temp;
			x1 = x1Temp + _settings.KernelWidth;
			y1 = y1Temp + _settings.KernelHeight;
		}

		public unsafe void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
		{
			UpdateSize(fontSize);

			fixed (byte* output = &buffer[startIndex])
			{
				_font.MakeGlyphBitmap(output, outWidth, outHeight, outStride, Scale, Scale, glyphId);
				if (_settings.KernelWidth > 0)
					NativeFont.HorizontalPrefilter(output, outWidth, outHeight, outStride, (uint)_settings.KernelWidth);
				if (_settings.KernelHeight > 0)
					NativeFont.VerticalPrefilter(output, outWidth, outHeight, outStride, (uint)_settings.KernelHeight);
			}
		}

		public int GetGlyphKernAdvance(int glyph1, int glyph2, int fontSize)
		{
			UpdateSize(fontSize);

			var key = ((glyph1 << 16) | (glyph1 >> 16)) ^ glyph2;
			int result;
			if (!_kernings.TryGetValue(key, out result))
			{
				result = _font.GetGlyphKernAdvance(glyph1, glyph2);
				_kernings[key] = result;
			}

			return (int)(result * Scale);
		}

		public static StbTrueTypeNativeSource FromMemory(byte[] data, FontSystemSettings settings)
		{
			var font = new StbTrueTypeNativeSource(data, settings);

			int ascent, descent, lineGap;
			font._font.GetFontVMetrics(out ascent, out descent, out lineGap);

			var fh = ascent - descent;
			font.AscentBase = ascent / (float)fh;
			font.DescentBase = descent / (float)fh;
			font.LineHeightBase = (fh + lineGap) / (float)fh;

			return font;
		}

	}
}

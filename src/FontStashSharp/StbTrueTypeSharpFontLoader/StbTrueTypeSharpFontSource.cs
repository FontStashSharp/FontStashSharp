using FontStashSharp.Interfaces;
using System;
using System.Runtime.InteropServices;
using static StbTrueTypeSharp.StbTrueType;

namespace FontStashSharp
{
	internal unsafe class StbTrueTypeSharpFontSource: IDynamicFontSource
	{
		private int? _lastSize;
		private GCHandle? dataPtr = null;
		private float AscentBase, DescentBase, LineHeightBase;
		readonly Int32Map<int> _kernings = new Int32Map<int>();

		public float Ascent { get; private set; }
		public float Descent { get; private set; }
		public float LineHeight { get; private set; }
		public float Scale { get; private set; }

		public stbtt_fontinfo _font = new stbtt_fontinfo();

		public StbTrueTypeSharpFontSource(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			dataPtr = GCHandle.Alloc(data, GCHandleType.Pinned);
		}

		~StbTrueTypeSharpFontSource()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (dataPtr != null)
			{
				dataPtr.Value.Free();
				dataPtr = null;
			}
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
			Scale = stbtt_ScaleForPixelHeight(_font, size);
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
			var result = stbtt_FindGlyphIndex(_font, codepoint);
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
			stbtt_GetGlyphHMetrics(_font, glyphId, &advanceTemp, &lsbTemp);
			advance = (int)(advanceTemp * Scale + 0.5f);

			int x0Temp, y0Temp, x1Temp, y1Temp;
			stbtt_GetGlyphBitmapBox(_font, glyphId, Scale, Scale, &x0Temp, &y0Temp, &x1Temp, &y1Temp);
			x0 = x0Temp;
			y0 = y0Temp;
			x1 = x1Temp;
			y1 = y1Temp;
		}

		public void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
		{
			UpdateSize(fontSize);

			fixed (byte* output = &buffer[startIndex])
			{
				stbtt_MakeGlyphBitmap(_font, output, outWidth, outHeight, outStride, Scale, Scale, glyphId);
			}
		}

		public void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride, uint kernelWidth)
		{
			UpdateSize(fontSize);

			fixed (byte* output = &buffer[startIndex])
			{
				stbtt_MakeGlyphBitmap(_font, output, outWidth, outHeight, outStride, Scale, Scale, glyphId);
				stbtt__h_prefilter(output, outWidth, outHeight, outStride, kernelWidth);
				stbtt__v_prefilter(output, outWidth, outHeight, outStride, kernelWidth);
			}
		}

		public int GetGlyphKernAdvance(int glyph1, int glyph2, int fontSize)
		{
			UpdateSize(fontSize);

			var key = ((glyph1 << 16) | (glyph1 >> 16)) ^ glyph2;
			int result;
			if (!_kernings.TryGetValue(key, out result))
			{
				result = stbtt_GetGlyphKernAdvance(_font, glyph1, glyph2);
				_kernings[key] = result;
			}
			
			return (int)(result * Scale);
		}

		public static StbTrueTypeSharpFontSource FromMemory(byte[] data)
		{
			var font = new StbTrueTypeSharpFontSource(data);

			byte* dataPtr = (byte *)font.dataPtr.Value.AddrOfPinnedObject();
			if (stbtt_InitFont(font._font, dataPtr, 0) == 0)
				throw new Exception("stbtt_InitFont failed");

			int ascent, descent, lineGap;
			stbtt_GetFontVMetrics(font._font , &ascent, &descent, &lineGap);

			var fh = ascent - descent;
			font.AscentBase = ascent / (float)fh;
			font.DescentBase = descent / (float)fh;
			font.LineHeightBase = (fh + lineGap) / (float)fh;

			return font;
		}
	}
}

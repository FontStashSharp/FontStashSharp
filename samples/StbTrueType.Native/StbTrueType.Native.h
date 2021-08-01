#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

#define STB_TRUETYPE_IMPLEMENTATION
#include "stb_truetype.h"

namespace StbTrueTypeNative {
	public ref class NativeFont
	{
	private:
		unsigned char* _data;
		stbtt_fontinfo* _font;

	public:
		NativeFont(array<unsigned char>^ bytes)
		{
			pin_ptr<unsigned char> p = &bytes[0];
			const unsigned char* ptr = (const unsigned char*)p;

			// Copy data to local buffer
			_data = new unsigned char[bytes->Length];
			memcpy(_data, ptr, bytes->Length);

			_font = new stbtt_fontinfo();
			if (stbtt_InitFont(_font, _data, 0) == 0)
			{
				delete _font;
				_font = nullptr;

				delete[] _data;
				_data = nullptr;
				throw gcnew System::Exception("stbtt_InitFont failed.");
			}
		}

		virtual ~NativeFont()
		{
			if (_font != nullptr)
			{
				delete _font;
				_font = nullptr;
			}

			if (_data != nullptr)
			{
				delete[] _data;
				_data = nullptr;
			}
		}

		float ScaleForPixelHeight(float height)
		{
			return stbtt_ScaleForPixelHeight(_font, height);
		}

		int FindGlyphIndex(int unicode_codepoint)
		{
			return stbtt_FindGlyphIndex(_font, unicode_codepoint);
		}

		void GetGlyphHMetrics(int glyph_index, [Out] int %advanceWidth, [Out] int %leftSideBearing)
		{
			int aw, lsb;
			stbtt_GetGlyphHMetrics(_font, glyph_index, &aw, &lsb);
			advanceWidth = aw;
			leftSideBearing = lsb;
		}

		void GetGlyphBitmapBox(int glyph, float scale_x, float scale_y, [Out] int% ix0, [Out] int% iy0, [Out] int% ix1, [Out] int% iy1)
		{
			int x0, y0, x1, y1;

			stbtt_GetGlyphBitmapBox(_font, glyph, scale_x, scale_y, &x0, &y0, &x1, &y1);

			ix0 = x0;
			iy0 = y0;
			ix1 = x1;
			iy1 = y1;
		}

		void MakeGlyphBitmap(unsigned char* output, int out_w, int out_h, int out_stride, float scale_x, float scale_y, int glyph)
		{
			stbtt_MakeGlyphBitmap(_font, output, out_w, out_h, out_stride, scale_x, scale_y, glyph);
		}

		int GetGlyphKernAdvance(int g1, int g2)
		{
			return stbtt_GetGlyphKernAdvance(_font, g1, g2);
		}

		void GetFontVMetrics([Out] int% ascent, [Out] int% descent, [Out] int% lineGap)
		{
			int a, d, l;
			stbtt_GetFontVMetrics(_font, &a, &d, &l);

			ascent = a;
			descent = d;
			lineGap = l;
		}

		static void HorizontalPrefilter(unsigned char* pixels, int w, int h, int stride_in_bytes, unsigned int kernel_width)
		{
			stbtt__h_prefilter(pixels, w, h, stride_in_bytes, kernel_width);
		}

		static void VerticalPrefilter(unsigned char* pixels, int w, int h, int stride_in_bytes, unsigned int kernel_width)
		{
			stbtt__v_prefilter(pixels, w, h, stride_in_bytes, kernel_width);
		}
	};
}

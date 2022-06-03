using FontStashSharp.Interfaces;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using System;
using System.Runtime.InteropServices;

namespace FontStashSharp.Rasterizers.FreeType
{
	internal class FreeTypeSource: IFontSource
	{
		private static IntPtr _libraryHandle;
		private GCHandle _memoryHandle;
		private IntPtr _faceHandle;
		private readonly FT_FaceRec _rec;

		public FreeTypeSource(byte[] data)
		{
			FT_Error err;
			if (_libraryHandle == IntPtr.Zero)
			{
				IntPtr libraryRef;
				err = FT.FT_Init_FreeType(out libraryRef);

				if (err != FT_Error.FT_Err_Ok)
					throw new FreeTypeException(err);

				_libraryHandle = libraryRef;
			}

			_memoryHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

			IntPtr faceRef;
			err = FT.FT_New_Memory_Face(_libraryHandle, _memoryHandle.AddrOfPinnedObject(), data.Length, 0, out faceRef);

			if (err != FT_Error.FT_Err_Ok)
				throw new FreeTypeException(err);

			_faceHandle = faceRef;
			_rec = PInvokeHelper.PtrToStructure<FT_FaceRec>(_faceHandle);
		}

		~FreeTypeSource()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_faceHandle != IntPtr.Zero)
			{
				FT.FT_Done_Face(_faceHandle);
				_faceHandle = IntPtr.Zero;
			}

			if (_memoryHandle.IsAllocated)
				_memoryHandle.Free();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public int? GetGlyphId(int codepoint)
		{
			var result = FT.FT_Get_Char_Index(_faceHandle, (uint)codepoint);
			if (result == 0)
			{
				return null;
			}

			return (int?)result;
		}

		public int GetGlyphKernAdvance(int previousGlyphId, int glyphId, int fontSize)
		{
			FT_Vector kerning;
			if (FT.FT_Get_Kerning(_faceHandle, (uint)previousGlyphId, (uint)glyphId, 0, out kerning) != FT_Error.FT_Err_Ok)
			{
				return 0;
			}

			return (int)kerning.x >> 6;
		}

		private void SetPixelSizes(int width, int height)
		{
			var err = FT.FT_Set_Pixel_Sizes(_faceHandle, (uint)width, (uint)height);
			if (err != FT_Error.FT_Err_Ok)
				throw new FreeTypeException(err);
		}

		private void LoadGlyph(int glyphId)
		{
			var err = FT.FT_Load_Glyph(_faceHandle, (uint)glyphId, FT.FT_LOAD_DEFAULT | FT.FT_LOAD_TARGET_NORMAL);
			if (err != FT_Error.FT_Err_Ok)
				throw new FreeTypeException(err);
		}

		private unsafe void GetCurrentGlyph(out FT_GlyphSlotRec glyph)
		{
			glyph = PInvokeHelper.PtrToStructure<FT_GlyphSlotRec>((IntPtr)_rec.glyph);
		}

		public void GetGlyphMetrics(int glyphId, int fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
		{
			SetPixelSizes(0, fontSize);
			LoadGlyph(glyphId);

			FT_GlyphSlotRec glyph;
			GetCurrentGlyph(out glyph);
			advance = (int)glyph.advance.x >> 6;
			x0 = (int)glyph.metrics.horiBearingX >> 6;
			y0 = -(int)glyph.metrics.horiBearingY >> 6;
			x1 = x0 + ((int)glyph.metrics.width >> 6);
			y1 = y0 + ((int)glyph.metrics.height >> 6);
		}

		public unsafe void GetMetricsForSize(int fontSize, out int ascent, out int descent, out int lineHeight)
		{
			SetPixelSizes(0, fontSize);
			var sizeRec = PInvokeHelper.PtrToStructure<FT_SizeRec>((IntPtr)_rec.size);

			ascent = (int)sizeRec.metrics.ascender >> 6;
			descent = (int)sizeRec.metrics.descender >> 6;
			lineHeight = (int)sizeRec.metrics.height >> 6;
		}

		public unsafe void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
		{
			SetPixelSizes(0, fontSize);
			LoadGlyph(glyphId);

			FT.FT_Render_Glyph((IntPtr)_rec.glyph, FT_Render_Mode.FT_RENDER_MODE_NORMAL);

			FT_GlyphSlotRec glyph;
			GetCurrentGlyph(out glyph);
			var ftbmp = glyph.bitmap;

			fixed (byte* bptr = buffer)
			{
				for (var y = 0; y < outHeight; ++y)
				{
					var pos = (y * outStride) + startIndex;

					byte* dst = bptr + pos;
					byte* src = (byte*)ftbmp.buffer + y * ftbmp.pitch;
					for (var x = 0; x < outWidth; ++x)
					{
						*dst++ = *src++;
					}
				}
			}
		}
	}
}

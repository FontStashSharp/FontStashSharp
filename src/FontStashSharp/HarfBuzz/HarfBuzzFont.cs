using System;
using System.Runtime.InteropServices;
using HarfBuzzSharp;

namespace FontStashSharp.HarfBuzz
{
	/// <summary>
	/// Wrapper for HarfBuzz font resources
	/// </summary>
	internal class HarfBuzzFont : IDisposable
	{
		private Blob _blob;
		private Face _face;
		private Font _font;
		private GCHandle _fontDataHandle;
		private int _currentScale = -1;

		public Font Font => _font;

		public HarfBuzzFont(byte[] fontData)
		{
			// Pin the byte array in memory
			_fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
			var dataPtr = _fontDataHandle.AddrOfPinnedObject();

			// Create HarfBuzz blob from font data
			_blob = new Blob(dataPtr, fontData.Length, MemoryMode.ReadOnly);

			// Create face from blob and font from face
			_face = new Face(_blob, 0);
			_font = new Font(_face);
		}

		/// <summary>
		/// Set the font scale for a specific font size
		/// </summary>
		public void SetScale(float fontSize)
		{
			// Convert fontSize to font units
			// Set scale to 26.6 fixed-point pixels (fontSize * 64)
			// HarfBuzz will then output positions in 26.6 format directly
			var scale = (int)(fontSize * 64);

			// Skip if scale hasn't changed
			if (_currentScale == scale)
			{
				return;
			}

			_currentScale = scale;
			_font.SetScale(scale, scale);
		}

		/// <summary>
		/// Shape text using this font
		/// </summary>
		public void Shape(HarfBuzzSharp.Buffer buffer)
		{
			_font.Shape(buffer);
		}

		public void Dispose()
		{
			_font?.Dispose();
			_face?.Dispose();
			_blob?.Dispose();

			if (_fontDataHandle.IsAllocated)
			{
				_fontDataHandle.Free();
			}
		}
	}
}
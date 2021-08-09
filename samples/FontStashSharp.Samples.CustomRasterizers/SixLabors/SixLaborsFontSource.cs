using FontStashSharp.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using TrippyGL.Fonts.Building;

namespace FontStashSharp.Samples.SixLabors
{
	public class SixLaborsFontSource : IFontSource
	{
		private FontGlyphSource _source;
		private float _ascent, _descent, _lineHeight;

		public SixLaborsFontSource(byte[] data)
		{
			FontInstance fontInstance;
			using (var ms = new MemoryStream(data))
			{
				fontInstance = FontInstance.LoadFont(ms);
				_source = new FontGlyphSource(fontInstance);
				_ascent = fontInstance.Ascender;
				_descent = fontInstance.Descender;
				_lineHeight = fontInstance.LineHeight;
			}
		}

		public void Dispose()
		{
		}

		public int? GetGlyphId(int codepoint)
		{
			return codepoint;
		}

		public int GetGlyphKernAdvance(int previousGlyphId, int glyphId, int fontSize)
		{
			var kerning = _source.GetKerning(fontSize, previousGlyphId, glyphId);
			return (int)kerning.X;
		}

		public void GetGlyphMetrics(int glyphId, int fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
		{
			var path = _source.CreatePath(fontSize, glyphId);

			advance = (int)_source.GetAdvance(fontSize, glyphId);
			x0 = path.Bounds.X;
			y0 = path.Bounds.Y;
			x1 = path.Bounds.Right;
			y1 = path.Bounds.Bottom;
		}

		public void GetMetrics(out float ascent, out float descent, out float lineHeight)
		{
			ascent = _ascent;
			descent = _descent;
			lineHeight = _lineHeight;
		}

		public void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
		{
			var path = _source.CreatePath(fontSize, glyphId);
			Image<Rgba32> image = new Image<Rgba32>(path.Bounds.Width, path.Bounds.Height);
			_source.DrawGlyphToImage(path, new System.Drawing.Point(0, 0), image);

			for(var y = 0; y < outHeight; ++y)
			{
				var pos = (y * outStride) + startIndex;
				for (var x = 0; x < outWidth; ++x)
				{
					var color = image[x, y];
					buffer[pos] = color.A;
					++pos;
				}
			}
		}
	}
}

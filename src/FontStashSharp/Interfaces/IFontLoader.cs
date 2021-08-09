using System;

namespace FontStashSharp.Interfaces
{
	public interface IFontSource: IDisposable
	{
		/// <summary>
		/// Returns font metrics for the specified font size
		/// </summary>
		/// <param name="fontSize"></param>
		/// <param name="ascent"></param>
		/// <param name="descent"></param>
		/// <param name="lineHeight"></param>
		void GetMetricsForSize(int fontSize, out float ascent, out float descent, out float lineHeight);

		/// <summary>
		/// Returns Id of a glyph corresponding to a codepoint
		/// Null if the codepoint can't be rasterized
		/// </summary>
		/// <param name="codepoint"></param>
		/// <returns></returns>
		int? GetGlyphId(int codepoint);

		/// <summary>
		/// Returns glyph metrics
		/// </summary>
		/// <param name="glyphId"></param>
		/// <param name="fontSize"></param>
		/// <param name="advance"></param>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		void GetGlyphMetrics(int glyphId, int fontSize, out int advance, out int x0, out int y0, out int x1, out int y1);

		/// <summary>
		/// Renders a glyph 
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="fontSize"></param>
		/// <param name="startIndex"></param>
		/// <param name="outWidth"></param>
		/// <param name="outHeight"></param>
		/// <param name="outStride"></param>
		void RasterizeGlyphBitmap(int glyphId, int fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride);

		/// <summary>
		/// Returns kerning
		/// </summary>
		/// <param name="previousGlyphId"></param>
		/// <param name="glyphId"></param>
		/// <param name="fontSize"></param>
		/// <returns></returns>
		int GetGlyphKernAdvance(int previousGlyphId, int glyphId, int fontSize);
	}

	/// <summary>
	/// Font Rasterization Service
	/// </summary>
	public interface IFontLoader
	{
		IFontSource Load(byte[] data);
	}
}

namespace FontStashSharp
{
	/// <summary>
	/// Font Rasterization Service
	/// </summary>
	public unsafe interface IFontRasterizationService
	{
		void RenderGlyphBitmap(byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride, int codepoint);
	}
}

namespace FontStashSharp.HarfBuzz
{
	/// <summary>
	/// Represents a single shaped glyph from HarfBuzz
	/// </summary>
	public class ShapedGlyph
	{
		/// <summary>
		/// The glyph ID from the font
		/// </summary>
		public int GlyphId;

		/// <summary>
		/// The cluster index in the original text
		/// </summary>
		public int Cluster;

		/// <summary>
		/// Which font source this glyph is from (for font fallback)
		/// </summary>
		public int FontSourceIndex;

		/// <summary>
		/// Horizontal advance in font units
		/// </summary>
		public int XAdvance;

		/// <summary>
		/// Vertical advance in font units
		/// </summary>
		public int YAdvance;

		/// <summary>
		/// Horizontal offset in font units
		/// </summary>
		public int XOffset;

		/// <summary>
		/// Vertical offset in font units
		/// </summary>
		public int YOffset;
	}

	/// <summary>
	/// Contains the result of HarfBuzz text shaping
	/// </summary>
	public class ShapedText
	{
		/// <summary>
		/// The shaped glyphs in visual order
		/// </summary>
		public ShapedGlyph[] Glyphs;

		/// <summary>
		/// The original text that was shaped
		/// </summary>
		public string OriginalText;

		/// <summary>
		/// Font size used for shaping
		/// </summary>
		public float FontSize;
	}
}
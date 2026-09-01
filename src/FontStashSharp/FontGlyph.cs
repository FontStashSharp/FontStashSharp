#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using Texture2D = System.Object;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Represents a rendered glyph with texture and layout information.
	/// </summary>
	public class FontGlyph
	{
		/// <summary>
		/// The Unicode codepoint of the character.
		/// </summary>
		public int Codepoint;
		/// <summary>
		/// A unique identifier for this glyph.
		/// </summary>
		public int Id;
		/// <summary>
		/// The horizontal advance width in pixels.
		/// </summary>
		public int XAdvance;
		/// <summary>
		/// The texture containing the glyph image.
		/// </summary>
		public Texture2D Texture;
		/// <summary>
		/// The offset from the baseline for rendering the glyph.
		/// </summary>
		public Point RenderOffset;
		/// <summary>
		/// The position of the glyph within the texture.
		/// </summary>
		public Point TextureOffset;
		/// <summary>
		/// The width and height of the glyph in pixels.
		/// </summary>
		public Point Size;

		/// <summary>
		/// The mode used to rasterize this glyph.
		/// </summary>
		public FontRasterizationMode FontRasterizationMode = FontRasterizationMode.Standard;

		/// <summary>
		/// Gets a value indicating whether this glyph has no visible content.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return Size.X == 0 || Size.Y == 0;
			}
		}

		/// <summary>
		/// Gets the rectangular region within the texture containing this glyph.
		/// </summary>
		public Rectangle TextureRectangle => new Rectangle(TextureOffset.X, TextureOffset.Y, Size.X, Size.Y);
		/// <summary>
		/// Gets the rectangular region where this glyph should be rendered.
		/// </summary>
		public Rectangle RenderRectangle => new Rectangle(RenderOffset.X, RenderOffset.Y, Size.X, Size.Y);
	}

	/// <summary>
	/// Represents a dynamically rendered glyph with additional font and effect information.
	/// </summary>
	public class DynamicFontGlyph : FontGlyph
	{
		/// <summary>
		/// The font size at which this glyph was rendered.
		/// </summary>
		public float FontSize;
		/// <summary>
		/// The index of the font source used to render this glyph.
		/// </summary>
		public int FontSourceIndex;
		/// <summary>
		/// The effect applied to this glyph (None, Blurry, or Stroked).
		/// </summary>
		public FontSystemEffect Effect;
		/// <summary>
		/// The strength or intensity of the applied effect.
		/// </summary>
		public int EffectAmount;
	}
}

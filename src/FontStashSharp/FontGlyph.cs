using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp
{
	public class FontGlyph
  {
		public int Codepoint;
		public int Id;
		public Rectangle Bounds;
		public int XAdvance;
		public int XOffset;
		public int YOffset;
		public ITexture2D Texture;

		public bool IsEmpty
		{
			get
			{
				return Bounds.Width == 0 || Bounds.Height == 0;
			}
		}
	}

	public class DynamicFontGlyph: FontGlyph
	{
		public int Size;
		public IFontSource Font;

		public static int PadFromBlur(int blur)
		{
			return blur + 2;
		}
	}
}

using FontStashSharp.Interfaces;
using System.Drawing;

namespace FontStashSharp
{
	public class FontGlyph
	{
		public int Codepoint;
		public int Id;
		public int Size;
		public IFont Font;
		public FontAtlas Atlas;
		public Rectangle Bounds;
		public int XAdvance;
		public int XOffset;
		public int YOffset;

		public static int PadFromBlur(int blur)
		{
			return blur + 2;
		}

		public bool IsEmpty
		{
			get
			{
				return Bounds.Width == 0 || Bounds.Height == 0;
			}
		}
	}
}

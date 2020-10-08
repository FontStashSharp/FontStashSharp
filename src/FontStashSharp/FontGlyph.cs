using System.Drawing;

namespace FontStashSharp
{
	public class FontGlyph
	{
		public FontAtlas Atlas;
		public int Codepoint;
		public int Index;
		public int Size;
		public Rectangle Bounds;
		public int XAdvance;
		public int XOffset;
		public int YOffset;
		public float Ascent;
		public float LineHeight;

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

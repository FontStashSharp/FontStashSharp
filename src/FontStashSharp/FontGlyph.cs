using FontStashSharp.Interfaces;

#if MONOGAME || FNA
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
	public class FontGlyph
	{
		public int Codepoint;
		public int Id;
		public Rectangle Bounds;
		public int XAdvance;
		public int XOffset;
		public int YOffset;
		public Texture2D Texture;

		public bool IsEmpty
		{
			get
			{
				return Bounds.Width == 0 || Bounds.Height == 0;
			}
		}
	}

	public class DynamicFontGlyph : FontGlyph
	{
		public int Size;
		public int FontSourceIndex;
	}
}

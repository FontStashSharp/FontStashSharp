#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp.RichText
{
	public class GlyphInfo
	{
		public int Index;
		public char Character;
		public Rectangle Bounds;
		public TextChunk TextChunk;
	}
}

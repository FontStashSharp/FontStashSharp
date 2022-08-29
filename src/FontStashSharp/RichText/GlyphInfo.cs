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
		public int Index { get; internal set; }
		public char Character { get; internal set; }
		public int LineTop { get; internal set; }
		public Rectangle Bounds { get; internal set; }
		public TextChunk TextChunk { get; internal set; }
	}
}

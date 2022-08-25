#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp.RichText
{
	internal enum ChunkInfoType
	{
		Text,
		Space,
		Image
	}

	internal struct ChunkInfo
	{
		public ChunkInfoType Type;
		public int X;
		public int Y;
		public int StartIndex;
		public int CharsCount;
		public int SkipCount;
		public bool LineEnd;
		public Texture2D Texture;
		public Rectangle TextureRegion;
	}
}
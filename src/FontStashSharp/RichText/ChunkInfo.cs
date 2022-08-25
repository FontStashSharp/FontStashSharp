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
		public bool LineEnd;
		public string Text;
		public Texture2D Texture;
		public Rectangle TextureRegion;

		public int Width
		{
			get
			{
				if (Type == ChunkInfoType.Image)
				{
					return TextureRegion.Width;
				}

				return X;
			}
		}

		public int Height
		{
			get
			{
				if (Type == ChunkInfoType.Image)
				{
					return TextureRegion.Height;
				}

				return Y;
			}
		}

	}
}

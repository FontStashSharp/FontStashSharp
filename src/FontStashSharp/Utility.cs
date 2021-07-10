using System.IO;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Numerics;
#endif


namespace FontStashSharp
{
	internal static class Utility
	{
		public static readonly Vector2 Vector2Zero = new Vector2(0, 0);

		public static byte[] ToByteArray(this Stream stream)
		{
			byte[] bytes;

			// Rewind stream if it is at end
			if (stream.CanSeek && stream.Length == stream.Position)
			{
				stream.Seek(0, SeekOrigin.Begin);
			}

			// Copy it's data to memory
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				bytes = ms.ToArray();
			}

			return bytes;
		}

		public static Point Size(this Texture2D texture)
		{
			return new Point(texture.Width, texture.Height);
		}
	}
}
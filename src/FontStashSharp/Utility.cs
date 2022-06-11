using System.IO;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Numerics;
using System.Drawing;
using Matrix = System.Numerics.Matrix3x2;
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

		public static Vector2 Transform(this Vector2 v, ref Matrix matrix)
		{
#if MONOGAME || FNA
			Vector2 result;
			Vector2.Transform(ref v, ref matrix, out result);
			return result;
#elif STRIDE
			Vector4 result;
			Vector2.Transform(ref v, ref matrix, out result);
			return new Vector2(result.X, result.Y);
#else
			return Vector2.Transform(v, matrix);
#endif
		}

		public static Vector3 TransformToVector3(this Vector2 v, ref Matrix matrix, float z)
		{
			var result = v.Transform(ref matrix);
			return new Vector3(result.X, result.Y, z);
		}
	}
}
using System;
using System.IO;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Numerics;
using System.Drawing;
using Matrix = System.Numerics.Matrix3x2;
using Texture2D = System.Object;
#endif


namespace FontStashSharp
{
	internal static class Utility
	{
		public static readonly Point PointZero = new Point(0, 0);
		public static readonly Vector2 Vector2Zero = new Vector2(0, 0);
		public static readonly Vector2 DefaultScale = new Vector2(1.0f, 1.0f);
		public static readonly Vector2 DefaultOrigin = new Vector2(0.0f, 0.0f);

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

		public static int Length(this string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return 0;
			}

			return s.Length;
		}

		public static void BuildTransform(Vector2 position, Vector2 scale, float rotation, Vector2 origin, out Matrix transformation)
		{
			// This code had been borrowed from MonoGame's SpriteBatch.DrawString
			transformation = Matrix.Identity;

			float offsetX, offsetY;
			if (rotation == 0)
			{
				transformation.M11 = scale.X;
				transformation.M22 = scale.Y;
				offsetX = position.X - (origin.X * transformation.M11);
				offsetY = position.Y - (origin.Y * transformation.M22);
			}
			else
			{
				var cos = (float)Math.Cos(rotation);
				var sin = (float)Math.Sin(rotation);
				transformation.M11 = scale.X * cos;
				transformation.M12 = scale.X * sin;
				transformation.M21 = scale.Y * -sin;
				transformation.M22 = scale.Y * cos;
				offsetX = position.X - (origin.X * transformation.M11) - (origin.Y * transformation.M21);
				offsetY = position.Y - (origin.X * transformation.M12) - (origin.Y * transformation.M22);
			}

#if MONOGAME || FNA || STRIDE
			transformation.M41 = offsetX;
			transformation.M42 = offsetY;
#else
			transformation.M31 = offsetX;
			transformation.M32 = offsetY;
#endif
		}

		public static void DrawQuad(this IFontStashRenderer2 renderer,
			Texture2D texture, Color color,
			Vector2 baseOffset, ref Matrix transformation, float layerDepth,
			Point size, Rectangle textureRectangle,
			ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
			ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
		{
#if MONOGAME || FNA || STRIDE
			var textureSize = new Point(texture.Width, texture.Height);
			var setColor = color;
#else
			var textureSize = renderer.TextureManager.GetTextureSize(texture);
			var setColor = (uint)(color.A << 24 | color.B << 16 | color.G << 8 | color.R);
#endif

			topLeft.Position = baseOffset.TransformToVector3(ref transformation, layerDepth);
			topLeft.TextureCoordinate = new Vector2((float)textureRectangle.X / textureSize.X,
													(float)textureRectangle.Y / textureSize.Y);
			topLeft.Color = setColor;

			topRight.Position = (baseOffset + new Vector2(size.X, 0)).TransformToVector3(ref transformation, layerDepth);
			topRight.TextureCoordinate = new Vector2((float)textureRectangle.Right / textureSize.X,
												 (float)textureRectangle.Y / textureSize.Y);
			topRight.Color = setColor;

			bottomLeft.Position = (baseOffset + new Vector2(0, size.Y)).TransformToVector3(ref transformation, layerDepth);
			bottomLeft.TextureCoordinate = new Vector2((float)textureRectangle.Left / textureSize.X,
														 (float)textureRectangle.Bottom / textureSize.Y);
			bottomLeft.Color = setColor;

			bottomRight.Position = (baseOffset + new Vector2(size.X, size.Y)).TransformToVector3(ref transformation, layerDepth);
			bottomRight.TextureCoordinate = new Vector2((float)textureRectangle.Right / textureSize.X,
														(float)textureRectangle.Bottom / textureSize.Y);
			bottomRight.Color = setColor;

			renderer.DrawQuad(texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
		}
	}
}
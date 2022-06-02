#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Texture2D = System.Object;
#endif

namespace FontStashSharp.Interfaces
{
#if PLATFORM_AGNOSTIC
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColorTexture
	{
		public Vector3 Position;
		public Color Color;
		public Vector2 TextureCoordinate;

		public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate)
		{
			Position = position;
			Color = color;
			TextureCoordinate = textureCoordinate;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Position.GetHashCode();
				hashCode = (hashCode * 397) ^ Color.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
				return hashCode;
			}
		}
	
		public override string ToString()
		{
			return "{{Position:" + this.Position + " Color:" + this.Color + " TextureCoordinate:" + this.TextureCoordinate + "}}";
		}

		public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
		{
			return (((left.Position == right.Position) && (left.Color == right.Color)) && (left.TextureCoordinate == right.TextureCoordinate));
		}

		public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType() != base.GetType())
				return false;

			return (this == ((VertexPositionColorTexture)obj));
		}
	}
#endif

	public interface IFontStashRenderer2
	{
#if MONOGAME || FNA || STRIDE
		GraphicsDevice GraphicsDevice { get; }
#else
		ITexture2DManager TextureManager { get; }
#endif

		Texture2D Texture { set; }

		void AddVertex(VertexPositionColorTexture vertex);
	}
}

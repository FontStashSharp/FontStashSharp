#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Numerics;
using Texture2D = System.Object;
using System.Runtime.InteropServices;
#endif

namespace FontStashSharp.Interfaces
{
#if PLATFORM_AGNOSTIC
	/// <summary>
	/// Represents a single vertex with position, color, and texture coordinate information.
	/// Used for rendering textured quads in a platform-agnostic manner.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColorTexture
	{
		/// <summary>
		/// The 3D position of the vertex.
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// The color to apply at this vertex.
		/// </summary>
		public FSColor Color;

		/// <summary>
		/// The texture coordinate (UV) at this vertex.
		/// </summary>
		public Vector2 TextureCoordinate;

		/// <summary>
		/// Initializes a new instance of the VertexPositionColorTexture struct.
		/// </summary>
		/// <param name="position">The 3D position of the vertex.</param>
		/// <param name="color">The color to apply at the vertex.</param>
		/// <param name="texCoord">The texture coordinate (UV) for the vertex.</param>
		public VertexPositionColorTexture(Vector3 position, FSColor color, Vector2 texCoord)
		{
			Position = position;
			Color = color;
			TextureCoordinate = texCoord;
		}
	}
#endif

	/// <summary>
	/// Provides advanced rendering capabilities for drawing textured quads with individual vertex control.
	/// </summary>
	public interface IFontStashRenderer2
	{
#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Gets the graphics device used for rendering.
		/// </summary>
		GraphicsDevice GraphicsDevice { get; }
#else
		/// <summary>
		/// Gets the texture manager for managing 2D textures in a platform-agnostic manner.
		/// </summary>
		ITexture2DManager TextureManager { get; }
#endif

		/// <summary>
		/// Draws a textured quad using four vertices with independent positioning, coloring, and texture coordinates.
		/// </summary>
		/// <param name="texture">The texture to apply to the quad.</param>
		/// <param name="topLeft">The top-left vertex of the quad.</param>
		/// <param name="topRight">The top-right vertex of the quad.</param>
		/// <param name="bottomLeft">The bottom-left vertex of the quad.</param>
		/// <param name="bottomRight">The bottom-right vertex of the quad.</param>
		void DrawQuad(Texture2D texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight);
	}
}

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using Color = FontStashSharp.FSColor;
using System.Numerics;
#endif

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Base class for chunks of content (text, space, or image) in a rich text layout.
	/// </summary>
	public abstract class BaseChunk
	{
		/// <summary>
		/// Gets the size of this chunk in pixels.
		/// </summary>
		public abstract Point Size { get; }

		/// <summary>
		/// Gets or sets the index of the line containing this chunk.
		/// </summary>
		public int LineIndex { get; internal set; }
		/// <summary>
		/// Gets or sets the index of this chunk within its line.
		/// </summary>
		public int ChunkIndex { get; internal set; }
		/// <summary>
		/// Gets or sets the vertical offset of this chunk from the baseline.
		/// </summary>
		public int VerticalOffset { get; internal set; }
		/// <summary>
		/// Gets or sets the color to apply when drawing this chunk, or null to use the default color.
		/// </summary>
		public Color? Color { get; set; }

		/// <summary>
		/// Initializes a new instance of the BaseChunk class.
		/// </summary>
		protected BaseChunk()
		{
		}

		/// <summary>
		/// Draws this chunk at the specified position.
		/// </summary>
		/// <param name="context">The rendering context.</param>
		/// <param name="position">The position to draw at.</param>
		/// <param name="color">The color to apply.</param>
		public abstract void Draw(IFSRenderContext context, Vector2 position, Color color);
	}
}

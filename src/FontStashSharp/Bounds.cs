#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Numerics;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Represents rectangular bounds with X, Y, X2, Y2 coordinates.
	/// </summary>
	public struct Bounds
	{
		/// <summary>
		/// An empty bounds structure with all values set to zero.
		/// </summary>
		public static readonly Bounds Empty = new Bounds
		{
			X = 0,
			Y = 0,
			X2 = 0,
			Y2 = 0,
		};

		/// <summary>
		/// The X coordinate of the top-left corner.
		/// </summary>
		public float X;
		/// <summary>
		/// The Y coordinate of the top-left corner.
		/// </summary>
		public float Y;
		/// <summary>
		/// The X coordinate of the bottom-right corner.
		/// </summary>
		public float X2;
		/// <summary>
		/// The Y coordinate of the bottom-right corner.
		/// </summary>
		public float Y2;

		/// <summary>
		/// Initializes a new instance of the Bounds struct with the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate of the top-left corner.</param>
		/// <param name="y">The Y coordinate of the top-left corner.</param>
		/// <param name="x2">The X coordinate of the bottom-right corner.</param>
		/// <param name="y2">The Y coordinate of the bottom-right corner.</param>
		public Bounds(float x, float y, float x2, float y2)
		{
			X = x;
			Y = y;
			X2 = x2;
			Y2 = y2;
		}

		/// <summary>
		/// Applies a scale factor to all coordinates of this bounds.
		/// </summary>
		/// <param name="scale">The scale factors to apply to X and Y coordinates.</param>
		public void ApplyScale(Vector2 scale)
		{
			X *= scale.X;
			Y *= scale.Y;
			X2 *= scale.X;
			Y2 *= scale.Y;
		}
	}
}

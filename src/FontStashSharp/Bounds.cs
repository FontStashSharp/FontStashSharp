#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Numerics;
#endif

namespace FontStashSharp
{
	public struct Bounds
	{
		public float X, Y, X2, Y2;

		public void ApplyScale(Vector2 scale)
		{
			X *= scale.X;
			Y *= scale.Y;
			X2 *= scale.X;
			Y2 *= scale.Y;
		}
	}
}

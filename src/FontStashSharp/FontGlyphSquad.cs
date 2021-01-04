using System.Runtime.InteropServices;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Numerics;
#endif

namespace FontStashSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct FontGlyphSquad
	{
		public float X0;
		public float Y0;
		public float S0;
		public float T0;
		public float X1;
		public float Y1;
		public float S1;
		public float T1;
		public Vector2 Offset;
	}
}

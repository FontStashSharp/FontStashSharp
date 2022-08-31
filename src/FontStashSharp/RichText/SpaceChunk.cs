using System;
using System.Collections.Generic;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
using Clr = Stride.Core.Mathematics.Color;
#else
using System.Drawing;
using Clr = System.Drawing.Color;
using System.Numerics;
#endif

namespace FontStashSharp.RichText
{
	public class SpaceChunk : BaseChunk
	{
		private readonly int _width;

		public override Point Size => new Point(_width, 0);

		public SpaceChunk(int width)
		{
			_width = width;
		}

		public override void Draw(IFontStashRenderer renderer, Vector2 position, Color color, Vector2 scale, float rotation, float layerDepth)
		{
		}

		public override void Draw(IFontStashRenderer2 renderer, Vector2 position, Color color, Vector2 scale, float rotation, float layerDepth)
		{
		}
	}
}

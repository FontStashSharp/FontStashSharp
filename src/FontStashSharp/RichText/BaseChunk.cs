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
	public abstract class BaseChunk
	{
		public abstract Point Size { get; }

		public int LineIndex { get; internal set; }
		public int ChunkIndex { get; internal set; }
		public int VerticalOffset { get; internal set; }
		public Color? Color { get; set; }

		protected BaseChunk()
		{
		}

		public abstract void Draw(IFontStashRenderer renderer, Vector2 position, Color color,
			Vector2 scale, float rotation, float layerDepth);

		public abstract void Draw(IFontStashRenderer2 renderer, Vector2 position, Color color,
			Vector2 scale, float rotation, float layerDepth);
	}
}

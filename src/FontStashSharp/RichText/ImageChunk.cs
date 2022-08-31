using System;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Clr = Microsoft.Xna.Framework.Color;
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
	public class ImageChunk : BaseChunk
	{
		private readonly IRenderable _renderable;

		public override Point Size => _renderable.Size;

		public ImageChunk(IRenderable renderable)
		{
			if (renderable == null)
			{
				throw new ArgumentNullException(nameof(renderable));
			}

			_renderable = renderable;
		}

		public override void Draw(IFontStashRenderer renderer, Vector2 position, Clr color, Vector2 scale, float rotation, float layerDepth)
		{
			_renderable.Draw(renderer, position, Clr.White, scale, rotation, layerDepth);
		}

		public override void Draw(IFontStashRenderer2 renderer, Vector2 position, Clr color, Vector2 scale, float rotation, float layerDepth)
		{
			_renderable.Draw(renderer, position, Clr.White, scale, rotation, layerDepth);
		}
	}
}

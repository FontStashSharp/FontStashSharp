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
		private readonly Texture2D _texture;
		private readonly Rectangle _region;
		
		public override Point Size => new Point(_region.Width, _region.Height);

		public ImageChunk(Texture2D texture, Rectangle region)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			_texture = texture;
			_region = region;
		}

		public override void Draw(IFontStashRenderer renderer, Vector2 position, Color color, Vector2? sourceScale, float rotation, float layerDepth)
		{
			var scale = sourceScale ?? Vector2.One;
			renderer.Draw(_texture, position, _region, Clr.White, rotation, scale, layerDepth);
		}
	}
}

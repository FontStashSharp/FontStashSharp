using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp.RichText
{
	public interface IRenderable
	{
		Point Size { get; }

		void Draw(IFontStashRenderer renderer, Vector2 position, Color color, Vector2 scale, float rotation, float layerDepth);
	}
}

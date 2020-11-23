#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp.Interfaces
{
	public interface IFontStashRenderer
	{
		void Draw(ITexture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation,
			Vector2 origin, Vector2 scale, float depth);
	}
}

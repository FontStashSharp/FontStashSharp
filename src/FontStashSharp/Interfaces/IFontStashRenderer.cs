#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using Vector2 = System.Drawing.PointF;
#endif

namespace FontStashSharp.Interfaces
{
	public interface IFontStashRenderer
	{
		void Draw(ITexture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation,
			Vector2 origin, Vector2 scale, float depth);
	}
}

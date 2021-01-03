#if MONOGAME || FNA
using Microsoft.Xna.Framework;
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
		void Draw(object texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 origin, Vector2 scale, float depth);
	}
}

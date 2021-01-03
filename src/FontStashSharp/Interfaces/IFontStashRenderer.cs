#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using Vector2 = System.Drawing.PointF;
using Texture2D = System.Object;
#endif

namespace FontStashSharp.Interfaces
{
	public interface IFontStashRenderer
	{
		void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 origin, Vector2 scale, float depth);
	}
}

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

namespace FontStashSharp.Interfaces
{
	public interface IFontStashRenderer
	{
		void Draw(ITexture2D texture, Rectangle dest, Rectangle source, Color color, float depth);
	}
}

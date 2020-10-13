using System.Drawing;

namespace FontStashSharp.Interfaces
{
	public interface IFontStashRenderer
	{
		void Draw(ITexture2D texture, Rectangle dest, Rectangle source, Color color, float depth);
	}
}

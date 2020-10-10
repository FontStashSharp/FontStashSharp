using System.Drawing;

namespace FontStashSharp.Interfaces
{
	public interface IRenderer
	{
		void Draw(ITexture texture, Rectangle dest, Rectangle source, Color color, float depth);
	}
}

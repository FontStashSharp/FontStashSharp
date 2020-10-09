using System.Drawing;

namespace FontStashSharp.Interfaces
{
	public interface IRenderer
	{
		void Draw(ITexture texture, Rectangle dest, Rectangle source, FssColor color, float depth);
	}
}

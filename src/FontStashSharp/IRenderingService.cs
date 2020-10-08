using System.Drawing;

namespace FontStashSharp
{
	public interface IRenderingService
	{
		void Draw(ITexture texture, Rectangle dest, Rectangle source, FssColor color, float depth);
	}
}

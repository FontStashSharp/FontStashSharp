using System.Drawing;

namespace FontStashSharp
{
	/// <summary>
	/// 2D Texture
	/// </summary>
	public interface ITexture
	{
		void SetData(Rectangle bounds, FssColor[] data);
	}

	/// <summary>
	/// Texture Creation Service
	/// </summary>
	public interface ITextureCreationService
	{
		ITexture Create(int width, int height);
	}
}
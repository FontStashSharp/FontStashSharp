using System;
using System.Drawing;

namespace FontStashSharp.Interfaces
{
	/// <summary>
	/// 2D Texture
	/// </summary>
	public interface ITexture: IDisposable
	{
		void SetData(Rectangle bounds, FssColor[] data);
	}

	/// <summary>
	/// Texture Creation Service
	/// </summary>
	public interface ITextureCreator
	{
		ITexture Create(int width, int height);
	}
}
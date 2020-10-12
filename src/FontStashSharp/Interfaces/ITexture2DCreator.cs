using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#else
using System.Drawing;
#endif

namespace FontStashSharp.Interfaces
{
	/// <summary>
	/// 2D Texture
	/// </summary>
	public interface ITexture2D: IDisposable
	{
		/// <summary>
		/// Sets RGBA data at the specified bounds
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="data"></param>
		void SetData(Rectangle bounds, byte[] data);
	}

	/// <summary>
	/// Texture Creation Service
	/// </summary>
	public interface ITexture2DCreator
	{
		ITexture2D Create(int width, int height);
	}
}
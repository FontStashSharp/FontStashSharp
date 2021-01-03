#if PLATFORM_AGNOSTIC

using System.Drawing;

namespace FontStashSharp.Interfaces
{
	/// <summary>
	/// Texture Creation Service
	/// </summary>
	public interface ITexture2DManager
	{
		object CreateTexture(int width, int height);

		/// <summary>
		/// Sets RGBA data at the specified bounds
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="data"></param>
		void SetTextureData(object texture, Rectangle bounds, byte[] data);
	}
}

#endif
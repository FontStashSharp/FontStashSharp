using FontStashSharp.Interfaces;

namespace FontStashSharp.SharpFont
{
	public class SharpFontLoader : IFontLoader
	{
		public IFontSource Load(byte[] data, FontLoaderSettings settings)
		{
			return new SharpFontSource(data);
		}
	}
}

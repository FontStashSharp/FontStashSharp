using FontStashSharp.Interfaces;

namespace FontStashSharp.Rasterizers.FreeType
{
	public class FreeTypeLoader : IFontLoader
	{
		public IFontSource Load(byte[] data)
		{
			return new FreeTypeSource(data);
		}
	}
}

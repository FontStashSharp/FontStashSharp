using FontStashSharp.Interfaces;

namespace FontStashSharp.SharpFont
{
	public class FreeTypeLoader : IFontLoader
	{
		public IFontSource Load(byte[] data)
		{
			return new FreeTypeSource(data);
		}
	}
}

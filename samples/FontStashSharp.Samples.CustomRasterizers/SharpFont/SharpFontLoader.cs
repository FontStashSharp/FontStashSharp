using FontStashSharp.Interfaces;

namespace FontStashSharp.SharpFont
{
	public class SharpFontLoader : IFontLoader
	{
		public IFontSource Load(byte[] data, FontSystemSettings settings)
		{
			return new SharpFontSource(data);
		}
	}
}

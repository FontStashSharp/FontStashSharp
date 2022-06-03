using FontStashSharp.Interfaces;

namespace FontStashSharp.Rasterizers.SixLabors.Fonts
{
	public class SixLaborsFontLoader: IFontLoader
	{
		public IFontSource Load(byte[] data)
		{
			return new SixLaborsFontSource(data);
		}
	}
}

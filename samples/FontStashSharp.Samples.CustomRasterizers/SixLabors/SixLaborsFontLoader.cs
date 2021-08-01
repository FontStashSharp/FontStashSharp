using FontStashSharp.Interfaces;

namespace FontStashSharp.Samples.SixLabors
{
	public class SixLaborsFontLoader: IFontLoader
	{
		public IFontSource Load(byte[] data, FontLoaderSettings settings)
		{
			return new SixLaborsFontSource(data);
		}
	}
}

using FontStashSharp.Interfaces;

namespace FontStashSharp.Samples.SixLabors
{
	public class SixLaborsFontLoader: IFontLoader
	{
		public IFontSource Load(byte[] data, FontSystemSettings settings)
		{
			return new SixLaborsFontSource(data);
		}
	}
}

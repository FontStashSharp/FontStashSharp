using FontStashSharp.Interfaces;

namespace FontStashSharp
{
	public class StbTrueTypeSharpFontLoader : IFontLoader
	{
		public static readonly StbTrueTypeSharpFontLoader Instance = new StbTrueTypeSharpFontLoader();

		private StbTrueTypeSharpFontLoader()
		{
		}

		public IFont Load(byte[] data)
		{
			return Font.FromMemory(data);
		}
	}
}

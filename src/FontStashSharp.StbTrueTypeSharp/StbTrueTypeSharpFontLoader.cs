using FontStashSharp.Interfaces;

namespace FontStashSharp
{
	public class StbTrueTypeSharpFontLoader : IFontLoader
	{
		public IFont Load(byte[] data)
		{
			return Font.FromMemory(data);
		}
	}
}

#if false

using FontStashSharp.Interfaces;

namespace FontStashSharp.StbTrueTypeNative
{
  public class StbTrueTypeNativeLoader: IFontLoader
  {
		public IFontSource Load(byte[] data, FontSystemSettings settings)
		{
			return StbTrueTypeNativeSource.FromMemory(data, settings);
		}
	}
}

#endif
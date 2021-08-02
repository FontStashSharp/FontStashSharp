using FontStashSharp.Interfaces;

namespace FontStashSharp
{

	internal class StbTrueTypeSharpFontLoader : IFontLoader
	{
		private readonly StbTrueTypeSharpSettings _settings;

		public StbTrueTypeSharpFontLoader(StbTrueTypeSharpSettings settings)
		{
			_settings = settings;
		}

		public IFontSource Load(byte[] data)
		{
			return new StbTrueTypeSharpFontSource(data, _settings);
		}
	}
}

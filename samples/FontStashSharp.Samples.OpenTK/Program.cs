using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace FontStashSharp
{
	class Program
	{
		private static void Main(string[] args)
		{
			var nativeWindowSettings = new NativeWindowSettings()
			{
				Size = new Vector2i(1200, 800),
				Title = "FontStashSharp.Samples.OpenTK",
				// This is needed to run on macos
				Flags = ContextFlags.ForwardCompatible,
			};

			using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
			{
				window.Run();
			}
		}
	}
}
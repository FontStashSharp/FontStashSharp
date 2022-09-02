using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using FontStashSharp.Platform;
using System.IO;
using System.Numerics;

namespace FontStashSharp
{
	class Program
	{
		private static IWindow window;
		private static Renderer renderer;
		private static FontSystem fontSystem;
		private static float _rads = 0.0f;

		private static void Main(string[] args)
		{
			var options = WindowOptions.Default;
			options.Size = new Vector2D<int>(1200, 800);
			options.Title = "FontStashSharp.Silk.NET";
			window = Window.Create(options);

			window.Load += OnLoad;
			window.Render += OnRender;
			window.Closing += OnClose;

			window.Run();
		}

		private static void OnLoad()
		{
			IInputContext input = window.CreateInput();
			for (int i = 0; i < input.Keyboards.Count; i++)
			{
				input.Keyboards[i].KeyDown += KeyDown;
			}

			Env.Gl = GL.GetApi(window);
			renderer = new Renderer();

			var settings = new FontSystemSettings
			{
				FontResolutionFactor = 2,
				KernelWidth = 2,
				KernelHeight = 2
			};

			fontSystem = new FontSystem(settings);
			fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
			fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
			fontSystem.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));
		}

		private static unsafe void OnRender(double obj)
		{
			Env.Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

			var text = "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog";
			var scale = new Vector2(2, 2);
				
			var font = fontSystem.GetFont(32);
			
			var size = font.MeasureString(text, scale);
			var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);

			renderer.Begin();
				
			
			font.DrawText(renderer, text, new Vector2(400, 400), FSColor.LightCoral, scale, _rads, origin);
			renderer.End();

			_rads += 0.01f;
		}

		private static void OnClose()
		{
			renderer.Dispose();
		}

		private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
		{
			if (arg2 == Key.Escape)
			{
				window.Close();
			}
		}
	}
}
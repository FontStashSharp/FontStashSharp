using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using FontStashSharp.Platform;
using System.Numerics;
using TrippyGL;
using System.IO;

namespace FontStashSharp
{
	internal class Game
	{
		private IWindow window;

		private Renderer renderer;
		private FontSystem fontSystem;
		private GraphicsDevice graphicsDevice;
		private float _rads = 0.0f;

		public Game()
		{
			var options = WindowOptions.Default;
			options.Size = new Vector2D<int>(1200, 800);
			options.Title = "FontStashSharp.Silk.NET.TrippyGL";
			window = Window.Create(options);
			window.Load += OnLoad;
			window.Render += OnRender;
			window.Closing += OnClose;
			window.Resize += OnResize;
		}

		public void Run() => window.Run();

		private void OnResize(Vector2D<int> size)
		{
			graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
			renderer.OnViewportChanged();
		}

		private void OnLoad()
		{
			graphicsDevice = new GraphicsDevice(GL.GetApi(window));
			renderer = new Renderer(graphicsDevice);

			OnResize(window.Size);

			IInputContext input = window.CreateInput();
			for (int i = 0; i < input.Keyboards.Count; i++)
			{
				input.Keyboards[i].KeyDown += KeyDown;
			}

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

		private void OnRender(double obj)
		{
			graphicsDevice.ClearColor = new Vector4(0, 0, 0, 1);
			graphicsDevice.Clear(ClearBuffers.Color);

			renderer.Begin();

			var text = "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog";
			var scale = new Vector2(2, 2);

			var font = fontSystem.GetFont(32);

			var size = font.MeasureString(text, scale);
			var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);

			font.DrawText(renderer, text, new Vector2(400, 400), FSColor.LightCoral, scale, _rads, origin);

			renderer.End();

			_rads += 0.01f;
		}

		private void OnClose()
		{
			graphicsDevice.Dispose();
		}

		private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
		{
			if (arg2 == Key.Escape)
			{
				window.Close();
			}
		}
	}
}

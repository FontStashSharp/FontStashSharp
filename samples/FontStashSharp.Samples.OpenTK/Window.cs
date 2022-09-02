using FontStashSharp.Platform;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace FontStashSharp
{
	internal class Window : GameWindow
	{
		private Renderer renderer;
		private FontSystem fontSystem;
		private float _rads = 0.0f;

		public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
			: base(gameWindowSettings, nativeWindowSettings)
		{
		}

		protected override void OnLoad()
		{
			base.OnLoad();

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

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			var text = "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog";
			var scale = new Vector2(2, 2);

			var font = fontSystem.GetFont(32);

			var size = font.MeasureString(text, scale);
			var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);

			renderer.Begin();


			font.DrawText(renderer, text, new Vector2(400, 400), FSColor.LightCoral, scale, _rads, origin);
			renderer.End();

			_rads += 0.01f;

			SwapBuffers();
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);

			GL.Viewport(0, 0, Size.X, Size.Y);
		}
	}
}

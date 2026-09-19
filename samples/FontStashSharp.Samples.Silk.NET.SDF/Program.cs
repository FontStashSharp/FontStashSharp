using FontStashSharp.Platform;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.IO;
using System.Numerics;

namespace FontStashSharp
{
	class Program
	{
		private const int FontSize = 32;

		private static readonly string[] Texts = new string[]
		{
			"The quick brown fox jumps over the lazy dog",
			"点おやをづ例声念ヒレル試石べ位掲質"
		};

		private static IWindow window;
		private static Renderer renderer;
		private static SDFTextBatch sdfBatch;
		private static SpriteFontBase _fontOrdinary, _fontSupersampling, _fontSdf;
		private static int _textIndex = 0;
		private static int _effectIndex = 0;

		private const float MinScale = 0.1f;
		private const float MaxScale = 10f;
		private const float ScaleStep = 0.25f;

		private const int ScreenWidth = 1400;
		private const int ScreenHeight = 1024;

		public static Vector2 Scale { get; set; } = new Vector2(2, 2);

		private static Vector2D<int> _windowSize = new Vector2D<int>(ScreenWidth, ScreenHeight);

		private static void Main(string[] args)
		{
			var options = WindowOptions.Default;
			options.Size = new Vector2D<int>(ScreenWidth, ScreenHeight);
			options.Title = "FontStashSharp.Silk.NET.SDF";
			window = Window.Create(options);

			window.Load += OnLoad;
			window.Render += OnRender;
			window.Resize += OnResize;
			window.Closing += OnClose;

			window.Run();
		}

		private static FontSystem CreateFontSystem(FontSystemSettings settings)
		{
			var fontSystem = new FontSystem(settings);

			fontSystem.AddFont(File.ReadAllBytes(@"Assets/Fonts/DroidSans.ttf"));
			fontSystem.AddFont(File.ReadAllBytes(@"Assets/Fonts/DroidSansJapanese.ttf"));
			fontSystem.AddFont(File.ReadAllBytes(@"Assets/Fonts/Symbola-Emoji.ttf"));

			return fontSystem;
		}

		private static void OnLoad()
		{
			IInputContext input = window.CreateInput();
			for (int i = 0; i < input.Keyboards.Count; i++)
			{
				input.Keyboards[i].KeyDown += KeyDown;
			}

			for (int i = 0; i < input.Mice.Count; i++)
			{
				input.Mice[i].Scroll += OnScroll;
			}

			Env.Gl = GL.GetApi(window);
			Env.Gl.Viewport(0, 0, (uint)ScreenWidth, (uint)ScreenHeight);

			renderer = new Renderer
			{
				Viewport = new Rectangle<int>(0, 0, ScreenWidth, ScreenHeight)
			};
			sdfBatch = new SDFTextBatch
			{
				Viewport = new Rectangle<int>(0, 0, ScreenWidth, ScreenHeight)
			};

			var fontSystem = CreateFontSystem(new FontSystemSettings());
			_fontOrdinary = fontSystem.GetFont(FontSize);

			fontSystem = CreateFontSystem(new FontSystemSettings
			{
				FontResolutionFactor = 4.0f,
				KernelWidth = 4,
				KernelHeight = 4
			});
			_fontSupersampling = fontSystem.GetFont(FontSize);

			fontSystem = CreateFontSystem(new FontSystemSettings
			{
				FontRasterizationMode = FontRasterizationMode.SDF,
				FixedSDFFontSize = 64
			});
			_fontSdf = fontSystem.GetFont(FontSize);
		}

		private static void OnResize(Vector2D<int> size)
		{
			_windowSize = size;

			if (Env.Gl != null)
			{
				Env.Gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
			}

			if (renderer != null)
			{
				renderer.Viewport = new Rectangle<int>(0, 0, size.X, size.Y);
				sdfBatch.Viewport = new Rectangle<int>(0, 0, size.X, size.Y);
			}
		}

		private static void OnScroll(IMouse mouse, ScrollWheel wheel)
		{
			var delta = wheel.Y != 0 ? wheel.Y : wheel.X;
			if (delta == 0)
			{
				return;
			}

			var newScale = Math.Max(MinScale, Math.Min(MaxScale, Scale.X + Math.Sign(delta) * ScaleStep));
			Scale = new Vector2(newScale, newScale);
		}

		private static void KeyDown(IKeyboard keyboard, Key key, int scancode)
		{
			if (key == Key.Escape)
			{
				window.Close();
			}
			else if (key == Key.S)
			{
				sdfBatch.Supersampling = !sdfBatch.Supersampling;
			}
			else if (key == Key.Space)
			{
				++_textIndex;

				if (_textIndex >= Texts.Length)
				{
					_textIndex = 0;
				}
			}
			else if (key == Key.Tab)
			{
				++_effectIndex;
				if (_effectIndex > 2)
				{
					_effectIndex = 0;
				}
			}
		}

		private static unsafe void OnRender(double obj)
		{
			Env.Gl.ClearColor(0.392f, 0.584f, 0.929f, 1.0f);
			Env.Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

			var text = Texts[_textIndex];
			var sz = _fontOrdinary.MeasureString(text, Scale);

			const float top = 160;

			renderer.Begin();

			_fontOrdinary.DrawText(renderer, $"Scale: {Scale.X:0.00}", new Vector2(0, 0), FSColor.White);
			_fontOrdinary.DrawText(renderer, "Use mouse wheel to control scale", new Vector2(0, 32), FSColor.White);

			var stateText = sdfBatch.Supersampling ? "on" : "off";
			_fontOrdinary.DrawText(renderer, $"Press 'S' to switch SDF supersampling ({stateText})", new Vector2(0, 64), FSColor.White);
			_fontOrdinary.DrawText(renderer, $"Press 'Tab' to switch effect({_effectIndex})", new Vector2(0, 96), FSColor.White);
			_fontOrdinary.DrawText(renderer, $"Press 'Space' to switch text({_textIndex})", new Vector2(0, 128), FSColor.White);

			_fontOrdinary.DrawText(renderer, text, new Vector2(0, top), FSColor.White, scale: Scale, effect: (FontSystemEffect)_effectIndex, effectAmount: 1);
			_fontSupersampling.DrawText(renderer, text, new Vector2(0, (_windowSize.Y - top - sz.Y) / 2 + top), FSColor.White, scale: Scale, effect: (FontSystemEffect)_effectIndex, effectAmount: 1);

			renderer.End();

			sdfBatch.Begin();

			switch (_effectIndex)
			{
				case 0:
					sdfBatch.DrawString(_fontSdf, text, new Vector2(0, _windowSize.Y - sz.Y - 10), FSColor.White, scale: Scale);
					break;
				case 1:
					sdfBatch.DrawShadowString(_fontSdf, text, new Vector2(0, _windowSize.Y - sz.Y - 10), FSColor.White, scale: Scale);
					break;
				case 2:
					sdfBatch.DrawStrokeString(_fontSdf, text, new Vector2(0, _windowSize.Y - sz.Y - 10), FSColor.White, scale: Scale);
					break;
			}

			sdfBatch.End();
		}

		private static void OnClose()
		{
			renderer.Dispose();
			sdfBatch.Dispose();
		}
	}
}
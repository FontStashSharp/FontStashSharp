using FontStashSharp.Platform;
using Microsoft.VisualBasic;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Rectangle = System.Drawing.Rectangle;

using Color = FontStashSharp.FSColor;

namespace FontStashSharp
{
	internal class Game
	{
		private class LineInfo
		{
			public string[] FontFiles { get; }
			public float Size { get; }
			public string Header { get; }
			public string Text { get; }
			public Color Color { get; set; } = Color.White;

			public LineInfo(string[] fontFiles, float size, string header, string text)
			{
				FontFiles = fontFiles;
				Size = size;
				Header = header;
				Text = text;
			}

			public LineInfo(string fontFile, float size, string header, string text)
			{
				FontFiles = new string[] { fontFile };
				Size = size;
				Header = header;
				Text = text;
			}
		}

		public const int EffectAmount = 1;
		public const int CharacterSpacing = 0;
		public const int LineSpacing = 4;

		private static readonly LineInfo[] _lines = new LineInfo[]
		{
			new LineInfo("NotoSans-Regular.ttf", 32, "English", "English: Some text to baseline."),
			new LineInfo("NotoSansDevanagari-Regular.ttf", 32, "Hindi", "हिंदी परीक्षण — यह एक जटिल लिपि है जिसमें संयुक्ताक्षर होते हैं।"),
			new LineInfo("NotoSansArabic-Regular.ttf", 32, "Arabic RTL", "العربية لغة جميلة وغنية بالتعبيرات والمعاني HarfBuzzSharp."),
			new LineInfo("NotoSansHebrew-Regular.ttf", 25, "Hebrew RTL", "עברית שפה יפה מאוד, עשירה בביטויים ומשמעות."),
			new LineInfo("NotoSansJP-Regular.ttf", 32, "Japanese", "日本語の文章をここに書きます。漢字とひらがなとカタカナ。"),
			new LineInfo(["NotoSansDevanagari-Regular.ttf", "NotoSansArabic-Regular.ttf", "NotoSansJP-Regular.ttf", "NotoSansHebrew-Regular.ttf"], 32, "Mixed scripts", "Hello! नमस्ते! مرحباً! こんにちは! שלום!"),
			new LineInfo(["NotoSansArabic-Regular.ttf", "NotoSansHebrew-Regular.ttf"], 25, "Mixed LTR/RTL", "Start LTR ثم نص عربي inside LTR again סוף."),
			new LineInfo("seguiemj.ttf", 32, "Emojis 1", "😀 😁 😂 🤣 😜 😎 ❤️ 💙 💜 💔 💕"),
			new LineInfo("seguiemj.ttf", 32, "Emojis 2", "🏴‍☠️ 🇺🇸 🇳🇿 🇯🇵 🇫🇮 🇪🇬 🇨🇳"),
			new LineInfo("seguiemj.ttf", 32, "Emoji ZWJ 1", "🏴 + ☠️ = 🏴‍☠️   👨 + 👩 + 👧 + 👦 = 👨‍👩‍👧‍👦"),
			new LineInfo("seguiemj.ttf", 32, "Emoji ZWJ 2", "👩 + 🦰 = 👩‍🦰   👨 + 🦱 = 👨‍🦱   👩 + 🦳 = 👩‍🦳"),
			new LineInfo("NotoSans-Regular.ttf", 25, "Combining accents", "A\u0301 E\u0301 I\u0301 O\u0301 U\u0301 à è ì ò ù ñ ã õ"),
			new LineInfo("NotoSansJP-Regular.ttf", 25, "Ligatures", "office → offering → efficient → affine → coffee."),
			new LineInfo(["NotoSansHebrew-Regular.ttf", "NotoSansArabic-Regular.ttf", "NotoSansJP-Regular.ttf"], 25, "BiDi mix", "Price ₪250 ثم discount 10% valid until יום רביעי."),
			new LineInfo(["NotoSansDevanagari-Regular.ttf", "NotoSansArabic-Regular.ttf", "NotoSansJP-Regular.ttf"], 25, "Complex mix", "हिंदी text inside العربية with 日本語 characters.")
		};

		private static Game _instance;
		private IWindow window;
		private Renderer renderer;

		private FontSystemEffect _currentEffect = FontSystemEffect.None;
		private readonly Dictionary<string, FontSystem> _fontSystemCache = new Dictionary<string, FontSystem>();

		private bool _drawBackground = false;
		private bool _animatedScaling = false;
		private readonly TextGrid _grid = new TextGrid
		{
			XSpacing = 25,
			YSpacing = 8,
		};
		private Texture _white;
		private DateTime _started;

		public static Game Instance => _instance;

		public FontSystemEffect CurrentEffect => _currentEffect;

		public Game()
		{
			_instance = this;
		}

		public void Run()
		{
			var options = WindowOptions.Default;
			options.Size = new Vector2D<int>(2100, 900);
			options.Title = "FontStashSharp.Silk.NET.RichText";
			window = Window.Create(options);

			window.Load += OnLoad;
			window.Render += OnRender;
			window.Closing += OnClose;

			window.Run();
		}

		private FontSystem GetFontSystem(string[] fontNames, bool shaped)
		{
			var sortedNames = (from f in fontNames orderby f select f).ToList();
			var key = $"{string.Join(',', sortedNames)}|{shaped}";

			FontSystem fontSystem;
			if (_fontSystemCache.TryGetValue(key, out fontSystem))
			{
				return fontSystem;
			}

			var settings = new FontSystemSettings();
			if (shaped)
			{
				settings.TextShaper = new HarfBuzzTextShaper();
			}

			fontSystem = new FontSystem(settings);

			var fontBase = Path.Combine(Utility.ExecutingAssemblyDirectory, "Fonts");
			foreach (var fontName in fontNames)
			{
				var fontPath = Path.Combine(fontBase, fontName);
				var fontBytes = File.ReadAllBytes(fontPath);

				fontSystem.AddFont(fontBytes);
			}

			_fontSystemCache[key] = fontSystem;

			return fontSystem;
		}

		private FontSystem GetFontSystem(string fontName, bool shaped)
		{
			return GetFontSystem(new string[] { fontName }, shaped);
		}

		private FontSystem GetDefaultFontSystem() => GetFontSystem("NotoSans-Regular.ttf", false);

		private void OnLoad()
		{
			_started = DateTime.Now;

			IInputContext input = window.CreateInput();
			for (int i = 0; i < input.Keyboards.Count; i++)
			{
				input.Keyboards[i].KeyDown += KeyDown;
			}

			Env.Gl = GL.GetApi(window);
			renderer = new Renderer();

			// FontSystemDefaults.FontLoader = new FreeTypeLoader();
			// FontSystemDefaults.TextShaper = new HarfBuzzTextShaper();

			var defaultFontSystem = GetDefaultFontSystem();
			var defaultFont = defaultFontSystem.GetFont(32);
			_grid.SetCell(1, 0, defaultFont, "Ordinary");
			_grid.SetCell(2, 0, defaultFont, "Shaped");

			for (var i = 0; i < _lines.Length; ++i)
			{
				var line = _lines[i];

				var fontSystem = GetFontSystem(line.FontFiles, false);
				var font = fontSystem.GetFont(line.Size);

				_grid.SetCell(0, i + 1, font, line.Header);
				_grid.SetCell(1, i + 1, font, line.Text);

				var fontSystemShaped = GetFontSystem(line.FontFiles, true);
				var fontShaped = fontSystemShaped.GetFont(line.Size);
				_grid.SetCell(2, i + 1, fontShaped, line.Text);
			}

			_white = new Texture(1, 1);
			_white.SetData(new Rectangle(0, 0, 1, 1), new byte[] { 255, 255, 255, 255 });

			GC.Collect();
		}

		private void DrawRectangle(Rectangle rectangle, Color color)
		{
			renderer.Draw(_white, new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.Width, rectangle.Height), color);
		}

		private Vector2 DrawString(TextGrid.GridCell cell, Color color, Vector2 scale)
		{
			Vector2 dimensions = cell.Font.MeasureString(cell.Text,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);
			Vector2 origin = Vector2.Zero;

			var pos = new Vector2(cell.ScreenX, cell.ScreenY);
			if (_drawBackground)
			{
				var backgroundRect = new Rectangle((int)Math.Round(pos.X - origin.X * scale.X),
					(int)Math.Round(pos.Y - origin.Y * scale.Y),
					(int)Math.Round(dimensions.X * scale.X),
					(int)Math.Round(dimensions.Y * scale.Y));
				DrawRectangle(backgroundRect, Color.Green);

				var glyphs = cell.Font.GetGlyphs(cell.Text, pos, origin, scale,
					characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
					effect: _currentEffect, effectAmount: EffectAmount);
				foreach (var g in glyphs)
				{
					DrawRectangle(g.Bounds, Color.Gray);
				}
			}

			cell.Font.DrawText(renderer, cell.Text, pos, color, scale: scale, origin: origin,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);

			return dimensions;
		}



		private void OnRender(double obj)
		{
			Env.Gl.ClearColor(Color.CornflowerBlue.ToSystemDrawing());
			Env.Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

			var passed = (float)(DateTime.Now - _started).TotalSeconds;
			Vector2 scale = _animatedScaling
				? new Vector2(1 + .32f * (float)Math.Sin(passed * .5f))
				: Vector2.One;

			renderer.Viewport = new Rectangle<int>(0, 0, window.Size.X, window.Size.Y);

			renderer.Begin();

			Vector2 cursor = Vector2.Zero;

			// Render some text
			foreach (var cell in _grid.Cells)
			{
				DrawString(cell, Color.White, scale);
			}

			renderer.End();
		}

		private void OnClose()
		{
			renderer.Dispose();
		}

		private void KeyDown(IKeyboard arg1, Key arg2, int arg3)
		{
			switch (arg2)
			{
				case Key.Escape:
					window.Close();
					break;
				case Key.Space:
					_drawBackground = !_drawBackground;
					break;

				case Key.Tab:
					var i = (int)_currentEffect;

					++i;
					if (i > (int)FontSystemEffect.Stroked)
					{
						i = 0;
					}

					_currentEffect = (FontSystemEffect)i;
					break;

				case Key.Enter:
					foreach (var pair in _fontSystemCache)
					{
						pair.Value.UseKernings = !pair.Value.UseKernings;
					}
					break;

				case Key.ShiftLeft:
					_animatedScaling = !_animatedScaling;
					break;
			}
		}
	}
}

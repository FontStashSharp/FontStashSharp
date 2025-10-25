using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#elif STRIDE
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Core.Mathematics;
using Stride.Input;
using Texture2D = Stride.Graphics.Texture;
#endif

namespace FontStashSharp.Samples
{
	/// <summary>
	/// Indicates how text is aligned.
	/// </summary>
	public enum Alignment
	{
		Left,
		Center,
		Right
	}

	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

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

#if !STRIDE
		private readonly GraphicsDeviceManager _graphics;
#endif

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;
		private FontSystemEffect _currentEffect = FontSystemEffect.None;
		private readonly Dictionary<string, FontSystem> _fontSystemCache = new Dictionary<string, FontSystem>();

		private Texture2D _white;
		private bool _drawBackground = false;
		private bool _animatedScaling = false;
		private readonly TextGrid _grid = new TextGrid
		{
			XSpacing = 25,
			YSpacing = 8,
		};

		public FontSystemEffect CurrentEffect => _currentEffect;

		public Game1()
		{
			Instance = this;

#if MONOGAME || FNA
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 2100,
				PreferredBackBufferHeight = 900
			};

			Window.AllowUserResizing = true;
#endif

			IsMouseVisible = true;
		}

#if STRIDE
		public override void ConfirmRenderingSettings(bool gameCreation)
		{
			base.ConfirmRenderingSettings(gameCreation);

			GraphicsDeviceManager.PreferredBackBufferWidth = 2100;
			GraphicsDeviceManager.PreferredBackBufferHeight = 900;
			GraphicsDeviceManager.PreferredColorSpace = ColorSpace.Gamma;
		}
#endif

#if ANDROID
		private static byte[] LoadFontResource(string name)
		{
			var assembly = typeof(Game1).Assembly;

			return assembly.ReadResourceAsBytes($"FontStashSharp.Resources.Fonts.{name}");
		}
#endif

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

			var fontBase = Path.Combine(ExecutingAssemblyDirectory, "Fonts");
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

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
#if !STRIDE
		protected override void LoadContent()
#else
		protected override Task LoadContent()
#endif
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

#if MONOGAME || FNA
			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });
#elif STRIDE
			_white = Texture2D.New2D(GraphicsDevice, 1, 1, false, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource);
			_white.SetData(GraphicsContext.CommandList, new[] { Color.White } );
#endif

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

			GC.Collect();

#if STRIDE
			return base.LoadContent();
#endif
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			KeyboardUtils.Begin();

			if (KeyboardUtils.IsPressed(Keys.Space))
			{
				_drawBackground = !_drawBackground;
			}

			if (KeyboardUtils.IsPressed(Keys.Tab))
			{
				var i = (int)_currentEffect;

				++i;
				if (i > (int)FontSystemEffect.Stroked)
				{
					i = 0;
				}

				_currentEffect = (FontSystemEffect)i;
			}

			if (KeyboardUtils.IsPressed(Keys.Enter))
			{
				foreach (var pair in _fontSystemCache)
				{
					pair.Value.UseKernings = !pair.Value.UseKernings;
				}
			}

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			KeyboardUtils.End();
		}

		private Vector2 DrawString(TextGrid.GridCell cell, Alignment alignment, Color color, Vector2 scale)
		{
			Vector2 dimensions = cell.Font.MeasureString(cell.Text,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);
			Vector2 origin = AlignmentOrigin(alignment, dimensions);

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

			_spriteBatch.DrawString(cell.Font, cell.Text, pos, color, scale: scale, origin: origin,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);

			return dimensions;
		}

		private void DrawRectangle(Rectangle rectangle, Color color)
		{
			_spriteBatch.Draw(_white, rectangle, color);
		}

		private static Vector2 AlignmentOrigin(Alignment alignment, Vector2 dimensions)
		{
			switch (alignment)
			{
				case Alignment.Left:
					return Vector2.Zero;
				case Alignment.Center:
					return new Vector2(dimensions.X / 2, 0);
				case Alignment.Right:
					return new Vector2(dimensions.X, 0);
				default:
					return Vector2.Zero;
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
#if MONOGAME || FNA
			GraphicsDevice.Clear(Color.CornflowerBlue);
			TimeSpan total = gameTime.TotalGameTime;
#elif STRIDE
			// Clear screen
			GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.CornflowerBlue);
			GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);

			// Set render target
			GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
			TimeSpan total = gameTime.Total;
#endif


			Vector2 scale = _animatedScaling
				? new Vector2(1 + .32f * (float)Math.Sin(total.TotalSeconds * .5f))
				: Vector2.One;

			// TODO: Add your drawing code here
#if MONOGAME || FNA
			_spriteBatch.Begin();
#elif STRIDE
			_spriteBatch.Begin(GraphicsContext);
#endif

			Vector2 cursor = Vector2.Zero;

			// Render some text
			foreach (var cell in _grid.Cells)
			{
				DrawString(cell, Alignment.Left, Color.White, scale);
			}

			/*			// Render the atlas texture
						_font = _fontSystem.GetFont(26);
						DrawString("Texture:", ref cursor, Alignment.Left, Vector2.One);

						var texture = _fontSystem.Atlases[0].Texture;
						_spriteBatch.Draw(texture, cursor, Color.White);*/

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
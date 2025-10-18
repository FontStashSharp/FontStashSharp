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
			public string FontFile { get; }
			public float Size { get; }
			public string Text { get; }
			public Color Color { get; set; } = Color.White;
			public FontSystem FontSystem { get; }
			public FontSystem FontSystemShaped { get; }
			public SpriteFontBase Font { get; }
			public SpriteFontBase ShapedFont { get; }

			public bool UseKernings
			{
				get => FontSystem.UseKernings;

				set
				{
					FontSystem.UseKernings = value;
					FontSystemShaped.UseKernings = value;
				}
			}

			public LineInfo(string fontFile, float size, string text)
			{
				FontFile = fontFile;
				Size = size;
				Text = text;

				var fontBase = Path.Combine(ExecutingAssemblyDirectory, "Fonts");
				var fontPath = Path.Combine(fontBase, FontFile);
				var fontBytes = File.ReadAllBytes(fontPath);

				FontSystem = new FontSystem();
				FontSystem.AddFont(fontBytes);
				Font = FontSystem.GetFont(size);

				FontSystemShaped = new FontSystem(new FontSystemSettings
				{
					UseTextShaping = true,
					EnableBiDi = true
				});
				FontSystemShaped.AddFont(fontBytes);
				ShapedFont = FontSystemShaped.GetFont(size);
			}
		}

		private const int EffectAmount = 1;
		private const int CharacterSpacing = 2;
		private const int LineSpacing = 4;

		private static readonly LineInfo[] _lines = new LineInfo[]
		{
			new LineInfo("MozillaText-Bold.ttf", 16, "English: Some text to baseline."),
			new LineInfo("NotoSansDevanagari-Regular.ttf", 25, "Hindi: हिंदी परीक्षण — यह एक जटिल लिपि है जिसमें संयुक्ताक्षर होते हैं।"),
			new LineInfo("segoeui.ttf", 25, "Arabic RTL: العربية لغة جميلة وغنية بالتعبيرات والمعاني HarfBuzzSharp."),
			new LineInfo("MozillaText-Bold.ttf", 16, "Hebrew RTL: עברית שפה יפה מאוד, עשירה בביטויים ומשמעות."),
			new LineInfo("NotoSansJP-Regular.ttf", 25, "Japanese: 日本語の文章をここに書きます。漢字とひらがなとカタカナ。"),
			new LineInfo("MozillaText-Bold.ttf", 16, "Mixed scripts: Hello! नमस्ते! مرحباً! こんにちは! שלום!"),
			new LineInfo("MozillaText-Bold.ttf", 16, "Mixed LTR/RTL: Start LTR ثم نص عربي inside LTR again סוף."),
			new LineInfo("seguiemj.ttf", 25, "Emojis 1: 😀 😁 😂 🤣 😜 😎 ❤️ 💙 💜 💔 💕"),
			new LineInfo("seguiemj.ttf", 25, "Emojis 2: 🏳️‍🌈 🏴‍☠️ 🏳️‍⚧️  🇺🇸 🇳🇿 🇯🇵 🇫🇮 🇪🇬 🇨🇳"),
			new LineInfo("seguiemj.ttf", 25, "Emoji ZWJ 1: 🏴 + ☠️ = 🏴‍☠️   👨 + 👩 + 👧 + 👦 = 👨‍👩‍👧‍👦"),
			new LineInfo("seguiemj.ttf", 25, "Emoji ZWJ 2: 👩 + 🦰 = 👩‍🦰   👨 + 🦱 = 👨‍🦱   👩 + 🦳 = 👩‍🦳"),
			new LineInfo("MozillaText-Bold.ttf", 16, "Combining accents: A\u0301 E\u0301 I\u0301 O\u0301 U\u0301 à è ì ò ù ñ ã õ"),
			new LineInfo("MozillaText-Bold.ttf", 16, "Ligatures: office → offering → efficient → affine → coffee."),
			new LineInfo("MozillaText-Bold.ttf", 16, "BiDi mix: Price ₪250 ثم discount 10% valid until יום רביעי."),
			new LineInfo("MozillaText-Bold.ttf", 16, "Complex mix: हिंदी text inside العربية with 日本語 characters.")
		};

#if !STRIDE
		private readonly GraphicsDeviceManager _graphics;
#endif

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;
		private FontSystemEffect _currentEffect = FontSystemEffect.None;

		private Texture2D _white;
		private bool _drawBackground = false;
		private bool _animatedScaling = false;

		public Game1()
		{
			Instance = this;

#if MONOGAME || FNA
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
#endif

			IsMouseVisible = true;
		}

#if STRIDE
		public override void ConfirmRenderingSettings(bool gameCreation)
		{
			base.ConfirmRenderingSettings(gameCreation);

			GraphicsDeviceManager.PreferredBackBufferWidth = 1200;
			GraphicsDeviceManager.PreferredBackBufferHeight = 800;
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

			// TODO: use this.Content to load your game content here
			FontSystemDefaults.UseTextShaping = true;

#if MONOGAME || FNA
			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });
#elif STRIDE
			_white = Texture2D.New2D(GraphicsDevice, 1, 1, false, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource);
			_white.SetData(GraphicsContext.CommandList, new[] { Color.White } );
#endif

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
				foreach (var line in _lines)
				{
					line.UseKernings = !line.UseKernings;
				}
			}

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			KeyboardUtils.End();
		}

		private Vector2 DrawString(SpriteFontBase _font, string text, Vector2 pos, Alignment alignment, Color color, Vector2 scale)
		{
			Vector2 dimensions = _font.MeasureString(text,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);
			Vector2 origin = AlignmentOrigin(alignment, dimensions);

			if (_drawBackground)
			{
				var backgroundRect = new Rectangle((int)Math.Round(pos.X - origin.X * scale.X),
					(int)Math.Round(pos.Y - origin.Y * scale.Y),
					(int)Math.Round(dimensions.X * scale.X),
					(int)Math.Round(dimensions.Y * scale.Y));
				DrawRectangle(backgroundRect, Color.Green);

				var glyphs = _font.GetGlyphs(text, pos, origin, scale,
					characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
					effect: _currentEffect, effectAmount: EffectAmount);
				foreach (var g in glyphs)
				{
					DrawRectangle(g.Bounds, Color.Gray);
				}
			}

			_spriteBatch.DrawString(_font, text, pos, color, scale: scale, origin: origin,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);

			return dimensions;
		}

		private void DrawString2(LineInfo line, ref Vector2 pos, Alignment alignment, Vector2 scale)
		{
			// Ordinary font
			DrawString(line.Font, line.Text, pos, alignment, line.Color, scale);

			// Shaped font
			var shapedOffset = GraphicsDevice.Viewport.Width / 2;
			var dimensions = DrawString(line.ShapedFont, line.Text, pos + new Vector2(shapedOffset, 0), alignment, line.Color, scale);

			pos.Y += dimensions.Y + LineSpacing;
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
				? new Vector2(1 + .25f * (float)Math.Sin(total.TotalSeconds * .5f))
				: Vector2.One;

			// TODO: Add your drawing code here
#if MONOGAME || FNA
			_spriteBatch.Begin();
#elif STRIDE
			_spriteBatch.Begin(GraphicsContext);
#endif

			Vector2 cursor = Vector2.Zero;

			// Render some text
			for(var i = 0; i < _lines.Length; ++i)
			{
				DrawString2(_lines[i], ref cursor, Alignment.Left, scale);
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
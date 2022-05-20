using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using FontStashSharp.Samples.UI;
using Myra;
using FontStashSharp.Samples.SixLabors;
using FontStashSharp.Samples.FreeType;
using System;

namespace FontStashSharp.Samples
{
	public class TextRenderingGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private MainPanel _mainPanel;
		private TopWidget _topWidget;
		private Desktop _topDesktop, _bottomDesktop;

		public static TextRenderingGame Instance { get; private set; }

		public static TopWidget TopWidget => Instance._topWidget;

		public static Desktop TopDesktop => Instance._topDesktop;

		public TextRenderingGame()
		{
			Instance = this;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};
			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			MyraEnvironment.Game = this;
			// MyraEnvironment.DrawWidgetsFrames = true;

			_bottomDesktop = new Desktop
			{
				// Inform Myra that external text input is available
				// So it stops translating Keys to chars
				HasExternalTextInput = true
			};

			// Provide that text input
			Window.TextInput += (s, a) =>
			{
				_bottomDesktop.OnChar(a.Character);
			};

			_mainPanel = new MainPanel();

			_bottomDesktop.Root = _mainPanel;

			_topWidget = new TopWidget();
			_topWidget.AddFontSystem("StbTrueTypeSharp(default)", null);
			// _topWidget.AddFontSystem("StbTrueType(native)", new StbTrueTypeNativeLoader());

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				_topWidget.AddFontSystem("FreeType", new FreeTypeLoader());
			}

			_topWidget.AddFontSystem("SixLabors.Fonts", new SixLaborsFontLoader());

			_topWidget.SetFontSize(32);

			_topDesktop = new Desktop
			{
				Root = _topWidget
			};

			// Top desktop occupies upper half
			_topDesktop.BoundsFetcher = () => new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height / 2);

			// Bottom desktop - bottom half
			_bottomDesktop.BoundsFetcher = () => new Rectangle(0, GraphicsDevice.Viewport.Height / 2, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height / 2);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_bottomDesktop.Render();
			_topDesktop.Render();
		}
	}
}
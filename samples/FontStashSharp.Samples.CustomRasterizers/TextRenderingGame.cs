using Microsoft.Xna.Framework;
using System;
using FontStashSharp.Rasterizers.FreeType;
using System.Collections.Generic;
using FontStashSharp.Interfaces;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp.Rasterizers.SharpAstro;

namespace FontStashSharp.Samples
{
	public class TextRenderingGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private readonly List<TextWidget> _widgets = new List<TextWidget>();

		public TextRenderingGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1400,
				PreferredBackBufferHeight = 900
			};
			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		private static byte[] GetFileBytes(string fileName)
		{
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
			return File.ReadAllBytes(path);
		}

		private void AddFontSystem(string header, IFontLoader fontLoader, bool stbTrueTypeUseOldRasterizer = false)
		{
			var settings = new FontSystemSettings
			{
				FontLoader = fontLoader,
				StbTrueTypeUseOldRasterizer = stbTrueTypeUseOldRasterizer
			};

			var fontSystem = new FontSystem(settings);
			fontSystem.AddFont(GetFileBytes(@"Fonts/DroidSans.ttf"));
			fontSystem.AddFont(GetFileBytes(@"Fonts/DroidSansJapanese.ttf"));
			fontSystem.AddFont(GetFileBytes(@"Fonts/Symbola-Emoji.ttf"));

			var textWidget = new TextWidget(header, fontSystem);
			_widgets.Add(textWidget);
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			AddFontSystem("StbTrueTypeSharp(default)", null);
			AddFontSystem("StbTrueTypeSharp(old)", null, true);
			AddFontSystem("SharpAstro.Fonts", new SharpAstroLoader());
			AddFontSystem("FreeTypeSharp", new FreeTypeLoader());
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin();

			var bounds = new Rectangle
			{
				X = 0,
				Y = 0,
				Width = GraphicsDevice.Viewport.Width / _widgets.Count,
				Height = GraphicsDevice.Viewport.Height
			};

			foreach(var widget in _widgets)
			{
				widget.Draw(_spriteBatch, bounds, "The quick brown\nfox jumps over\nthe lazy dog");
				bounds.X += bounds.Width;
			}

			_spriteBatch.End();
		}
	}
}
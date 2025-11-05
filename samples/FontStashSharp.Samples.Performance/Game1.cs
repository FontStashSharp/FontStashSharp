using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FontStashSharp.Samples
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private const string Text = "The quick brown fox jumps over the lazy dog";
		private const int Count = 1000;

		private readonly GraphicsDeviceManager _graphics;
		private SpriteFont _oldFont;
		private StaticSpriteFont _fssStaticFont;
		private DynamicSpriteFont _fssFont;
		private DynamicSpriteFont _fssShapedFont;
		private readonly Stopwatch _watch = new Stopwatch();
		private float _oldCounter = 0;
		private float _fssStaticCounter = 0;
		private float _fssFontCounter = 0;
		private float _fssShapedCounter = 0;

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;

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

		public Game1()
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

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// MG
			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory));
			_oldFont = assetManager.LoadSpriteFont(GraphicsDevice, "Fonts/debugFont.fnt");

			// Static
			_fssStaticFont = StaticSpriteFont.FromBMFont(File.ReadAllText(Path.Combine(ExecutingAssemblyDirectory, @"Fonts/debugFont.fnt")),
				s =>
				{
					var bytes = File.ReadAllBytes(Path.Combine(ExecutingAssemblyDirectory, $"Fonts/{s}"));
					return new MemoryStream(bytes);
				},
				GraphicsDevice);
			// _fssStaticFont.UseKernings = false;

			// Simple
			var fontSystem = new FontSystem();
			fontSystem.AddFont(File.ReadAllBytes(Path.Combine(ExecutingAssemblyDirectory, @"Fonts/DroidSans.ttf")));

			_fssFont = fontSystem.GetFont(24);

			var shapedSettings = new FontSystemSettings
			{
				TextShaper = new HarfBuzzTextShaper()
			};
			var fontSystemShaped = new FontSystem(shapedSettings);
			fontSystemShaped.AddFont(File.ReadAllBytes(Path.Combine(ExecutingAssemblyDirectory, @"Fonts/DroidSans.ttf")));

			_fssShapedFont = fontSystemShaped.GetFont(24);

			GC.Collect();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			// MG Font
			_watch.Restart();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_oldFont, Text, new Vector2(0, 0), Color.White);
			}

			_spriteBatch.DrawString(_oldFont, $"Ordinary SpriteFont: {_watch.Elapsed.TotalMilliseconds} ms", new Vector2(0, 32), Color.White);
			_oldCounter += (float)_watch.Elapsed.TotalMilliseconds;

			// FSS
			_watch.Restart();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_fssStaticFont, Text, new Vector2(0, 64), Color.White);
			}

			_fssStaticCounter += (float)_watch.Elapsed.TotalMilliseconds;
			_spriteBatch.DrawString(_fssStaticFont, $"Static FontStashSharp: {_watch.Elapsed.TotalMilliseconds} ms/{_fssStaticCounter / _oldCounter}x", new Vector2(0, 96), Color.White);

			// FSS
			_watch.Restart();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_fssFont, Text, new Vector2(0, 128), Color.White);
			}

			_fssFontCounter += (float)_watch.Elapsed.TotalMilliseconds;
			_spriteBatch.DrawString(_fssFont, $"Dynamic FontStashSharp: {_watch.Elapsed.TotalMilliseconds} ms/{_fssFontCounter / _oldCounter}x", new Vector2(0, 160), Color.White);

			// FSS Shaped
			_watch.Restart();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_fssShapedFont, Text, new Vector2(0, 192), Color.White);
			}

			_fssShapedCounter += (float)_watch.Elapsed.TotalMilliseconds;
			_spriteBatch.DrawString(_fssShapedFont, $"Shaped FontStashSharp: {_watch.Elapsed.TotalMilliseconds} ms/{_fssShapedCounter / _oldCounter}x", new Vector2(0, 224), Color.White);


			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
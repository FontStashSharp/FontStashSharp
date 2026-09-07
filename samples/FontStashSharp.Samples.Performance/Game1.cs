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
		private SpriteFontBase _fssFont;
		private	SpriteFontBase _fssShapedFont;
		private SpriteFontBase _fssSDFFont;
		private readonly Counter _oldCounter = new Counter();
		private readonly Counter _fssStaticCounter = new Counter();
		private readonly Counter _fssFontCounter = new Counter();
		private readonly Counter _fssShapedCounter = new Counter();
		private readonly Counter _fssSDFCounter = new Counter();
		private readonly Counter _fssSDFSupersamplingCounter = new Counter();
		
		private SpriteBatch _spriteBatch;
		private SDFTextBatch _sdfBatch;

		public Game1()
		{
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
			// FontSystemDefaults.UseKernings = false;

			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_sdfBatch = new SDFTextBatch(GraphicsDevice);

			var folder = AppDomain.CurrentDomain.BaseDirectory;

			// MG
			var assetManager = AssetManager.CreateFileAssetManager(folder);
			_oldFont = assetManager.LoadSpriteFont(GraphicsDevice, "Fonts/debugFont.fnt");

			// Static
			_fssStaticFont = StaticSpriteFont.FromBMFont(File.ReadAllText(Path.Combine(folder, @"Fonts/debugFont.fnt")),
				s =>
				{
					var bytes = File.ReadAllBytes(Path.Combine(folder, $"Fonts/{s}"));
					return new MemoryStream(bytes);
				},
				GraphicsDevice);

			// _fssStaticFont.UseKernings = false;

			// Simple
			var fontSystem = new FontSystem();
			fontSystem.AddFont(File.ReadAllBytes(Path.Combine(folder, @"Fonts/DroidSans.ttf")));

			_fssFont = fontSystem.GetFont(24);

			var shapedSettings = new FontSystemSettings
			{
				TextShaper = new HarfBuzzTextShaper()
			};
			var fontSystemShaped = new FontSystem(shapedSettings);
			fontSystemShaped.AddFont(File.ReadAllBytes(Path.Combine(folder, @"Fonts/DroidSans.ttf")));

			_fssShapedFont = fontSystemShaped.GetFont(24);

			var sdfSettings = new FontSystemSettings
			{
				FontRasterizationMode = FontRasterizationMode.SDF,
				FixedSDFFontSize = 64
			};
			var fontSystemSDF = new FontSystem(sdfSettings);
			fontSystemSDF.AddFont(File.ReadAllBytes(Path.Combine(folder, @"Fonts/DroidSans.ttf")));

			_fssSDFFont = fontSystemSDF.GetFont(24);

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
			_oldCounter.Start();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_oldFont, Text, new Vector2(0, 0), Color.White);
			}
			_oldCounter.Stop();

			_spriteBatch.DrawString(_oldFont, $"Ordinary SpriteFont: {_oldCounter.Last} ms", new Vector2(0, 32), Color.White);

			// FSS
			_fssStaticCounter.Start();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_fssStaticFont, Text, new Vector2(0, 64), Color.White);
			}
			_fssStaticCounter.Stop();

			_spriteBatch.DrawString(_oldFont, $"Static FontStashSharp: {_fssStaticCounter.Last} ms/{_fssStaticCounter.Total / _oldCounter.Total}x", new Vector2(0, 96), Color.White);

			// FSS
			_fssFontCounter.Start();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_fssFont, Text, new Vector2(0, 128), Color.White);
			}
			_fssFontCounter.Stop();

			_spriteBatch.DrawString(_oldFont, $"Dynamic FontStashSharp: {_fssFontCounter.Last} ms/{_fssFontCounter.Total / _oldCounter.Total}x", new Vector2(0, 160), Color.White);

			// FSS Shaped
			_fssShapedCounter.Start();
			for (var i = 0; i < Count; ++i)
			{
				_spriteBatch.DrawString(_fssShapedFont, Text, new Vector2(0, 192), Color.White);
			}
			_fssShapedCounter.Stop();

			_spriteBatch.DrawString(_oldFont, $"Shaped FontStashSharp: {_fssShapedCounter.Last} ms/{_fssShapedCounter.Total / _oldCounter.Total}x", new Vector2(0, 224), Color.White);

			// FSS SDF
			_sdfBatch.Supersampling = false;
			_sdfBatch.Begin();
			
			_fssSDFCounter.Start();
			for (var i = 0; i < Count; ++i)
			{
				_sdfBatch.DrawString(_fssSDFFont, Text, new Vector2(0, 256), Color.White);
			}
			_fssSDFCounter.Stop();

			_sdfBatch.End();

			_spriteBatch.DrawString(_oldFont, $"SDF FontStashSharp: {_fssSDFCounter.Last} ms/{_fssSDFCounter.Total / _oldCounter.Total}x", new Vector2(0, 288), Color.White);

			_sdfBatch.Supersampling = true;
			_sdfBatch.Begin();

			_fssSDFCounter.Start();
			for (var i = 0; i < Count; ++i)
			{
				_sdfBatch.DrawString(_fssSDFFont, Text, new Vector2(0, 320), Color.White);
			}
			_fssSDFCounter.Stop();

			_sdfBatch.End();

			_spriteBatch.DrawString(_oldFont, $"SDF Supersampled FontStashSharp: {_fssSDFCounter.Last} ms/{_fssSDFCounter.Total / _oldCounter.Total}x", new Vector2(0, 352), Color.White);


			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
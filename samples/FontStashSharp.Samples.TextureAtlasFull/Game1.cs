using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontStashSharp.Samples
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private const int AtlasesPerRow = 3;
		private const int AtlasSize = 256;
		private const int TexturePadding = 16;

		GraphicsDeviceManager _graphics;
		SpriteBatch _spriteBatch;
		private FontSystem _fontSystem;
		private Texture2D _white;
		private readonly Random _random = new Random();

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1024,
				PreferredBackBufferHeight = 768
			};

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			var settings = new FontSystemSettings
			{
				TextureWidth = AtlasSize,
				TextureHeight = AtlasSize
			};
			_fontSystem = new FontSystem(settings);
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));

			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });

			GC.Collect();
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here
			_spriteBatch.Begin();

			var c = (char)_random.Next(32, 100);

			var font = _fontSystem.GetFont(_random.Next(20, 40));
			_spriteBatch.DrawString(font, c.ToString(), Vector2.Zero, Color.White);

			var count = 0;
			foreach (var atlas in _fontSystem.Atlases)
			{
				var texture = atlas.Texture;
				var x = (count % AtlasesPerRow) * (texture.Width + TexturePadding);
				var y = 50 + (count / AtlasesPerRow) * (texture.Height + TexturePadding);

				_spriteBatch.Draw(_white, new Rectangle(x, y, texture.Width, texture.Height), Color.Green);
				_spriteBatch.Draw(texture, new Vector2(x, y), Color.White);


				++count;
			}
			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontStashSharp
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager _graphics;
		SpriteBatch _spriteBatch;
		private FontSystem[] _fontSystems;
		private Texture2D _white;
		private const string sampleText = "This is some sample text. The first two instances of this text are with a resolution factor of 1. The second two instances of this text are with a resolution factor of 3. The green box text are being drawn at a size of 15 and a scale of 1. The red box text are being drawn at a size of 30 and a scale of half.";

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1800,
				PreferredBackBufferHeight = 450
			};

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
		}

		private FontSystem LoadFont(float resolutionFactor)
		{
			var settings = new FontSystemSettings
			{
				FontResolutionFactor = resolutionFactor
			};

			var result = new FontSystem(settings);
			result.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
			result.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
			result.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

			return result;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			var fonts = new List<FontSystem>();

			fonts.Add(LoadFont(1));
			fonts.Add(LoadFont(3));

			_fontSystems = fonts.ToArray();

			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });

			GC.Collect();
		}

		private void DrawString(SpriteFontBase font, string text, int y, Color color, Color boxColor, Vector2 scale)
		{
			var size = font.MeasureString(text, scale);
			_spriteBatch.Draw(
					_white,
					new Rectangle(
							0,
							y,
							(int)size.X,
							(int)size.Y),
					boxColor);

			font.DrawText(_spriteBatch, text, new Vector2(0, y), color, scale);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin();

			// Draw the text four times with measured bounds.

			//First, draw it in the first font (resolution 1), size of 15 and scale of 1 (green box).
			var font = _fontSystems[0].GetFont(15);
			DrawString(font, sampleText, 10, Color.White, Color.Green, Vector2.One);

			//Second, draw it again with size of 30 of scale of half (red box).
			font = _fontSystems[0].GetFont(30);
			DrawString(font, sampleText, 40, Color.White, Color.Red, Vector2.One * .5f);

			//Third, draw it in the second font (resolution 3), size of 15 and scale of 1 (green box).
			font = _fontSystems[1].GetFont(15);
			DrawString(font, sampleText, 70, Color.White, Color.Green, Vector2.One);

			//Finally, draw it again with size of 30 and scale of half (red box).
			font = _fontSystems[1].GetFont(30);
			DrawString(font, sampleText, 100, Color.White, Color.Red, Vector2.One * .5f);

			/* This example will demonstrate a couple of things.
			 * The text drawn with the first font, resolution 1, the boxes accurately wrap around the text. 
			 * However, the red and green boxes are not identical.
			 * The text is rendered the same size but the horizontal spacing becomes out of sync the longer the text gets.
			 * (Note: this is correctable by calculating x scale differences, not demonstrated) 
			 * 
			 * Green boxes and red boxes are identical in shape with their respective text.
			 * However, the text drawn with the second font, resolution 3, the boxes do not accurately wrap around the text.
			 * (Note: this is NOT correctable by calculating x scale differences) 
			 */

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
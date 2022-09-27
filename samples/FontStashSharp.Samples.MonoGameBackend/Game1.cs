using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FontStashSharp
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private const int EffectAmount = 1;

		GraphicsDeviceManager _graphics;
		SpriteBatch _spriteBatch;
		private DynamicSpriteFont _font;
		private FontSystem _currentFontSystem;
		private FontSystemEffect _currentEffect;
		private Renderer _renderer;

		private Texture2D _white;
		private bool _drawBackground = false;

		private static readonly FSColor[] _colors = new FSColor[]
		{
			FSColor.Red,
			FSColor.Blue,
			FSColor.Green,
			FSColor.Aquamarine,
			FSColor.Azure,
			FSColor.Chartreuse,
			FSColor.Lavender,
			FSColor.OldLace,
			FSColor.PaleGreen,
			FSColor.SaddleBrown,
			FSColor.IndianRed,
			FSColor.ForestGreen,
			FSColor.Khaki
		};

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
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
			_renderer = new Renderer(_spriteBatch);

			_currentFontSystem = new FontSystem();
			_currentFontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
			_currentFontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
			_currentFontSystem.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });

			GC.Collect();
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
				_currentFontSystem.UseKernings = !_currentFontSystem.UseKernings;
			}

			KeyboardUtils.End();
		}

		public Vector2 MeasureString(string text) => _font.MeasureString(text, effect: _currentEffect, effectAmount: EffectAmount).ToXNA();

		public void DrawString(string text, Vector2 position, FSColor[] glyphColors)
		{
			if (_drawBackground)
			{
				var size = MeasureString(text);
				_spriteBatch.Draw(_white, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), Color.Green);


				var glyphs = _font.GetGlyphs(text, position.ToSystemNumerics(), effect: _currentEffect, effectAmount: EffectAmount);
				foreach (var g in glyphs)
				{
					_spriteBatch.Draw(_white, g.Bounds.ToXNA(), Color.Gray);
				}
			}

			_font.DrawText(_renderer, text, position.ToSystemNumerics(), glyphColors, effect: _currentEffect, effectAmount: EffectAmount);
		}

		private void DrawString(string text, int y, Color color)
		{
			if (_drawBackground)
			{
				var size = MeasureString(text);
				_spriteBatch.Draw(_white, new Rectangle(0, y, (int)size.X, (int)size.Y), Color.Green);

				var glyphs = _font.GetGlyphs(text, new System.Numerics.Vector2(0, y), effect: _currentEffect, effectAmount: EffectAmount);
				foreach (var g in glyphs)
				{
					_spriteBatch.Draw(_white, g.Bounds.ToXNA(), Color.Gray);
				}
			}

			_font.DrawText(_renderer, text, new System.Numerics.Vector2(0, y), color.ToFontStashSharp(), effect: _currentEffect, effectAmount: EffectAmount);
		}

		private void DrawString(string text, int y)
		{
			DrawString(text, y, Color.White);
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

			// Render some text
			_font = _currentFontSystem.GetFont(18);
			DrawString("The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog adfasoqiw yraldh ald halwdha ldjahw dlawe havbx get872rq", 0);

			_font = _currentFontSystem.GetFont(30);
			DrawString("The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog", 80, Color.Bisque);

			DrawString("Colored Text", new Vector2(0, 200), _colors);

			_font = _currentFontSystem.GetFont(26);
			DrawString("Texture:", 380);

			var wrapper = (Texture2D)_currentFontSystem.Atlases[0].Texture;
			_spriteBatch.Draw(wrapper, new Vector2(0, 410), Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
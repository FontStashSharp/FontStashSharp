using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

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
		private const int EffectAmount = 1;
		private const string Text = "The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog adfasoqiw yraldh ald\nhalwdha ldjahw dlawe havbx\nget872rq";

		private readonly GraphicsDeviceManager _graphics;

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;
		private FontSystem _fontSystem;
		private FontSystemEffect _currentEffect;

		private Texture2D _white;
		private bool _animatedScaling = false;
		private float _angle;
		private Renderer _renderer;

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

			_renderer = new Renderer(_spriteBatch);

			// Simple
			_fontSystem = new FontSystem();
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });

			GC.Collect();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			KeyboardUtils.Begin();

			if (KeyboardUtils.IsPressed(Keys.Tab))
			{
				var i = (int)_currentEffect;
				++i;

				if (i > 2)
				{
					i = 0;
				}

				_currentEffect = (FontSystemEffect)i;
			}

			if (KeyboardUtils.IsPressed(Keys.Enter))
			{
				_fontSystem.UseKernings = !_fontSystem.UseKernings;
			}

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			KeyboardUtils.End();
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			TimeSpan total = gameTime.TotalGameTime;

			// TODO: Add your drawing code here
			_spriteBatch.Begin();

			Vector2 scale = _animatedScaling
				? new Vector2(1 + .25f * (float)Math.Sin(total.TotalSeconds * .5f))
				: Vector2.One;

			var position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			var font = _fontSystem.GetFont(32);

			var size = font.MeasureString(Text, scale.ToSystemNumerics());

			var rads = (float)(_angle * Math.PI / 180);
			font.DrawText(_renderer, Text, position.ToSystemNumerics(), FSColor.White,
						rads, new Vector2(size.X / 2, size.Y / 2).ToSystemNumerics(), scale.ToSystemNumerics(),
						effect: _currentEffect, effectAmount: EffectAmount);

			_spriteBatch.End();

			_angle += 0.4f;

			if (_angle >= 360.0f)
			{
				_angle -= 360.0f;
			}

			base.Draw(gameTime);
		}
	}
}
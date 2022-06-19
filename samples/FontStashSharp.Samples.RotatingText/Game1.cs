using System;
using System.Collections.Generic;
using System.IO;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteFontPlus;
#if ANDROID
using System;
using Microsoft.Xna.Framework.GamerServices;
#endif
#elif STRIDE
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Core.Mathematics;
using Stride.Input;
using Texture2D = Stride.Graphics.Texture;
using SharpDX.Direct3D11;
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
		private const int EffectAmount = 1;
		private const string Text = "The quick brown\nfox jumps over\nthe lazy dog";
		private const int CharacterSpacing = 4;
		private const int LineSpacing = 8;

#if !STRIDE
		private readonly GraphicsDeviceManager _graphics;
		private SpriteFont _font;
#endif

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;
		private FontSystem _currentFontSystem;
		private FontSystem[] _fontSystems;

		private Texture2D _white;
		private bool _animatedScaling = false;
		private float _angle;

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

		private void LoadFontSystem(FontSystem result)
		{
			result.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
		}

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
			var fontSystems = new List<FontSystem>();

			// Simple
			var fontSystem = new FontSystem();
			LoadFontSystem(fontSystem);
			fontSystems.Add(fontSystem);

			// Blurry
			var settings = new FontSystemSettings
			{
				Effect = FontSystemEffect.Blurry,
				EffectAmount = EffectAmount
			};
			var blurryFontSystem = new FontSystem(settings);
			LoadFontSystem(blurryFontSystem);
			fontSystems.Add(blurryFontSystem);

			// Stroked
			settings.Effect = FontSystemEffect.Stroked;
			var strokedFontSystem = new FontSystem(settings);
			LoadFontSystem(strokedFontSystem);
			fontSystems.Add(strokedFontSystem);

			_fontSystems = fontSystems.ToArray();
			_currentFontSystem = _fontSystems[0];

#if MONOGAME || FNA
			var fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(@"C:\\Windows\\Fonts\arial.ttf"),
					32,
					1024,
					1024,
					new[]
					{
						CharacterRange.BasicLatin,
						CharacterRange.Latin1Supplement,
						CharacterRange.LatinExtendedA,
						CharacterRange.Cyrillic
					}
				);
			_font = fontBakeResult.CreateSpriteFont(GraphicsDevice);
			
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

			if (KeyboardUtils.IsPressed(Keys.Tab))
			{
				var i = 0;

				for (; i < _fontSystems.Length; ++i)
				{
					if (_currentFontSystem == _fontSystems[i])
					{
						break;
					}
				}

				++i;
				if (i >= _fontSystems.Length)
				{
					i = 0;
				}

				_currentFontSystem = _fontSystems[i];
			}

			if (KeyboardUtils.IsPressed(Keys.Enter))
			{
				_currentFontSystem.UseKernings = !_currentFontSystem.UseKernings;
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

			// TODO: Add your drawing code here
#if MONOGAME || FNA
			_spriteBatch.Begin();
#elif STRIDE
			_spriteBatch.Begin(GraphicsContext);
#endif

			Vector2 scale = _animatedScaling
				? new Vector2(1 + .25f * (float)Math.Sin(total.TotalSeconds * .5f))
				: Vector2.One;

#if !STRIDE
			var position = new Vector2(GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 2);
#else
			var position = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width / 2, GraphicsDevice.Presenter.BackBuffer.Height / 2);
#endif

			var font = _currentFontSystem.GetFont(32);
			var size = font.MeasureString(Text, scale, characterSpacing: CharacterSpacing, lineSpacing: LineSpacing);
			var rads = (float)(_angle * Math.PI / 180);
			var normalizedOrigin = new Vector2(0.5f, 0.5f);

			_spriteBatch.Draw(_white, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y), 
				null, Color.Green, rads, normalizedOrigin, SpriteEffects.None, 0.0f);
			_spriteBatch.DrawString(font, Text, position, Color.White, scale, rads, size * normalizedOrigin, characterSpacing: CharacterSpacing, lineSpacing: LineSpacing);

#if !STRIDE
			position = new Vector2(GraphicsDevice.Viewport.Width * 3 / 4, GraphicsDevice.Viewport.Height / 2);

			size = _font.MeasureString(Text) * scale;
			_spriteBatch.Draw(_white, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y),
				null, Color.Green, rads, normalizedOrigin, SpriteEffects.None, 0.0f);
			_spriteBatch.DrawString(_font, Text, position, Color.White, rads, size * normalizedOrigin, scale, SpriteEffects.None, 0);
#endif

			_spriteBatch.End();

			_angle += 0.4f;

			while (_angle >= 360.0f)
			{
				_angle -= 360.0f;
			}

			base.Draw(gameTime);
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontStashSharp.Effects;


#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
		private const int CharacterSpacing = 2;
		private const int LineSpacing = 4;

#if !STRIDE
		private readonly GraphicsDeviceManager _graphics;
#endif

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;
		private FontSystem _fontSystem;
		private FontSystemEffect _currentEffect = FontSystemEffect.None;
		private DynamicSpriteFont _font;

		private Texture2D _white;
		private bool _drawBackground = false;
		private bool _animatedScaling = false;

		private static readonly Color[] ColoredTextColors = new Color[]
		{
			Color.Red,
			Color.Blue,
			Color.Green,
			Color.Aquamarine,
			Color.Azure,
			Color.Chartreuse,
			Color.Lavender,
			Color.OldLace,
			Color.PaleGreen,
			Color.SaddleBrown,
			Color.IndianRed,
			Color.ForestGreen,
			Color.Khaki
		};

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

			// Simple
			_fontSystem = new FontSystem();
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
			_fontSystem.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

#if MONOGAME || FNA
			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });
#elif STRIDE
			_white = Texture2D.New2D(GraphicsDevice, 1, 1, false, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource);
			_white.SetData(GraphicsContext.CommandList, new[] { Color.White } );
#endif

			_sdfEffect = new SdfEffect(GraphicsDevice);

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
				_fontSystem.UseKernings = !_fontSystem.UseKernings;
			}

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			KeyboardUtils.End();
		}

		private void DrawString(string text, ref Vector2 cursor, Alignment alignment, Color[] glyphColors, Vector2 scale)
		{
			Vector2 dimensions = _font.MeasureString(text, effect: _currentEffect, effectAmount: EffectAmount);
			Vector2 origin = AlignmentOrigin(alignment, dimensions);

			if (_drawBackground)
			{
				var backgroundRect = new Rectangle((int)Math.Round(cursor.X - origin.X * scale.X),
					(int)Math.Round(cursor.Y - origin.Y * scale.Y),
					(int)Math.Round(dimensions.X * scale.X),
					(int)Math.Round(dimensions.Y * scale.Y));
				DrawRectangle(backgroundRect, Color.Green);

				var glyphs = _font.GetGlyphs(text, cursor, origin, scale, effect: _currentEffect, effectAmount: EffectAmount);
				foreach (var r in glyphs)
				{
					DrawRectangle(r.Bounds, Color.Gray);
				}
			}

			_spriteBatch.DrawString(_font, text, cursor, glyphColors,
				scale: scale, origin: origin,
				effect: _currentEffect, effectAmount: EffectAmount);
			cursor.Y += dimensions.Y + LineSpacing;
		}

		private void DrawString(string text, ref Vector2 cursor, Alignment alignment, Color color, Vector2 scale)
		{
			Vector2 dimensions = _font.MeasureString(text, 
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);
			Vector2 origin = AlignmentOrigin(alignment, dimensions);

			if (_drawBackground)
			{
				var backgroundRect = new Rectangle((int)Math.Round(cursor.X - origin.X * scale.X),
					(int)Math.Round(cursor.Y - origin.Y * scale.Y),
					(int)Math.Round(dimensions.X * scale.X),
					(int)Math.Round(dimensions.Y * scale.Y));
				DrawRectangle(backgroundRect, Color.Green);

				var glyphs = _font.GetGlyphs(text, cursor, origin, scale, 
					characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
					effect: _currentEffect, effectAmount: EffectAmount);
				foreach (var g in glyphs)
				{
					DrawRectangle(g.Bounds, Color.Gray);
				}
			}

			_spriteBatch.DrawString(_font, text, cursor, color, scale: scale, origin: origin,
				characterSpacing: CharacterSpacing, lineSpacing: LineSpacing,
				effect: _currentEffect, effectAmount: EffectAmount);
			cursor.Y += dimensions.Y + LineSpacing;
		}

		private void DrawString(string text, ref Vector2 cursor, Alignment alignment, Vector2 scale)
		{
			DrawString(text, ref cursor, alignment, Color.White, scale);
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

		private SdfEffect _sdfEffect;

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
			_spriteBatch.Begin(effect: _sdfEffect);
#elif STRIDE
			_spriteBatch.Begin(GraphicsContext);
#endif

			Vector2 cursor = Vector2.Zero;

			// Render some text

			_font = _fontSystem.GetFont(18);
			DrawString("The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog adfasoqiw yraldh ald halwdha ldjahw dlawe havbx get872rq", ref cursor, Alignment.Left, scale);

			_font = _fontSystem.GetFont(30);
			DrawString("The quick いろは brown\nfox にほへ jumps over\nt🙌h📦e l👏a👏zy dog", ref cursor, Alignment.Left, Color.Bisque, scale);

			DrawString("Colored Text", ref cursor, Alignment.Left, ColoredTextColors, scale);

			// Render some scaled text with alignment using origin.

			Vector2 columnCursor = cursor;
			DrawString("Left-Justified", ref columnCursor, Alignment.Left, new Vector2(.75f) * scale);


#if !STRIDE
			var width = GraphicsDevice.Viewport.Width;
#else
			var width = GraphicsDevice.Presenter.BackBuffer.Width;
#endif

			columnCursor = new Vector2(width / 2f, cursor.Y);
			DrawString("Centered", ref columnCursor, Alignment.Center, new Vector2(1) * scale);

			columnCursor = new Vector2(width, cursor.Y);
			DrawString("Right-Justified", ref columnCursor, Alignment.Right, new Vector2(1.5f) * scale);

			cursor = new Vector2(0, columnCursor.Y);

			// Render the atlas texture
			_font = _fontSystem.GetFont(26);
			DrawString("Texture:", ref cursor, Alignment.Left, Vector2.One);

			_spriteBatch.End();

			_spriteBatch.Begin();
			var texture = _fontSystem.Atlases[0].Texture;
			_spriteBatch.Draw(texture, cursor, Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
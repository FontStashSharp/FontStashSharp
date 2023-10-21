using System;

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
		private const int LineSpacing = 10;

#if !STRIDE
		private readonly GraphicsDeviceManager _graphics;
#endif

		public static Game1 Instance { get; private set; }
		
		private SpriteBatch _spriteBatch;
		private StaticSpriteFont _font;

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
			var assembly = GetType().Assembly;
			_font = StaticSpriteFont.FromBMFont(assembly.ReadResourceAsString("FontStashSharp.Samples.StaticSpriteFont.Fonts.Arial.fnt"),
					fileName => assembly.OpenResourceStream("FontStashSharp.Samples.StaticSpriteFont.Fonts." + fileName),
					GraphicsDevice);

#if MONOGAME || FNA
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

			if (KeyboardUtils.IsPressed(Keys.Space))
			{
				_drawBackground = !_drawBackground;
			}

			if (KeyboardUtils.IsPressed(Keys.Enter))
			{
				_font.UseKernings = !_font.UseKernings;
			}

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			KeyboardUtils.End();
		}

		private void DrawString(string text, ref Vector2 cursor, Alignment alignment, Color[] glyphColors, Vector2 scale)
		{
			Vector2 dimensions = _font.MeasureString(text);
			Vector2 origin = AlignmentOrigin(alignment, dimensions);

			if (_drawBackground)
			{
				var backgroundRect = new Rectangle((int)Math.Round(cursor.X - origin.X * scale.X),
					(int)Math.Round(cursor.Y - origin.Y * scale.Y),
					(int)Math.Round(dimensions.X * scale.X),
					(int)Math.Round(dimensions.Y * scale.Y));
				DrawRectangle(backgroundRect, Color.Green);

				var glyphs = _font.GetGlyphs(text, cursor, origin, scale);
				foreach (var g in glyphs)
				{
					DrawRectangle(g.Bounds, Color.Gray);
				}
			}

			_spriteBatch.DrawString(_font, text, cursor, glyphColors, 0, origin, scale);
			cursor.Y += dimensions.Y + LineSpacing;
		}

		private void DrawString(string text, ref Vector2 cursor, Alignment alignment, Color color, Vector2 scale)
		{
			Vector2 dimensions = _font.MeasureString(text);
			Vector2 origin = AlignmentOrigin(alignment, dimensions);

			if (_drawBackground)
			{
				var backgroundRect = new Rectangle((int)Math.Round(cursor.X - origin.X * scale.X),
					(int)Math.Round(cursor.Y - origin.Y * scale.Y),
					(int)Math.Round(dimensions.X * scale.X),
					(int)Math.Round(dimensions.Y * scale.Y));
				DrawRectangle(backgroundRect, Color.Green);

				var glyphs = _font.GetGlyphs(text, cursor, origin, scale);
				foreach (var g in glyphs)
				{
					DrawRectangle(g.Bounds, Color.Gray);
				}
			}

			_spriteBatch.DrawString(_font, text, cursor, color, 0, origin, scale);
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
				? new Vector2(1 + .25f * (float) Math.Sin(total.TotalSeconds * .5f))
				: Vector2.One;
			
			// TODO: Add your drawing code here
#if MONOGAME || FNA
			_spriteBatch.Begin();
#elif STRIDE
			_spriteBatch.Begin(GraphicsContext);
#endif

			Vector2 cursor = Vector2.Zero;

			// Render some text

			DrawString("The quick brown\nfox jumps over\nthe lazy dog", ref cursor, Alignment.Left, scale);

			DrawString("Colored Text", ref cursor, Alignment.Left, ColoredTextColors, scale);

			// Render some scaled text with alignment using origin.
			
			Vector2 columnCursor = cursor;
			DrawString("Left-Justified", ref columnCursor, Alignment.Left, new Vector2(.75f) * scale);


#if !STRIDE
			var width = GraphicsDevice.Viewport.Width;
#else
			var width = GraphicsDevice.Presenter.BackBuffer.Width;
#endif

			columnCursor = new Vector2(width/2f, cursor.Y);
			DrawString("Centered", ref columnCursor, Alignment.Center, new Vector2(1) * scale);
			
			columnCursor = new Vector2(width, cursor.Y);
			DrawString("Right-Justified", ref columnCursor, Alignment.Right, new Vector2(1.5f) * scale);

			cursor = new Vector2(0, columnCursor.Y);

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp.RichText;

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
using StbImageSharp;
#endif

namespace FontStashSharp.Samples
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private static readonly string[] Strings = new[]
		{
			"First line./nSecond line.",
			"This is /c[red]colored /c[#00f0fa]ext, /cdcolor could be set either /c[lightGreen]by name or /c[#fa9000ff]by hex code.",
			"Text in default font./n/f[arialbd.ttf, 24]Bold and smaller font. /f[ariali.ttf, 48]Italic and larger font./n/fdBack to the default font.",
			"E=mc/v[-8]2/n/vdMass–energy equivalence.",
			"A small tree: /i[mangrove1.png]",
			"A small /c[red]tree: /v[8]/i[mangrove1.png]/vd/cd/tuand some text",
			"/ebThis /es2is the /edfirst line. This is the second line. This is the third line.",
		};

#if !STRIDE
		private readonly GraphicsDeviceManager _graphics;
#endif

		public static Game1 Instance { get; private set; }

		private SpriteBatch _spriteBatch;
		private Texture2D _white;
		private bool _animatedScaling = false;
		private RichTextLayout _richText;
		private int _stringIndex = 0;
		private readonly Dictionary<string, FontSystem> _fontCache = new Dictionary<string, FontSystem>();
		private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

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

			var fontSystem = new FontSystem();
			fontSystem.AddFont(File.ReadAllBytes(@"C:/Windows/Fonts/arial.ttf"));

			_richText = new RichTextLayout
			{
				Font = fontSystem.GetFont(32),
				Text = Strings[_stringIndex],
				VerticalSpacing = 8
			};

			RichTextDefaults.FontResolver = p =>
			{
				// Parse font name and size
				var args = p.Split(',');
				var fontName = args[0].Trim();
				var fontSize = int.Parse(args[1].Trim());

				// _fontCache is field of type Dictionary<string, FontSystem>
				// It is used to cache fonts
				FontSystem fontSystem;
				if (!_fontCache.TryGetValue(fontName, out fontSystem))
				{
					// Load and cache the font system
					fontSystem = new FontSystem();
					fontSystem.AddFont(File.ReadAllBytes(Path.Combine(@"C:\Windows\Fonts", fontName)));
					_fontCache[fontName] = fontSystem;
				}

				// Return the required font
				return fontSystem.GetFont(fontSize);
			};

			RichTextDefaults.ImageResolver = p =>
			{
				Texture2D texture;

				// _textureCache is field of type Dictionary<string, Texture2D>
				// it is used to cache textures
				if (!_textureCache.TryGetValue(p, out texture))
				{
#if MONOGAME || FNA
					using (var stream = File.OpenRead(Path.Combine(@"D:\Temp\DCSSTiles\dngn\trees\", p)))
					{
						texture = Texture2D.FromStream(GraphicsDevice, stream);
					}
#else
					ImageResult image;
					using (var stream = File.OpenRead(Path.Combine(@"D:\Temp\DCSSTiles\dngn\trees\", p)))
					{
						image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
					}

					// Premultiply Alpha
					unsafe
					{
						fixed (byte* b = image.Data)
						{
							byte* ptr = b;
							for (var i = 0; i < image.Data.Length; i += 4, ptr += 4)
							{
								var falpha = ptr[3] / 255.0f;
								ptr[0] = (byte)(ptr[0] * falpha);
								ptr[1] = (byte)(ptr[1] * falpha);
								ptr[2] = (byte)(ptr[2] * falpha);
							}
						}
					}

					// Create texture
					texture = Texture2D.New2D(GraphicsDevice, image.Width, image.Height, false, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource);
					var context = new GraphicsContext(texture.GraphicsDevice);
					texture.SetData(context.CommandList, image.Data, 0, 0, new ResourceRegion(0, 0, 0, image.Width, image.Height, 1));
#endif

					_textureCache[p] = texture;
				}

				return new TextureFragment(texture);
			};

#if MONOGAME || FNA
			_white = new Texture2D(GraphicsDevice, 1, 1);
			_white.SetData(new[] { Color.White });
#elif STRIDE
			_white = Texture2D.New2D(GraphicsDevice, 1, 1, false, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource);
			_white.SetData(GraphicsContext.CommandList, new[] { Color.White });
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

			if (KeyboardUtils.IsPressed(Keys.Enter))
			{
				//				_currentFontSystem.UseKernings = !_currentFontSystem.UseKernings;
			}

			if (KeyboardUtils.IsPressed(Keys.LeftShift))
			{
				_animatedScaling = !_animatedScaling;
			}

			if (KeyboardUtils.IsPressed(Keys.Space))
			{
				++_stringIndex;
				if (_stringIndex >= Strings.Length)
				{
					_stringIndex = 0;
				}

				_richText.Text = Strings[_stringIndex];
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
			_spriteBatch.DrawString(_richText.Font, "Press 'Space' to switch between strings.", Vector2.Zero, Color.White);

			Vector2 scale = _animatedScaling
				? new Vector2(1 + .25f * (float)Math.Sin(total.TotalSeconds * .5f))
				: Vector2.One;

#if !STRIDE
			var viewportSize = new Point(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
#else
			var viewportSize = new Point(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
#endif
			if (_stringIndex != 6)
			{
				_richText.Width = viewportSize.X;
			}
			else
			{
				_richText.Width = 300;
			}

			var position = new Vector2(0, viewportSize.Y / 2 - _richText.Size.Y / 2);
			var size = _richText.Size;
			var rect = new Rectangle((int)position.X,
				(int)position.Y,
				(int)(size.X * scale.X),
				(int)(size.Y * scale.Y));
			_spriteBatch.Draw(_white, rect, Color.Green);

			_richText.Draw(_spriteBatch, position, Color.White, scale);

			_spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
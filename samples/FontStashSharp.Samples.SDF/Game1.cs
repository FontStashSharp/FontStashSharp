using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FontStashSharp.Samples;

/// <summary>
/// This is the main type for your game.
/// </summary>
public class Game1 : Game
{
	private const int FontSize = 32;
	private readonly string[] Texts = new string[] 
	{
		"The quick brown fox jumps over the lazy dog",
		"点おやをづ例声念ヒレル試石べ位掲質"
	};

	private readonly GraphicsDeviceManager _graphics;
	private SpriteFontBase _fontOrdinary, _fontSupersampling, _fontSdf;
	private SpriteBatch _spriteBatch;
	private SDFTextBatch _sdfBatch;
	private int _textIndex = 0;
	private int _effectIndex = 0;

	private const float MinScale = 0.1f;
	private const float MaxScale = 10f;
	private const float ScaleStep = 0.25f;

	private int _lastScrollWheelValue;
	public Vector2 Scale { get; set; } = new Vector2(2, 2);

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = 1400,
			PreferredBackBufferHeight = 1024
		};

		Window.AllowUserResizing = true;

		IsMouseVisible = true;
	}

	private FontSystem CreateFontSystem(FontSystemSettings settings)
	{
		var fontSystem = new FontSystem(settings);

		fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
		fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
		fontSystem.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

		return fontSystem;
	}

	/// <summary>
	/// LoadContent will be called once per game and is the place to load
	/// all of your content.
	/// </summary>
	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);
		_sdfBatch = new SDFTextBatch(GraphicsDevice);

		var fontSystem = CreateFontSystem(new FontSystemSettings());
		_fontOrdinary = fontSystem.GetFont(FontSize);

		fontSystem = CreateFontSystem(new FontSystemSettings
		{
			FontResolutionFactor = 4.0f,
			KernelWidth = 4,
			KernelHeight = 4
		});
		_fontSupersampling = fontSystem.GetFont(FontSize);

		fontSystem = CreateFontSystem(new FontSystemSettings
		{
			FontRasterizationMode = FontRasterizationMode.SDF,
			FixedSDFFontSize = 64
		});
		_fontSdf = fontSystem.GetFont(FontSize);
	}

	protected override void Update(GameTime gameTime)
	{
		var mouse = Mouse.GetState();
		var scrollDelta = mouse.ScrollWheelValue - _lastScrollWheelValue;
		_lastScrollWheelValue = mouse.ScrollWheelValue;

		if (scrollDelta != 0)
		{
			var newScale = MathHelper.Clamp(Scale.X + System.Math.Sign(scrollDelta) * ScaleStep, MinScale, MaxScale);
			Scale = new Vector2(newScale, newScale);
		}

		KeyboardUtils.Begin();

		if (KeyboardUtils.IsPressed(Keys.S))
		{
			_sdfBatch.Supersampling = !_sdfBatch.Supersampling;
		}

		if (KeyboardUtils.IsPressed(Keys.Space))
		{
			++_textIndex;

			if (_textIndex >= Texts.Length)
			{
				_textIndex = 0;
			}
		}

		if (KeyboardUtils.IsPressed(Keys.Tab))
		{
			++_effectIndex;
			if (_effectIndex > 2)
			{
				_effectIndex = 0;
			}
		}

		KeyboardUtils.End();

		base.Update(gameTime);
	}

	/// <summary>
	/// This is called when the game should draw itself.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		var screenHeight = GraphicsDevice.Viewport.Height;

		_spriteBatch.Begin();

		_spriteBatch.DrawString(_fontOrdinary, $"Scale: {Scale.X:0.00}", new Vector2(0, 0), Color.White);
		_spriteBatch.DrawString(_fontOrdinary, $"Use mouse wheel to control scale", new Vector2(0, 32), Color.White);

		var supersamplingText = _sdfBatch.Supersampling ? "on" : "off";
		_spriteBatch.DrawString(_fontOrdinary, $"Press 'S' to switch SDF supersampling ({supersamplingText})", new Vector2(0, 64), Color.White);
		_spriteBatch.DrawString(_fontOrdinary, $"Press 'Tab' to switch effect({_effectIndex})", new Vector2(0, 96), Color.White);
		_spriteBatch.DrawString(_fontOrdinary, $"Press 'Space' to switch text({_textIndex})", new Vector2(0, 128), Color.White);

		var text = Texts[_textIndex];
		var sz = _fontOrdinary.MeasureString(text, Scale);

		var top = 160;

		_spriteBatch.DrawString(_fontOrdinary, text, new Vector2(0, top), Color.White, scale: Scale, effect: (FontSystemEffect)_effectIndex, effectAmount: 1);
		_spriteBatch.DrawString(_fontSupersampling, text, new Vector2(0, (screenHeight - top - sz.Y) / 2 + top), Color.White, scale: Scale, effect: (FontSystemEffect)_effectIndex, effectAmount: 1);
		_spriteBatch.End();

		_sdfBatch.Begin();

		switch(_effectIndex)
		{
			case 0:
				_sdfBatch.DrawString(_fontSdf, text, new Vector2(0, screenHeight - sz.Y), Color.White, scale: Scale);
				break;
			case 1:
				_sdfBatch.DrawShadowString(_fontSdf, text, new Vector2(0, screenHeight - sz.Y), Color.White, scale: Scale, shadowOffsetX: 2, shadowOffsetY: 2);
				break;
			case 2:
				_sdfBatch.DrawStrokeString(_fontSdf, text, new Vector2(0, screenHeight - sz.Y), Color.White, scale: Scale, strokeThickness: 0.6f, strokeSmoothness: 0.1f);
				break;
		}
		_sdfBatch.End();
	}
}
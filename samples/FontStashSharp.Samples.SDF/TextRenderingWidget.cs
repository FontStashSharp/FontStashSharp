using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System;
using System.IO;

namespace FontStashSharp.Samples;

internal class TextRenderingWidget : Widget
{
	private SDFTextBatch _sdfTextBatch;
	private SpriteBatch _spriteBatch;
	private FontSystem _fontSystemSDF;
	private FontSystem _fontSystemStandard;
	private bool _initialized;

	public string Text { get; set; } = "Hello, World!";

	public float FontSize { get; set; } = 32.0f;

	public float TextScale { get; set; } = 1.0f;
	public Color Color { get; set; } = Color.White;
	public TextStyle TextStyle { get; set; } = TextStyle.None;
	public SDFTextSettings SDFTextSettings { get; set; } = SDFTextSettings.Default;

	public TextRenderingWidget()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;
		Background = new SolidBrush(Color.CornflowerBlue);
	}

	private void EnsureInitialized()
	{
		if (_initialized)
		{
			return;
		}

		var device = MyraEnvironment.GraphicsDevice;
		_sdfTextBatch = new SDFTextBatch(device);

		// Create a new SpriteBatch, which can be used to draw textures.
		_spriteBatch = new SpriteBatch(device);

		// Simple
		var settings = new FontSystemSettings
		{
			FontRasterizationMode = FontRasterizationMode.SDF
		};
		_fontSystemSDF = new FontSystem(settings);

		_fontSystemSDF.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
		_fontSystemSDF.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
		_fontSystemSDF.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

		settings = new FontSystemSettings
		{
			FontResolutionFactor = 2,
			KernelWidth = 2,
			KernelHeight = 2
		};
		_fontSystemStandard = new FontSystem(settings);

		_fontSystemStandard.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));
		_fontSystemStandard.AddFont(File.ReadAllBytes(@"Fonts/DroidSansJapanese.ttf"));
		_fontSystemStandard.AddFont(File.ReadAllBytes(@"Fonts/Symbola-Emoji.ttf"));

		GC.Collect();

		_initialized = true;
	}

	public override void InternalRender(RenderContext context)
	{
		base.InternalRender(context);

		EnsureInitialized();

		context.End();

		var screenPosition = ToGlobal(new Point(0, 0));

		var device = MyraEnvironment.GraphicsDevice;

		var oldViewport = device.Viewport;
		device.Viewport = new Viewport(screenPosition.X, screenPosition.Y, ActualBounds.Width, ActualBounds.Height);

		_sdfTextBatch.Begin(SDFTextSettings);
		var font = _fontSystemSDF.GetFont(FontSize);
		_sdfTextBatch.DrawString(font, Text, new Vector2(0, 0), Color, scale: new Vector2(TextScale), textStyle: TextStyle);
		_sdfTextBatch.End();

		_spriteBatch.Begin();
		font = _fontSystemStandard.GetFont(FontSize);

		var effect = FontSystemEffect.None;
		if (SDFTextSettings.Effect == SDFFontEffect.Stroked)
		{
			effect = FontSystemEffect.Stroked;
		}

		_spriteBatch.DrawString(font, Text, new Vector2(0, ActualBounds.Height / 2), Color, scale: new Vector2(TextScale), effect: effect, effectAmount: 1, textStyle: TextStyle);
		_spriteBatch.End();

		device.Viewport = oldViewport;

		// Restart the Myra render context
		context.Begin();
	}
}

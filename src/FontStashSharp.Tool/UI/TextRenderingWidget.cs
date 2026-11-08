using FontStashSharp.RichText;
using FontStashSharp.Tool;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using System;

namespace FontStashSharp.Samples;

internal class TextRenderingWidget : Widget
{
	private SDFTextBatch _sdfTextBatch;
	private SpriteBatch _spriteBatch;
	private readonly RichTextLayout _rtf = new RichTextLayout();

	public SpriteFontBase Font { get => _rtf.Font; set => _rtf.Font = value; }

	public string Text { get => _rtf.Text; set => _rtf.Text = value; }

	public float TextScale { get; set; } = 1.0f;

	public TextRenderingWidget()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
		VerticalAlignment = VerticalAlignment.Stretch;

		var device = MyraEnvironment.GraphicsDevice;
		_sdfTextBatch = new SDFTextBatch(device);

		_spriteBatch = new SpriteBatch(device);
	}

	public override void InternalRender(RenderContext context)
	{
		base.InternalRender(context);

		if (Font == null)
		{
			return;
		}

		context.End();

		var screenPosition = ToGlobal(new Point(0, 0));

		var device = MyraEnvironment.GraphicsDevice;

		var oldViewport = device.Viewport;
		device.Viewport = new Viewport(screenPosition.X, screenPosition.Y, ActualBounds.Width, ActualBounds.Height);

		try
		{
			var sz = _rtf.Measure(null).ToVector2();

			sz.X *= TextScale;
			sz.Y *= TextScale;

			screenPosition.Y += (int)((ActualBounds.Height - sz.Y) / 2);

			if (Font.FontRasterizationMode == FontRasterizationMode.Standard)
			{
				_spriteBatch.Begin();

				try
				{
					_rtf.Draw(_spriteBatch, screenPosition.ToVector2(), Settings.Instance.TextColor, scale: new Vector2(TextScale));
				}
				finally
				{
					_spriteBatch.End();
				}
			}
			else
			{
				_sdfTextBatch.Supersampling = Settings.Instance.SDFSupersampling;

				_sdfTextBatch.Begin();

				try
				{
					_rtf.Draw(_sdfTextBatch, screenPosition.ToVector2(), Settings.Instance.TextColor, scale: new Vector2(TextScale));
				}
				finally
				{
					_sdfTextBatch.End();
				}
			}

			// Restart the Myra render context
			context.Begin();
		}
		catch (Exception ex)
		{
			// Restart the Myra render context
			context.Begin();

			// Draw error
			context.DrawLine(0, 0, ActualBounds.Width, ActualBounds.Height, Color.Red, 2);
			context.DrawLine(ActualBounds.Width, 0, 0, ActualBounds.Height, Color.Red, 2);

			var sz = DefaultAssets.DebugFont.MeasureString(ex.Message);

			context.DrawString(DefaultAssets.DebugFont, ex.Message, new Vector2(0, (ActualBounds.Height - sz.Y) / 2), Color.White);
		}
		finally
		{
			device.Viewport = oldViewport;
		}
	}
}
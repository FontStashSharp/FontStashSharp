using FontStashSharp.RichText;
using FontStashSharp.Tool;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

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
		Background = new SolidBrush(Color.CornflowerBlue);

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

		var sz = _rtf.Measure(null).ToVector2();

		sz.X *= TextScale;
		sz.Y *= TextScale;

		screenPosition.Y += (int)((ActualBounds.Height - sz.Y) / 2);

		if (Font.FontRasterizationMode == FontRasterizationMode.Standard)
		{
			_spriteBatch.Begin();
			_rtf.Draw(_spriteBatch, screenPosition.ToVector2(), Settings.Instance.TextColor, scale: new Vector2(TextScale));
			_spriteBatch.End();
		}
		else
		{
			_sdfTextBatch.Supersampling = Settings.Instance.SDFSupersampling;

			_sdfTextBatch.Begin();
			_rtf.Draw(_sdfTextBatch, screenPosition.ToVector2(), Settings.Instance.TextColor, scale: new Vector2(TextScale));

			_sdfTextBatch.End();
		}

		device.Viewport = oldViewport;

		// Restart the Myra render context
		context.Begin();
	}
}
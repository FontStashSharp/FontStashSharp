using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using System.IO;

namespace FontStashSharp.Samples
{
	public class TextWidget : Widget
	{
		private float _fontResolutionFactor = 1.0f;
		private int _kernelWidth = 0, _kernelHeight = 0;

		private readonly SpriteBatch _spriteBatch;
		private DynamicSpriteFont _font;

		public float FontResolutionFactor
		{
			get => _fontResolutionFactor;
			set
			{
				if (value == _fontResolutionFactor)
				{
					return;
				}

				_fontResolutionFactor = value;
				_font = null;
			}
		}

		public int KernelWidth
		{
			get => _kernelWidth;

			set
			{
				if (value == _kernelWidth)
				{
					return;
				}
				_kernelWidth = value;
				_font = null;
			}
		}

		public int KernelHeight
		{
			get => _kernelHeight;

			set
			{
				if (value == _kernelHeight)
				{
					return;
				}
				_kernelHeight = value;
				_font = null;
			}
		}

		private DynamicSpriteFont Font
		{
			get
			{
				if (_font != null)
				{
					return _font;
				}

				var settings = new FontSystemSettings
				{
					FontResolutionFactor = FontResolutionFactor,
					KernelWidth = KernelWidth,
					KernelHeight = KernelHeight
				};

				var fontSystem = new FontSystem(settings);
				fontSystem.AddFont(File.ReadAllBytes(@"Fonts/DroidSans.ttf"));

				_font = fontSystem.GetFont(32);

				return _font;
			}
		}

		public TextWidget(GraphicsDevice device)
		{
			_spriteBatch = new SpriteBatch(device);
			HorizontalAlignment = HorizontalAlignment.Stretch;
			VerticalAlignment = VerticalAlignment.Stretch;
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			var text = $"Font Resolution Factor: {FontResolutionFactor}\nKernel Width: {KernelWidth}\nKernel Height: {KernelHeight}";

			if (SampleEnvironment.DrawBackground)
			{
				var dimensions = Font.MeasureString(text);
				context.FillRectangle(
					new Rectangle(ActualBounds.X, ActualBounds.Y, (int)dimensions.X, (int)dimensions.Y),
					Color.Green);

				var rects = _font.GetGlyphRects(text, ActualBounds.Location.ToVector2());
				foreach (var r in rects)
				{
					context.FillRectangle(r, Color.Gray);
				}
				context.Flush();
			}

			_spriteBatch.Begin();

			var screenPosition = ToGlobal(ActualBounds.Location);
			var position = new Vector2(screenPosition.X, screenPosition.Y);
			_spriteBatch.DrawString(Font, text, position, Color.White, Scale);
			_spriteBatch.End();
		}
	}
}
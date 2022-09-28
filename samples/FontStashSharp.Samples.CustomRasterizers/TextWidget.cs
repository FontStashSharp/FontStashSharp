using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp.Samples
{
	public class TextWidget
	{
		private readonly string _header;
		private readonly FontSystem _fontSystem;

		public string Header => _header;
		public FontSystem FontSystem => _fontSystem;

		public TextWidget(string header, FontSystem fontSystem)
		{
			if (string.IsNullOrEmpty(header))
				throw new ArgumentNullException("header");
			if (fontSystem == null)
				throw new ArgumentNullException(nameof(fontSystem));

			_header = header;
			_fontSystem = fontSystem;
		}
		
		public void Draw(SpriteBatch batch, Rectangle bounds, string text)
		{
			var font = _fontSystem.GetFont(32);
			var y = bounds.Y;
			font.DrawText(batch, _header, new Vector2(bounds.X, y), Color.White);

			y += 100;
			font.DrawText(batch, text, new Vector2(bounds.X, y), Color.White);
		}
	}
}
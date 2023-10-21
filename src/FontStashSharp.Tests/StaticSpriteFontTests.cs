using NUnit.Framework;
using System.Linq;

namespace FontStashSharp.Tests
{
	[TestFixture]
	public class StaticSpriteFontTests
	{
		[Test]
		public void Load()
		{
			var assembly = TestsEnvironment.Assembly;
			var data = assembly.ReadResourceAsString("Resources.arial64.fnt");

			var font = StaticSpriteFont.FromBMFont(data, fileName => assembly.OpenResourceStream("Resources." + fileName), TestsEnvironment.GraphicsDevice);

			Assert.AreEqual(63, font.FontSize);
			Assert.AreEqual(191, font.Glyphs.Count);

			var texture = font.Glyphs.First().Value.Texture;

			Assert.NotNull(texture);
			Assert.AreEqual(512, texture.Width);
			Assert.AreEqual(512, texture.Height);
		}
	}
}

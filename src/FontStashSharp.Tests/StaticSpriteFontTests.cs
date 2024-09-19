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

			Assert.That(font.FontSize, Is.EqualTo(63));
			Assert.That(font.Glyphs.Count, Is.EqualTo(191));

			var texture = font.Glyphs.First().Value.Texture;

			Assert.That(texture, Is.Not.Null);
			Assert.That(texture.Width, Is.EqualTo(512));
			Assert.That(texture.Height, Is.EqualTo(512));
		}
	}
}

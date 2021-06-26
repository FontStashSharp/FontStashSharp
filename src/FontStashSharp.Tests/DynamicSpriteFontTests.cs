using NUnit.Framework;

namespace FontStashSharp.Tests
{
	[TestFixture]
	public class DynamicSpriteFontTests
	{
		[Test]
		public void CacheNull()
		{
			var fontSystem = new FontSystem();
			fontSystem.AddFont(TestsEnvironment.Assembly.ReadResourceAsBytes("Resources.DroidSans.ttf"));

			var font = fontSystem.GetFont(32);

			// Such symbol doesnt exist in ttf
			var codePoint = 12345678;

			// Shouldn't exist
			DynamicFontGlyph glyph;
			Assert.IsFalse(font.Glyphs.TryGetValue(codePoint, out glyph));

			glyph = (DynamicFontGlyph)font.GetGlyph(TestsEnvironment.GraphicsDevice, codePoint);

			// Now it should exist
			Assert.IsTrue(font.Glyphs.TryGetValue(codePoint, out glyph));

			// And should be equal to null too
			Assert.AreEqual(glyph, null);
		}
	}
}

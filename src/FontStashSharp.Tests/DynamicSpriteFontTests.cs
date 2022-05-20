using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

		[Test]
		public void ExistingTexture()
		{
			Texture2D existingTexture;
			using (var stream = TestsEnvironment.Assembly.OpenResourceStream("Resources.default_ui_skin.png"))
			{
				existingTexture = Texture2D.FromStream(TestsEnvironment.GraphicsDevice, stream);
			}

			var settings = new FontSystemSettings
			{
				ExistingTexture = existingTexture,
				ExistingTextureUsedSpace = new Rectangle(0, 0, existingTexture.Width, 160)
			};

			var fontSystem = new FontSystem(settings);
			fontSystem.AddFont(TestsEnvironment.Assembly.ReadResourceAsBytes("Resources.DroidSans.ttf"));

			var atlasFull = false;
			fontSystem.CurrentAtlasFull += (s, a) => atlasFull = true;

			for (var size = 64; size < 128; ++size)
			{
				var font = fontSystem.GetFont(size);
				for (var codePoint = (int)'a'; codePoint < 'z'; ++codePoint)
				{
					var glyph = (DynamicFontGlyph)font.GetGlyph(TestsEnvironment.GraphicsDevice, codePoint);

					// Make sure glyph doesnt intersects with the used space
					if (!atlasFull)
					{
						Assert.IsFalse(settings.ExistingTextureUsedSpace.Intersects(glyph.TextureRectangle));
					}
					else
					{
						// If we've moved to the next atlas
						// The new glyph should override old existing texture used space
						Assert.IsTrue(settings.ExistingTextureUsedSpace.Intersects(glyph.TextureRectangle));

						// This concludes the testing
						goto finish;
					}
				}
			}

		finish:
			;
		}
	}
}

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace FontStashSharp.Tests
{
	public class DynamicSpriteFontTests
	{
		/// <summary>
		/// Tests that requesting a non-existent code point returns null and caches it properly.
		/// </summary>
		[Fact]
		public void CacheNull()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var baseFont = fontSystem.GetFont(32);
			Assert.IsType<DynamicSpriteFont>(baseFont);
			var font = (DynamicSpriteFont)baseFont;

			// Such symbol doesnt exist in ttf
			var codePoint = 12345678;

			// Shouldn't exist
			var glyphs = font.GetGlyphs(FontSystemEffect.None, 0);
			DynamicFontGlyph glyph;
			Assert.False(glyphs.TryGetValue(codePoint, out glyph));

			glyph = (DynamicFontGlyph)font.GetGlyph(TestsEnvironment.GraphicsDevice, codePoint, FontSystemEffect.None, 0);

			// Now it should exist
			Assert.True(glyphs.TryGetValue(codePoint, out glyph));

			// And should be equal to null too
			Assert.Null(glyph);
		}

		/// <summary>
		/// Tests font rendering with an existing texture and verifies glyphs do not intersect with reserved space.
		/// </summary>
		[Fact]
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

			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);

			var atlasFull = false;
			fontSystem.CurrentAtlasFull += (s, a) => atlasFull = true;

			for (var size = 64; size < 128; ++size)
			{
				var baseFont = fontSystem.GetFont(size);
				Assert.IsType<DynamicSpriteFont>(baseFont);
				var font = (DynamicSpriteFont)baseFont;
				for (var codePoint = (int)'a'; codePoint < 'z'; ++codePoint)
				{
					var glyph = (DynamicFontGlyph)font.GetGlyph(TestsEnvironment.GraphicsDevice, codePoint, FontSystemEffect.None, 0);

					// Make sure glyph doesnt intersects with the used space
					if (!atlasFull)
					{
						Assert.False(settings.ExistingTextureUsedSpace.Intersects(glyph.TextureRectangle));
					}
					else
					{
						// If we've moved to the next atlas
						// The new glyph should override old existing texture used space
						Assert.True(settings.ExistingTextureUsedSpace.Intersects(glyph.TextureRectangle));

						// This concludes the testing
						goto finish;
					}
				}
			}

		finish:
			;
		}

		private class RendererCall
		{
			public Texture2D Texture;
			public Vector2 Pos;
			public Rectangle? Source;
			public Color Color;
			public float Rotation;
			public Vector2 Scale;
			public float Depth;
		}

		private class Renderer : IFontStashRenderer
		{
			public GraphicsDevice GraphicsDevice => TestsEnvironment.GraphicsDevice;
			public readonly System.Collections.Generic.List<RendererCall> Calls = new System.Collections.Generic.List<RendererCall>();

			public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				Calls.Add(new RendererCall
				{
					Texture = texture,
					Pos = pos,
					Source = src,
					Color = color,
					Rotation = rotation,
					Scale = scale,
					Depth = depth
				});
			}
		}

		/// <summary>
		/// Tests text rendering with various font sizes, character spacing, and kerning settings to verify glyph positions.
		/// </summary>
		[Theory]
		[InlineData("Tuesday", 45, 4, true, new int[] { 2, 9, 22, 17, 43, 18, 63, 17, 86, 10, 109, 17, 132, 18 })]
		[InlineData("Tuesday", 45, 4, false, new int[] { 2, 9, 24, 17, 45, 18, 65, 17, 88, 10, 111, 17, 134, 18 })]
		[InlineData("Tuesday", 45.5f, 4, true, new int[] { 2, 10, 22, 18, 43, 19, 63, 18, 87, 11, 110, 18, 133, 19 })]
		public void DrawText(string text, float size, int characterSpacing, bool useKernings, int[] glyphPos)
		{
			var settings = new FontSystemSettings();
			var fontSystem = new FontSystem(settings)
			{
				UseKernings = useKernings
			};
			fontSystem.AddFont(TestsEnvironment.Assembly.ReadResourceAsBytes("Resources.Komika.ttf"));

			var renderer = new Renderer();
			var font = fontSystem.GetFont(size);

			font.DrawText(renderer, text, Vector2.Zero, Color.White, characterSpacing: characterSpacing);

			Assert.Equal(glyphPos.Length, renderer.Calls.Count * 2);
			for (var i = 0; i < renderer.Calls.Count; i++)
			{
				Assert.Equal(glyphPos[i * 2], (int)renderer.Calls[i].Pos.X);
				Assert.Equal(glyphPos[i * 2 + 1], (int)renderer.Calls[i].Pos.Y);
			}
		}
	}
}

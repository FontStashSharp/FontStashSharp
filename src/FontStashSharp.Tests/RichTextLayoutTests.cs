using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Xunit;

namespace FontStashSharp.Tests
{
	public class RichTextLayoutTests
	{
		/// <summary>
		/// Tests basic rich text layout parsing with color codes, newlines, and effects to verify line count, chunks, and dimensions.
		/// </summary>
		[Theory]
		[InlineData("First line./nSecond line.", 2, 1, 149, 64)]
		[InlineData("This is /c[red]colored /c[#00f0fa]ext, /cdcolor could be set either /c[lightGreen]by name or /c[#fa9000ff]by hex code.", 1, 6, 844, 32)]
		[InlineData("/esT/eb[2]e/edxt", 1, 3, 52, 32)]
		public void BasicTests(string text, int linesCount, int chunksInFirstLineCount, int width, int height)
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = text,
				Font = fontSystem.GetFont(32)
			};

			Assert.Equal(linesCount, richTextLayout.Lines.Count);
			if (linesCount > 0)
			{
				Assert.Equal(chunksInFirstLineCount, richTextLayout.Lines[0].Chunks.Count);
			}
			Assert.Equal(new Point(width, height), richTextLayout.Size);
		}

		/// <summary>
		/// Tests rich text layout with numeric parameters for vertical offset, strike effect, blur effect, and bold.
		/// </summary>
		[Fact]
		public void NumericParametersTest()
		{
			const string text = "/v[-8]Test/v4Test/vd/es[2]Test/edTest/eb3Test";

			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = text,
				Font = fontSystem.GetFont(32),
				ShiftByTop = false
			};

			Assert.Single(richTextLayout.Lines);
			var chunks = richTextLayout.Lines[0].Chunks;
			Assert.Equal(5, chunks.Count);
			Assert.Equal(-8, chunks[0].VerticalOffset);
			Assert.Equal(4, chunks[1].VerticalOffset);

			var textChunk = (TextChunk)chunks[2];
			Assert.Equal(0, textChunk.VerticalOffset);
			Assert.Equal(FontSystemEffect.Stroked, textChunk.Effect);
			Assert.Equal(2, textChunk.EffectAmount);

			textChunk = (TextChunk)chunks[3];
			Assert.Equal(FontSystemEffect.None, textChunk.Effect);
			Assert.Equal(0, textChunk.EffectAmount);

			textChunk = (TextChunk)chunks[4];
			Assert.Equal(FontSystemEffect.Blurry, textChunk.Effect);
			Assert.Equal(3, textChunk.EffectAmount);
		}

		/// <summary>
		/// Tests that the '/ds' command accepts an optional color parameter that overrides RichTextDefaults.
		/// </summary>
		[Fact]
		public void ShadowCommandColorParamTest()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = "/ds[Red]Shadow /ddsPlain",
				Font = fontSystem.GetFont(32)
			};

			Assert.Single(richTextLayout.Lines);
			var chunks = richTextLayout.Lines[0].Chunks;
			Assert.Equal(2, chunks.Count);

			var textChunk = (TextChunk)chunks[0];
			Assert.Equal(SDFTextEffect.Shadow, textChunk.SDFEffect);
			Assert.Equal(Color.Red, textChunk.SDFEffectColor);

			textChunk = (TextChunk)chunks[1];
			Assert.Equal(SDFTextEffect.None, textChunk.SDFEffect);
		}

		/// <summary>
		/// Tests that the '/dt' command accepts an optional color parameter that overrides RichTextDefaults.
		/// </summary>
		[Fact]
		public void StrokeCommandColorParamTest()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = "/dt[#00ff00]Stroke /ddPlain",
				Font = fontSystem.GetFont(32)
			};

			Assert.Single(richTextLayout.Lines);
			var chunks = richTextLayout.Lines[0].Chunks;

			var textChunk = (TextChunk)chunks[0];
			Assert.Equal(SDFTextEffect.Stroke, textChunk.SDFEffect);
			Assert.Equal(new Color(0, 255, 0, 255), textChunk.SDFEffectColor);
		}

		/// <summary>
		/// Tests that the effect color param persists until '/dd' and that a param-less command falls back to RichTextDefaults.
		/// </summary>
		[Fact]
		public void SDFEffectColorPersistenceTest()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = "/ds[Green]First /dsSecond /ddPlain /dsThird",
				Font = fontSystem.GetFont(32)
			};

			Assert.Single(richTextLayout.Lines);
			var chunks = richTextLayout.Lines[0].Chunks;
			Assert.Equal(4, chunks.Count);

			// First chunk uses the explicitly set color
			Assert.Equal(Color.Green, ((TextChunk)chunks[0]).SDFEffectColor);

			// Second chunk keeps the color set by the previous '/ds[Green]'
			Assert.Equal(Color.Green, ((TextChunk)chunks[1]).SDFEffectColor);

			// After '/dd', the color falls back to RichTextDefaults
			Assert.Equal(RichTextDefaults.SDFShadowColor, ((TextChunk)chunks[3]).SDFEffectColor);
		}

		/// <summary>
		/// Tests text wrapping behavior when rich text layout width is constrained.
		/// </summary>
		[Fact]
		public void WrappingTest()
		{
			const string text = "This is the first line. This is the second line. This is the third line.";

			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = text,
				Font = fontSystem.GetFont(32),
				Width = 300
			};

			Assert.Equal(3, richTextLayout.Lines.Count);
		}

		/// <summary>
		/// Tests that rich text layout with UTF-32 characters (emojis) can be measured without throwing exceptions.
		/// </summary>
		[Fact]
		public void MeasureUtf32DoesNotThrow()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Font = fontSystem.GetFont(32),
				Text = "🙌h📦e l👏a👏zy"
			};

			var size = Point.Zero;
			var ex = Record.Exception(() =>
			{
				size = richTextLayout.Size;
			});
			Assert.Null(ex);

			Assert.True(size.X >= 0);
			Assert.True(size.Y >= 0);
		}

		/// <summary>
		/// Tests automatic ellipsis insertion when text exceeds height constraints using character-based ellipsis method.
		/// </summary>
		[Fact]
		public void EllipsisCharacter()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Font = fontSystem.GetFont(32),
				Text = "/ebThis /es2is the /edfirst line. This /es2is the /edsecond line. This is the third line.",
				Width = 260,
				Height = 100,
				VerticalSpacing = 8,
				AutoEllipsisMethod = AutoEllipsisMethod.Character
			};

			var lines = richTextLayout.Lines;

			Assert.Equal(2, lines.Count);
			Assert.Equal(3, lines[1].Chunks.Count);
			Assert.IsType<TextChunk>(lines[1].Chunks[2]);
			var textChunk = (TextChunk)lines[1].Chunks[2];
			Assert.Equal("second li...", textChunk.Text);
		}

		/// <summary>
		/// Tests that Unicode emoji characters are correctly counted when calculating glyphs for rich text layout.
		/// </summary>
		[Fact]
		public void UnicodeCharactersCount()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Font = fontSystem.GetFont(32),
				Text = "💁👌🎍😍",
				CalculateGlyphs = true,
			};

			var lines = richTextLayout.Lines;

			Assert.Single(lines);
			Assert.Single(lines[0].Chunks);
			Assert.IsType<TextChunk>(lines[0].Chunks[0]);
			var textChunk = (TextChunk)lines[0].Chunks[0];
			Assert.Equal(4, textChunk.Glyphs.Count);
			Assert.Equal(4, textChunk.Count);
		}
	}
}

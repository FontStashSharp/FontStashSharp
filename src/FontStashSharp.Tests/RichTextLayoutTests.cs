using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace FontStashSharp.Tests
{
	[TestFixture]
	public class RichTextLayoutTests
	{
		[TestCase("First line./nSecond line.", 2, 1, 149, 64)]
		[TestCase("This is /c[red]colored /c[#00f0fa]ext, /cdcolor could be set either /c[lightGreen]by name or /c[#fa9000ff]by hex code.", 1, 6, 844, 32)]
		[TestCase("/esT/eb[2]e/edxt", 1, 3, 52, 32)]
		public void BasicTests(string text, int linesCount, int chunksInFirstLineCount, int width, int height)
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Text = text,
				Font = fontSystem.GetFont(32)
			};

			Assert.AreEqual(linesCount, richTextLayout.Lines.Count);
			if (linesCount > 0)
			{
				Assert.AreEqual(chunksInFirstLineCount, richTextLayout.Lines[0].Chunks.Count);
			}
			Assert.AreEqual(richTextLayout.Size, new Point(width, height));
		}

		[Test]
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

			Assert.AreEqual(1, richTextLayout.Lines.Count);
			var chunks = richTextLayout.Lines[0].Chunks;
			Assert.AreEqual(5, chunks.Count);
			Assert.AreEqual(-8, chunks[0].VerticalOffset);
			Assert.AreEqual(4, chunks[1].VerticalOffset);

			var textChunk = (TextChunk)chunks[2];
			Assert.AreEqual(0, textChunk.VerticalOffset);
			Assert.AreEqual(FontSystemEffect.Stroked, textChunk.Effect);
			Assert.AreEqual(2, textChunk.EffectAmount);

			textChunk = (TextChunk)chunks[3];
			Assert.AreEqual(FontSystemEffect.None, textChunk.Effect);
			Assert.AreEqual(0, textChunk.EffectAmount);

			textChunk = (TextChunk)chunks[4];
			Assert.AreEqual(FontSystemEffect.Blurry, textChunk.Effect);
			Assert.AreEqual(3, textChunk.EffectAmount);
		}

		[Test]
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

			Assert.AreEqual(3, richTextLayout.Lines.Count);
		}

		[Test]
		public void MeasureUtf32DoesNotThrow()
		{
			var fontSystem = TestsEnvironment.DefaultFontSystem;

			var richTextLayout = new RichTextLayout
			{
				Font = fontSystem.GetFont(32),
				Text = "🙌h📦e l👏a👏zy"
			};

			var size = Point.Zero;
			Assert.DoesNotThrow(() =>
			{
				size = richTextLayout.Size;
			});

			Assert.GreaterOrEqual(size.X, 0);
			Assert.GreaterOrEqual(size.Y, 0);
		}

		[Test]
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

			Assert.AreEqual(2, lines.Count);
			Assert.AreEqual(3, lines[1].Chunks.Count);
			Assert.IsInstanceOf<TextChunk>(lines[1].Chunks[2]);
			var textChunk = (TextChunk)lines[1].Chunks[2];
			Assert.AreEqual("second li...", textChunk.Text);
		}

		[Test]
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

			Assert.AreEqual(1, lines.Count);
			Assert.AreEqual(1, lines[0].Chunks.Count);
			Assert.IsInstanceOf<TextChunk>(lines[0].Chunks[0]);
			var textChunk = (TextChunk)lines[0].Chunks[0];
			Assert.AreEqual(4, textChunk.Glyphs.Count);
			Assert.AreEqual(4, textChunk.Count);
		}
	}
}

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

			Assert.That(richTextLayout.Lines.Count, Is.EqualTo(linesCount));
			if (linesCount > 0)
			{
				Assert.That(richTextLayout.Lines[0].Chunks.Count, Is.EqualTo(chunksInFirstLineCount));
			}
			Assert.That(new Point(width, height), Is.EqualTo(richTextLayout.Size));
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

			Assert.That(richTextLayout.Lines.Count, Is.EqualTo(1));
			var chunks = richTextLayout.Lines[0].Chunks;
			Assert.That(chunks.Count, Is.EqualTo(5));
			Assert.That(chunks[0].VerticalOffset, Is.EqualTo(-8));
			Assert.That(chunks[1].VerticalOffset, Is.EqualTo(4));

			var textChunk = (TextChunk)chunks[2];
			Assert.That(textChunk.VerticalOffset, Is.EqualTo(0));
			Assert.That(textChunk.Effect, Is.EqualTo(FontSystemEffect.Stroked));
			Assert.That(textChunk.EffectAmount, Is.EqualTo(2));

			textChunk = (TextChunk)chunks[3];
			Assert.That(textChunk.Effect, Is.EqualTo(FontSystemEffect.None));
			Assert.That(textChunk.EffectAmount, Is.EqualTo(0));

			textChunk = (TextChunk)chunks[4];
			Assert.That(textChunk.Effect, Is.EqualTo(FontSystemEffect.Blurry));
			Assert.That(textChunk.EffectAmount, Is.EqualTo(3));
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

			Assert.That(richTextLayout.Lines.Count, Is.EqualTo(3));
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

			Assert.That(size.X, Is.GreaterThanOrEqualTo(0));
			Assert.That(size.Y, Is.GreaterThanOrEqualTo(0));
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

			Assert.That(lines.Count, Is.EqualTo(2));
			Assert.That(lines[1].Chunks.Count, Is.EqualTo(3));
			Assert.That(lines[1].Chunks[2], Is.InstanceOf<TextChunk>());
			var textChunk = (TextChunk)lines[1].Chunks[2];
			Assert.That(textChunk.Text, Is.EqualTo("second li..."));
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

			Assert.That(lines.Count, Is.EqualTo(1));
			Assert.That(lines[0].Chunks.Count, Is.EqualTo(1));
			Assert.That(lines[0].Chunks[0], Is.InstanceOf<TextChunk>());
			var textChunk = (TextChunk)lines[0].Chunks[0];
			Assert.That(textChunk.Glyphs.Count, Is.EqualTo(4));
			Assert.That(textChunk.Count, Is.EqualTo(4));
		}
	}
}

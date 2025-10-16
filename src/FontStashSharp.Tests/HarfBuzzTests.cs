using FontStashSharp.HarfBuzz;
using NUnit.Framework;

namespace FontStashSharp.Tests
{
	[TestFixture]
	public class HarfBuzzTests
	{
		[TestCase("Hello World", 1, TextDirection.LTR)]
		[TestCase("مرحبا", 1, TextDirection.RTL)]
		[TestCase("", 0, TextDirection.LTR)]
		[TestCase("123", 1, TextDirection.LTR)]
		public void BiDiAnalyzer_SingleDirectionText(string text, int expectedRunCount, TextDirection expectedDirection)
		{
			var runs = BiDiAnalyzer.SegmentIntoDirectionalRuns(text);

			Assert.That(runs.Count, Is.EqualTo(expectedRunCount));

			if (expectedRunCount > 0)
			{
				Assert.That(runs[0].Direction, Is.EqualTo(expectedDirection));
				Assert.That(runs[0].Start, Is.EqualTo(0));
				Assert.That(runs[0].Length, Is.EqualTo(text.Length));
			}
		}

		[Test]
		public void BiDiAnalyzer_MixedLtrRtlText()
		{
			// English "Hello" + Arabic "مرحبا"
			var text = "Hello مرحبا";
			var runs = BiDiAnalyzer.SegmentIntoDirectionalRuns(text);

			// Should have 2 runs: LTR for "Hello " and RTL for "مرحبا"
			Assert.That(runs.Count, Is.EqualTo(2));

			Assert.That(runs[0].Direction, Is.EqualTo(TextDirection.LTR));
			Assert.That(runs[0].Start, Is.EqualTo(0));
			Assert.That(runs[1].Direction, Is.EqualTo(TextDirection.RTL));
		}

		[Test]
		public void BiDiAnalyzer_LeadingNeutralCharacters()
		{
			// Spaces before text should be assigned to the following run's direction
			var text = "  Hello";
			var runs = BiDiAnalyzer.SegmentIntoDirectionalRuns(text);

			Assert.That(runs.Count, Is.EqualTo(1));
			Assert.That(runs[0].Direction, Is.EqualTo(TextDirection.LTR));
			Assert.That(runs[0].Start, Is.EqualTo(0));
			Assert.That(runs[0].Length, Is.EqualTo(text.Length));
		}

		[Test]
		public void BiDiAnalyzer_OnlyNeutralCharacters()
		{
			// Text with only neutral characters should default to LTR
			var text = "   ...   ";
			var runs = BiDiAnalyzer.SegmentIntoDirectionalRuns(text);

			Assert.That(runs.Count, Is.EqualTo(1));
			Assert.That(runs[0].Direction, Is.EqualTo(TextDirection.LTR));
		}

		[Test]
		public void TextShaper_EmptyString()
		{
			var settings = new FontSystemSettings { UseTextShaping = true };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);

			var shaped = TextShaper.Shape(fontSystem, "", 32);

			Assert.That(shaped, Is.Not.Null);
			Assert.That(shaped.Glyphs, Is.Not.Null);
			Assert.That(shaped.Glyphs.Length, Is.EqualTo(0));
			Assert.That(shaped.OriginalText, Is.EqualTo(""));
		}

		[Test]
		public void TextShaper_NullString()
		{
			var settings = new FontSystemSettings { UseTextShaping = true };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);

			var shaped = TextShaper.Shape(fontSystem, null, 32);

			Assert.That(shaped, Is.Not.Null);
			Assert.That(shaped.Glyphs, Is.Not.Null);
			Assert.That(shaped.Glyphs.Length, Is.EqualTo(0));
			Assert.That(shaped.OriginalText, Is.EqualTo(""));
		}

		[Test]
		public void TextShaper_SimpleText()
		{
			// Create font system with BiDi disabled but text shaping enabled
			var settings = new FontSystemSettings { UseTextShaping = true, EnableBiDi = false };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);

			var shaped = TextShaper.Shape(fontSystem, "Hello", 32);

			Assert.That(shaped, Is.Not.Null);
			Assert.That(shaped.Glyphs, Is.Not.Null);
			Assert.That(shaped.Glyphs.Length, Is.GreaterThan(0));
			Assert.That(shaped.OriginalText, Is.EqualTo("Hello"));
			Assert.That(shaped.FontSize, Is.EqualTo(32));

			// Each glyph should have valid advance values
			foreach (var glyph in shaped.Glyphs)
			{
				Assert.That(glyph.XAdvance, Is.GreaterThan(0));
			}
		}

		[Test]
		public void TextShaper_WithBiDiEnabled()
		{
			// Create font system with BiDi enabled
			var settings = new FontSystemSettings { UseTextShaping = true, EnableBiDi = true };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);

			var shaped = TextShaper.Shape(fontSystem, "Test", 32);

			Assert.That(shaped, Is.Not.Null);
			Assert.That(shaped.Glyphs, Is.Not.Null);
			Assert.That(shaped.Glyphs.Length, Is.GreaterThan(0));
			Assert.That(shaped.OriginalText, Is.EqualTo("Test"));
		}

		[Test]
		public void TextShaper_SurrogatePair_FormsSingleCluster()
		{
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(new FontSystemSettings { UseTextShaping = true });
			var text = "😀"; // U+1F600 (surrogate pair)
			var shaped = TextShaper.Shape(fontSystem, text, 32);

			Assert.That(shaped.Glyphs.Length, Is.LessThanOrEqualTo(1), "Emoji surrogate pair should form a single cluster");
		}

		[Test]
		public void TextShaper_EmojiZWJSequence()
		{
			var settings = new FontSystemSettings { UseTextShaping = true };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);

			// Family: 👨‍👩‍👧‍👦 (multiple codepoints joined by ZWJ)
			var text = "👨‍👩‍👧‍👦";
			var shaped = TextShaper.Shape(fontSystem, text, 32);

			Assert.That(shaped.Glyphs.Length, Is.LessThan(text.Length),
					"ZWJ sequences should combine into fewer glyphs");
		}

		[Test]
		public void ShapedText_PreservesOriginalText()
		{
			var settings = new FontSystemSettings { UseTextShaping = true };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);
			var originalText = "Testing 123";

			var shaped = TextShaper.Shape(fontSystem, originalText, 32);

			Assert.That(shaped.OriginalText, Is.EqualTo(originalText));
		}

		[Test]
		public void ShapedGlyphs_HaveValidClusterIndices()
		{
			var settings = new FontSystemSettings { UseTextShaping = true };
			var fontSystem = TestsEnvironment.CreateDefaultFontSystem(settings);
			var text = "Hello";

			var shaped = TextShaper.Shape(fontSystem, text, 32);

			// All cluster indices should be within the text length
			foreach (var glyph in shaped.Glyphs)
			{
				Assert.That(glyph.Cluster, Is.GreaterThanOrEqualTo(0));
				Assert.That(glyph.Cluster, Is.LessThan(text.Length));
			}
		}
	}
}
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
		public void BasicTests(string text, int linesCount, int chunksInFirstLineCount, int width, int height)
		{
			var fontSystem = new FontSystem();
			fontSystem.AddFont(TestsEnvironment.Assembly.ReadResourceAsBytes("Resources.DroidSans.ttf"));

			var richTextLayout = new RichTextLayout
			{
				Text = text,
				Font = fontSystem.GetFont(32)
			};

			Assert.AreEqual(richTextLayout.Lines.Count, linesCount);
			if (linesCount > 0)
			{
				Assert.AreEqual(richTextLayout.Lines[0].Chunks.Count, chunksInFirstLineCount);
			}
			Assert.AreEqual(richTextLayout.Size, new Point(width, height));
		}
	}
}

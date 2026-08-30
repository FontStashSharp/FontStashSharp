using System.Text;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// A ref struct for iterating through text with associated colors.
	/// </summary>
	internal ref struct TextColorSource
	{
		/// <summary>
		/// The underlying text source.
		/// </summary>
		public TextSource TextSource;

		/// <summary>
		/// A single color to apply to all text, if set.
		/// </summary>
		public Color? SingleColor;

		/// <summary>
		/// An array of colors, one per codepoint, if set.
		/// </summary>
		public Color[] Colors;

		/// <summary>
		/// The current position in the color array.
		/// </summary>
		public int ColorPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextColorSource"/> struct from a string with a single color.
		/// </summary>
		/// <param name="text">The text to iterate through.</param>
		/// <param name="color">The color to apply to all characters.</param>
		public TextColorSource(string text, Color color)
		{
			TextSource = new TextSource(text);
			SingleColor = color;
			Colors = null;
			ColorPosition = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextColorSource"/> struct from a string with per-codepoint colors.
		/// </summary>
		/// <param name="text">The text to iterate through.</param>
		/// <param name="colors">An array of colors, one per codepoint.</param>
		public TextColorSource(string text, Color[] colors)
		{
			TextSource = new TextSource(text);
			SingleColor = null;
			Colors = colors;
			ColorPosition = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextColorSource"/> struct from a StringSegment with a single color.
		/// </summary>
		/// <param name="text">The text segment to iterate through.</param>
		/// <param name="color">The color to apply to all characters.</param>
		public TextColorSource(StringSegment text, Color color)
		{
			TextSource = new TextSource(text);
			SingleColor = color;
			Colors = null;
			ColorPosition = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextColorSource"/> struct from a StringSegment with per-codepoint colors.
		/// </summary>
		/// <param name="text">The text segment to iterate through.</param>
		/// <param name="colors">An array of colors, one per codepoint.</param>
		public TextColorSource(StringSegment text, Color[] colors)
		{
			TextSource = new TextSource(text);
			SingleColor = null;
			Colors = colors;
			ColorPosition = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextColorSource"/> struct from a StringBuilder with a single color.
		/// </summary>
		/// <param name="text">The StringBuilder to iterate through.</param>
		/// <param name="color">The color to apply to all characters.</param>
		public TextColorSource(StringBuilder text, Color color)
		{
			TextSource = new TextSource(text);
			SingleColor = color;
			Colors = null;
			ColorPosition = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextColorSource"/> struct from a StringBuilder with per-codepoint colors.
		/// </summary>
		/// <param name="text">The StringBuilder to iterate through.</param>
		/// <param name="colors">An array of colors, one per codepoint.</param>
		public TextColorSource(StringBuilder text, Color[] colors)
		{
			TextSource = new TextSource(text);
			SingleColor = null;
			Colors = colors;
			ColorPosition = 0;
		}

		/// <summary>
		/// Gets a value indicating whether the text source is null or empty.
		/// </summary>
		public bool IsNull => TextSource.IsNull;

		/// <summary>
		/// Gets the next codepoint and its associated color. Obsolete.
		/// </summary>
		/// <param name="codepoint">The codepoint value (output).</param>
		/// <param name="color">The color for the codepoint (output).</param>
		/// <returns>True if a codepoint was read, false if the end of text was reached.</returns>
		[System.Obsolete("Possible phase out.")]
		public bool GetNextCodepoint(out int codepoint, out Color color)
		{
			color = Color.Transparent;
			if (!TextSource.GetNextCodepoint(out codepoint))
			{
				return false;
			}

			if (SingleColor != null)
			{
				color = SingleColor.Value;
			}
			else
			{
				color = Colors[ColorPosition];
				++ColorPosition;
			}

			return true;
		}

		/// <summary>
		/// Gets the next codepoint from the text source.
		/// </summary>
		/// <param name="codepoint">The codepoint value (output).</param>
		/// <returns>True if a codepoint was read, false if the end of text was reached.</returns>
		public bool GetNextCodepoint(out int codepoint)
		{
			return TextSource.GetNextCodepoint(out codepoint);
		}

		/// <summary>
		/// Gets the color for the next codepoint.
		/// </summary>
		/// <returns>The color for the current position, with wraparound for color arrays.</returns>
		public Color GetNextColor()
		{
			var color = Color.Transparent;
			
			if (SingleColor != null)
			{
				color = SingleColor.Value;
			}
			else
			{
				color = Colors[ColorPosition % Colors.Length];
				++ColorPosition;
			}
			
			return color;
		}
	}
}

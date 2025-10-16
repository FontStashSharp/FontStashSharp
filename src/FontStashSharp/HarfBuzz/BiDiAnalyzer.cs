using System.Collections.Generic;

namespace FontStashSharp.HarfBuzz
{
	/// <summary>
	/// Direction of text flow
	/// </summary>
	public enum TextDirection
	{
		/// <summary>
		/// Left-to-right text (Latin, Cyrillic, etc.)
		/// </summary>
		LTR,

		/// <summary>
		/// Right-to-left text (Arabic, Hebrew, etc.)
		/// </summary>
		RTL
	}

	/// <summary>
	/// Represents a segment of text with uniform directionality
	/// </summary>
	public struct DirectionalRun
	{
		/// <summary>
		/// Start position in the original text
		/// </summary>
		public int Start;

		/// <summary>
		/// Length of the run
		/// </summary>
		public int Length;

		/// <summary>
		/// Direction of this run
		/// </summary>
		public TextDirection Direction;
	}

	/// <summary>
	/// A simple bidirectional text analyzer for mixed LTR/RTL text
	/// </summary>
	public static class BiDiAnalyzer
	{
		/// <summary>
		/// Segments text into runs of consistent directionality
		/// </summary>
		public static List<DirectionalRun> SegmentIntoDirectionalRuns(string text)
		{
			var runs = new List<DirectionalRun>();
			int runStart = 0;
			TextDirection? currentDirection = null;

			for (int i = 0; i < text.Length; i++)
			{
				var charDirection = GetStrongDirection(text[i]);

				// Skip neutral characters as they belong to the current run
				if (charDirection == null)
					continue;

				// If direction changes, start a new run
				if (currentDirection.HasValue && charDirection != currentDirection)
				{
					// Close the current run
					runs.Add(new DirectionalRun
					{
						Start = runStart,
						Length = i - runStart,
						Direction = currentDirection.Value
					});

					runStart = i;
					currentDirection = charDirection;
				}
				else if (!currentDirection.HasValue)
				{
					// First strong character - leading neutral characters become part of this run
					currentDirection = charDirection;
				}
			}

			// Add the final run
			if (currentDirection.HasValue)
			{
				runs.Add(new DirectionalRun
				{
					Start = runStart,
					Length = text.Length - runStart,
					Direction = currentDirection.Value
				});
			}
			else if (text.Length > runStart)
			{
				// Entire text is neutral - default to LTR
				runs.Add(new DirectionalRun
				{
					Start = runStart,
					Length = text.Length - runStart,
					Direction = TextDirection.LTR
				});
			}

			return runs;
		}

		/// <summary>
		/// Checks if a character is right-to-left
		/// </summary>
		/// <remarks>https://www.ssec.wisc.edu/~tomw/java/unicode.html</remarks>
		private static bool IsRTL(char c)
		{
			// Hebrew block (U+0590 to U+05FF)
			if (c >= 0x0590 && c <= 0x05FF) return true;

			// Arabic block (U+0600 to U+06FF)
			if (c >= 0x0600 && c <= 0x06FF) return true;

			// Syriac (U+0700 to U+074F)
			if (c >= 0x0700 && c <= 0x074F) return true;

			// Thaana (U+0780 to U+07BF)
			if (c >= 0x0780 && c <= 0x07BF) return true;

			// NKo (U+07C0 to U+07FF)
			if (c >= 0x07C0 && c <= 0x07FF) return true;

			// Samaritan (U+0800 to U+083F)
			if (c >= 0x0800 && c <= 0x083F) return true;

			// Mandaic (U+0840 to U+085F)
			if (c >= 0x0840 && c <= 0x085F) return true;

			// Arabic Extended-A (U+08A0 to U+08FF)
			if (c >= 0x08A0 && c <= 0x08FF) return true;

			// Hebrew presentation forms (U+FB1D to U+FB4F)
			if (c >= 0xFB1D && c <= 0xFB4F) return true;

			// Arabic presentation forms-A (U+FB50 to U+FDFF)
			if (c >= 0xFB50 && c <= 0xFDFF) return true;

			// Arabic presentation forms-B (U+FE70 to U+FEFC)
			if (c >= 0xFE70 && c <= 0xFEFC) return true;

			return false;
		}

		/// <summary>
		/// Checks if a character is neutral (takes direction from context)
		/// </summary>
		private static bool IsNeutral(char c)
		{
			return char.IsWhiteSpace(c) ||
						 char.IsPunctuation(c) ||
						 char.IsSymbol(c) ||
						 char.IsSeparator(c);
		}

		/// <summary>
		/// Gets the strong directionality of a character
		/// </summary>
		private static TextDirection? GetStrongDirection(char c)
		{
			if (IsRTL(c))
				return TextDirection.RTL;

			// If not RTL and not neutral, assume LTR
			if (!IsNeutral(c))
				return TextDirection.LTR;

			return null; // Neutral character
		}
	}
}
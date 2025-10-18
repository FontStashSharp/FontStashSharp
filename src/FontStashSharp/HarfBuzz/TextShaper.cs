using System;
using System.Collections.Generic;

namespace FontStashSharp.HarfBuzz
{
	/// <summary>
	/// Handles text shaping using HarfBuzz
	/// </summary>
	public static class TextShaper
	{
		/// <summary>
		/// Shape text using HarfBuzz
		/// </summary>
		/// <param name="fontSystem">The font system containing font sources</param>
		/// <param name="text">The text to shape</param>
		/// <param name="fontSize">The font size</param>
		/// <returns>Shaped text with glyph information</returns>
		public static ShapedText Shape(FontSystem fontSystem, string text, float fontSize)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new ShapedText
				{
					Glyphs = new ShapedGlyph[0],
					OriginalText = text ?? string.Empty,
					FontSize = fontSize
				};
			}

			var allShapedGlyphs = new List<ShapedGlyph>(text.Length);

			// Step 1: Analyze text for bidirectional runs (if enabled)
			List<DirectionalRun> directionalRuns;
			if (fontSystem.EnableBiDi)
			{
				directionalRuns = BiDiAnalyzer.SegmentIntoDirectionalRuns(text);
			}
			else
			{
				// BiDi disabled - treat entire text as single LTR run
				directionalRuns = new List<DirectionalRun>
				{
					new DirectionalRun
					{
						Start = 0,
						Length = text.Length,
						Direction = TextDirection.LTR
					}
				};
			}

			// Step 2: Process each directional run
			foreach (var dirRun in directionalRuns)
			{
				// Step 3: Within each directional run, segment by font source
				var fontRuns = SegmentTextIntoFontRuns(fontSystem, text, dirRun.Start, dirRun.Length);

				// Step 4: Shape each font run with its appropriate font
				foreach (var fontRun in fontRuns)
				{
					var hbFont = fontSystem.GetHarfBuzzFont(fontRun.FontSourceIndex);

					if (hbFont == null)
					{
						throw new InvalidOperationException($"HarfBuzz font not available for font source {fontRun.FontSourceIndex}. Ensure font data is cached.");
					}

					hbFont.SetScale(fontSize);

					using (var buffer = new HarfBuzzSharp.Buffer())
					{
						// Add text run to buffer
						buffer.AddUtf16(text, fontRun.Start, fontRun.Length);
						buffer.GuessSegmentProperties();
						hbFont.Shape(buffer);

						// Get the shaped output
						var glyphInfos = buffer.GlyphInfos;
						var glyphPositions = buffer.GlyphPositions;

						// Convert to our ShapedGlyph format
						for (int i = 0; i < glyphInfos.Length; i++)
						{
							var info = glyphInfos[i];
							var pos = glyphPositions[i];

							allShapedGlyphs.Add(new ShapedGlyph
							{
								GlyphId = (int)info.Codepoint,
								Cluster = (int)info.Cluster + fontRun.Start,
								FontSourceIndex = fontRun.FontSourceIndex,
								XAdvance = pos.XAdvance,
								YAdvance = pos.YAdvance,
								XOffset = pos.XOffset,
								YOffset = pos.YOffset
							});
						}
					}
				}
			}

			return new ShapedText
			{
				Glyphs = allShapedGlyphs.ToArray(),
				OriginalText = text,
				FontSize = fontSize
			};
		}

		private struct FontRun
		{
			public int Start;
			public int Length;
			public int FontSourceIndex;
		}

		private static List<FontRun> SegmentTextIntoFontRuns(FontSystem fontSystem, string text, int start, int length)
		{
			var runs = new List<FontRun>();
			int currentRunStart = start;
			int currentFontSourceIndex = -1;
			int end = start + length;

			for (int i = start; i < end;)
			{
				// Get the codepoint at position i
				int codepoint;
				int charCount;
				if (i < text.Length - 1 && char.IsSurrogatePair(text, i))
				{
					codepoint = char.ConvertToUtf32(text, i);
					charCount = 2;
				}
				else
				{
					codepoint = text[i];
					charCount = 1;
				}

				// Find which font source has this codepoint
				var glyphId = fontSystem.GetCodepointIndex(codepoint, out int fontSourceIndex);

				// If this is a new font source, start a new run
				if (fontSourceIndex != currentFontSourceIndex)
				{
					// Save the previous run if it exists
					if (currentFontSourceIndex >= 0)
					{
						runs.Add(new FontRun
						{
							Start = currentRunStart,
							Length = i - currentRunStart,
							FontSourceIndex = currentFontSourceIndex
						});
					}

					// Start new run
					currentRunStart = i;
					currentFontSourceIndex = fontSourceIndex;
				}

				i += charCount;
			}

			// Add the final run
			if (currentFontSourceIndex >= 0)
			{
				runs.Add(new FontRun
				{
					Start = currentRunStart,
					Length = end - currentRunStart,
					FontSourceIndex = currentFontSourceIndex
				});
			}

			return runs;
		}
	}
}
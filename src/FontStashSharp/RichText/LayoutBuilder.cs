using System;
using System.Collections.Generic;
using System.Text;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
#endif

namespace FontStashSharp.RichText
{
	internal class LayoutBuilder
	{
		public const int NewLineWidth = 0;
		public const string Commands = "cfivsn";

		private string _text;
		private SpriteFontBase _font;
		private bool _measureRun;

		private readonly List<TextLine> _lines = new List<TextLine>();
		private TextLine _currentLine;
		private int lineTop, lineBottom;
		private int? width;
		private int _lineCount;
		private int _currentLineWidth;
		private int _currentLineChunks;

		private readonly StringBuilder _stringBuilder = new StringBuilder();
		private Color? _currentColor;
		private SpriteFontBase _currentFont;
		private int _currentVerticalOffset;

		public List<TextLine> Lines => _lines;

		public int VerticalSpacing { get; set; }
		public bool SupportsCommands { get; set; } = true;
		public bool CalculateGlyphs { get; set; }
		public bool ShiftByTop { get; set; } = true;
		public char CommandPrefix { get; set; } = '/';

		private bool ProcessCommand(ref int i, ref ChunkInfo r, out bool chunkFilled)
		{
			chunkFilled = false;

			if (!SupportsCommands ||
				i >= _text.Length - 2 ||
				_text[i] != CommandPrefix || 
				_text[i + 1] == 'n' || 
				_text[i + 1] == CommandPrefix ||
				Commands.IndexOf(_text[i + 1]) == -1)
			{
				// Not a command(or newline command that is processed differently)
				return false;
			}

			++i;

			var command = _text[i].ToString();
			if (_text[i + 1] == 'd')
			{
				switch (command)
				{
					case "c":
						_currentColor = null;
						break;
					case "f":
						// Switch to default font
						_currentFont = _font;
						break;
					case "v":
						_currentVerticalOffset = 0;
						break;
					default:
						throw new Exception($"Can't use 'd' parameter for command {command}");
				}

				i += 2;
			}
			else
			{
				// Find end
				var startPos = i + 2;
				var j = _text.IndexOf(']', startPos);

				if (j == -1)
				{
					throw new Exception($"Command '{command}' doesnt have ']'.");
				}

				var parameters = _text.Substring(startPos, j - startPos);
				switch (command)
				{
					case "c":
						_currentColor = ColorStorage.FromName(parameters);
						break;

					case "f":
						if (RichTextDefaults.FontResolver == null)
						{
							throw new Exception($"FontResolver isnt set");
						}

						_currentFont = RichTextDefaults.FontResolver(parameters);
						break;
					case "s":
						var size = int.Parse(parameters);
						r.Type = ChunkInfoType.Space;
						r.X = size;
						r.Y = 0;
						r.LineEnd = false;
						chunkFilled = true;
						break;
					case "v":
						_currentVerticalOffset = int.Parse(parameters);
						break;
					case "i":
						if (RichTextDefaults.ImageResolver == null)
						{
							throw new Exception($"ImageResolver isnt set");
						}

						var renderable = RichTextDefaults.ImageResolver(parameters);
						r.Type = ChunkInfoType.Image;
						r.Renderable = renderable;

						r.LineEnd = false;
						chunkFilled = true;

						break;
				}

				i = j + 1;
			}

			return true;
		}

		private ChunkInfo GetNextChunk(ref int i, int? remainingWidth)
		{
			var r = new ChunkInfo
			{
				LineEnd = true
			};

			// Process commands at the beginning of the chunk
			bool chunkFilled;
			while (ProcessCommand(ref i, ref r, out chunkFilled))
			{
				if (chunkFilled)
				{
					// Content chunk(image or space) is filled, return it
					return r;
				}
			}

			_stringBuilder.Clear();

			r.StartIndex = r.EndIndex = i;

			Point? lastBreakMeasure = null;
			var lastBreakIndex = i;

			for (; i < _text.Length; ++i, ++r.EndIndex)
			{
				var c = _text[i];

				if (char.IsHighSurrogate(c))
				{
					_stringBuilder.Append(c);
					continue;
				}

				if (SupportsCommands && 
					c == CommandPrefix && 
					i < _text.Length - 1 && 
					Commands.IndexOf(_text[i + 1]) != -1)
				{
					if (_text[i + 1] == 'n')
					{
						var sz2 = new Point(r.X + NewLineWidth, Math.Max(r.Y, _currentFont.LineHeight));

						// Break right here
						r.X = sz2.X;
						r.Y = sz2.Y;
						i += 2;
						break;
					}

					if (i < _text.Length - 1 && _text[i + 1] == CommandPrefix)
					{
						// Two '\' means one
						++i;
					}
					else if (i < _text.Length - 2)
					{
						// Return right here, so the command
						// would be processed in the next chunk
						r.LineEnd = false;
						break;
					}
				}

				_stringBuilder.Append(c);

				Point sz;
				if (c != '\n')
				{
					var v = _currentFont.MeasureString(_stringBuilder);
					sz = new Point((int)v.X, _font.LineHeight);
				}
				else
				{
					sz = new Point(r.X + NewLineWidth, Math.Max(r.Y, _font.LineHeight));

					// Break right here
					++r.EndIndex;
					++i;
					r.X = sz.X;
					r.Y = sz.Y;
					break;
				}

				if (remainingWidth != null && sz.X > remainingWidth.Value && i > r.StartIndex)
				{
					if (lastBreakMeasure != null)
					{
						r.X = lastBreakMeasure.Value.X;
						r.Y = lastBreakMeasure.Value.Y;
						r.EndIndex = i = lastBreakIndex;
					}

					break;
				}

				if (char.IsWhiteSpace(c) || c == '.')
				{
					lastBreakMeasure = sz;
					lastBreakIndex = i + 1;
				}

				r.X = sz.X;
				r.Y = sz.Y;
			}

			return r;
		}

		private void ResetCurrents()
		{
			_currentColor = null;
			_currentFont = _font;
			_currentVerticalOffset = 0;
		}

		private void StartLine(int startIndex, int? rowWidth)
		{
			if (!_measureRun)
			{
				_currentLine = new TextLine
				{
					TextStartIndex = startIndex
				};
			}

			lineTop = 0;
			lineBottom = 0;
			_currentLineWidth = 0;
			_currentLineChunks = 0;
			width = rowWidth;
		}

		private void EndLine(ref Point size)
		{
			var lineHeight = lineBottom - lineTop;
			++_lineCount;

			if (_currentLineWidth > size.X)
			{
				size.X = _currentLineWidth;
			}
			size.Y += lineHeight;

			if (!_measureRun)
			{
				if (ShiftByTop)
				{
					// Shift all chunks top by lineTop
					foreach (var lineChunk in _currentLine.Chunks)
					{
						lineChunk.VerticalOffset -= lineTop;
					}
				}

				_currentLine.Size.Y = lineHeight;

				// New line
				_lines.Add(_currentLine);
			}
		}

		public Point Layout(string text, SpriteFontBase font, int? rowWidth, bool measureRun = false)
		{
			if (!measureRun)
			{
				_lines.Clear();
			}

			_lineCount = 0;
			var size = Utility.PointZero;

			if (string.IsNullOrEmpty(text))
			{
				return size;
			}

			_text = text;
			_font = font;
			_measureRun = measureRun;

			ResetCurrents();

			var i = 0;

			StartLine(0, rowWidth);
			while (i < _text.Length)
			{
				var c = GetNextChunk(ref i, width);

				if (width != null && c.Width > width.Value && _currentLineChunks > 0)
				{
					// New chunk doesn't fit in the line
					// Hence move it to the second
					EndLine(ref size);
					StartLine(i, rowWidth);
				}

				width -= c.Width;
				if (_currentVerticalOffset < lineTop)
				{
					lineTop = _currentVerticalOffset;
				}

				if (_currentVerticalOffset + c.Height > lineBottom)
				{
					lineBottom = _currentVerticalOffset + c.Height;
				}

				_currentLineWidth += c.Width;

				if (!_measureRun)
				{
					Point? startPos = null;
					if (CalculateGlyphs)
					{
						startPos = new Point(_currentLine.Size.X, size.Y);
					}
					_currentLine.Size.X += c.Width;

					BaseChunk chunk = null;
					switch (c.Type)
					{
						case ChunkInfoType.Text:
							var t = _text.Substring(c.StartIndex, c.EndIndex - c.StartIndex).Replace("//", "/");
							chunk = new TextChunk(_currentFont, t, new Point(c.X, c.Y), startPos);
							break;
						case ChunkInfoType.Space:
							chunk = new SpaceChunk(c.X);
							break;
						case ChunkInfoType.Image:
							chunk = new ImageChunk(c.Renderable);
							break;
					}

					chunk.Color = _currentColor;
					chunk.VerticalOffset = _currentVerticalOffset;

					var asText = chunk as TextChunk;
					if (asText != null)
					{
						_currentLine.Count += asText.Count;
					}

					_currentLine.Chunks.Add(chunk);
				}

				++_currentLineChunks;

				if (c.LineEnd)
				{
					EndLine(ref size);
					StartLine(i, rowWidth);
				}
			}

			// Add last line if it isnt empty
			if (_currentLineChunks > 0)
			{
				EndLine(ref size);
			}

			// If text ends with '\n', then add additional line
			if (_text[_text.Length - 1] == '\n')
			{
				var lineSize = _currentFont.MeasureString(" ");
				if (!_measureRun)
				{
					var additionalLine = new TextLine
					{
						TextStartIndex = _text.Length
					};

					additionalLine.Size.Y = (int)lineSize.Y;

					_lines.Add(additionalLine);
				}

				size.Y += (int)lineSize.Y;
			}

			// Index lines and chunks
			if (!_measureRun)
			{
				for (i = 0; i < _lines.Count; ++i)
				{
					_currentLine = _lines[i];
					_currentLine.LineIndex = i;

					for (var j = 0; j < _currentLine.Chunks.Count; ++j)
					{
						var chunk = _currentLine.Chunks[j];
						chunk.LineIndex = _currentLine.LineIndex;
						chunk.ChunkIndex = j;
					}
				}
			}

			size.Y += (_lineCount - 1) * VerticalSpacing;

			return size;
		}
	}
}

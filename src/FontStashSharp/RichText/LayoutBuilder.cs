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
		public const string Commands = "cfivs";

		private string _text;
		private SpriteFontBase _font;
		private bool _supportsCommands;

		private readonly List<TextLine> _lines = new List<TextLine>();
		private readonly StringBuilder _stringBuilder = new StringBuilder();
		private Color? _currentColor;
		private SpriteFontBase _currentFont;
		private int _currentVerticalOffset;

		private bool ProcessCommand(ref int i, ref ChunkInfo r, out bool chunkFilled)
		{
			chunkFilled = false;

			if (_text[i] != '/' || i >= _text.Length - 2 || _text[i + 1] == 'n' || _text[i + 1] == '/')
			{
				// Not a command(or newline command that is processed differently)
				return false;
			}

			++i;

			var command = _text[i].ToString();
			if (!Commands.Contains(command))
			{
				throw new Exception($"Unknown command '{command}'.");
			}

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

				if (c == '/')
				{
					if (i < _text.Length - 1 && _text[i + 1] == 'n')
					{
						var sz2 = new Point(r.X + NewLineWidth, Math.Max(r.Y, _currentFont.LineHeight));

						// Break right here
						r.X = sz2.X;
						r.Y = sz2.Y;
						i += 2;
						break;
					}

					if (i < _text.Length - 1 && _text[i + 1] == '/')
					{
						// Two '\' means one
						++i;
					} else if (_supportsCommands && i < _text.Length - 2)
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
					++i;
					r.X = sz.X;
					r.Y = sz.Y;
					break;
				}

				if (remainingWidth != null && sz.X > remainingWidth.Value)
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

		public List<TextLine> Layout(string text, SpriteFontBase font, int verticalSpacing, int? rowWidth, 
			bool supportsCommands, bool calculateGlyphs, out Point size)
		{
			_lines.Clear();
			size = Point.Zero;

			if (string.IsNullOrEmpty(text))
			{
				return _lines;
			}

			_text = text;
			_font = font;
			_supportsCommands = supportsCommands;

			ResetCurrents();

			var width = rowWidth;
			var i = 0;
			var line = new TextLine
			{
				TextStartIndex = i
			};
			var lineTop = 0;
			var lineBottom = 0;

			while (i < _text.Length)
			{
				var c = GetNextChunk(ref i, width);

				width -= c.Width;
				line.Size.X += c.Width;
				if (_currentVerticalOffset < lineTop)
				{
					lineTop = _currentVerticalOffset;
				}

				if (_currentVerticalOffset + c.Height > lineBottom)
				{
					lineBottom = _currentVerticalOffset + c.Height;
				}

				BaseChunk chunk = null;
				switch (c.Type)
				{
					case ChunkInfoType.Text:
						var t = _text.Substring(c.StartIndex, c.EndIndex - c.StartIndex).Replace("//", "/");
						chunk = new TextChunk(_currentFont, t, new Point(c.X, c.Y), calculateGlyphs);
						break;
					case ChunkInfoType.Space:
						chunk = new SpaceChunk(c.X);
						break;
					case ChunkInfoType.Image:
						chunk = new ImageChunk(c.Renderable);
						break;
				}

				chunk.Color = _currentColor;
				chunk.Top = _currentVerticalOffset;

				var asText = chunk as TextChunk;
				if (asText != null)
				{
					line.Count += asText.Count;
				}

				line.Chunks.Add(chunk);

				if (c.LineEnd)
				{
					// Shift all chunks top by lineTop
					foreach (var lineChunk in line.Chunks)
					{
						lineChunk.Top -= lineTop;
					}

					line.Size.Y = lineBottom - lineTop;

					// New line
					_lines.Add(line);

					line = new TextLine
					{
						TextStartIndex = i
					};

					lineTop = lineBottom = 0;
					width = rowWidth;
				}
			}

			// If text ends with '\n', then add additional line
			if (_text[_text.Length - 1] == '\n')
			{
				var additionalLine = new TextLine
				{
					TextStartIndex = _text.Length
				};

				var lineSize = _currentFont.MeasureString(" ");
				additionalLine.Size.Y = (int)lineSize.Y;

				_lines.Add(additionalLine);
			}

			// Calculate size
			size = Utility.PointZero;
			for (i = 0; i < _lines.Count; ++i)
			{
				line = _lines[i];

				line.LineIndex = i;
				line.Top = size.Y;

				for (var j = 0; j < line.Chunks.Count; ++j)
				{
					var chunk = line.Chunks[j];
					chunk.LineIndex = line.LineIndex;
					chunk.ChunkIndex = j;
				}

				if (line.Size.X > size.X)
				{
					size.X = line.Size.X;
				}

				size.Y += line.Size.Y;

				if (i < _lines.Count - 1)
				{
					size.Y += verticalSpacing;
				}
			}

			return _lines;
		}
	}
}

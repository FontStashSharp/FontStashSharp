using System;
using System.Collections.Generic;
using System.Text;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp.RichText
{
	public class FormattedText
	{
		public const int NewLineWidth = 0;
		public const string Commands = "cfivs";

		private SpriteFontBase _font;
		private string _text = string.Empty;
		private int _verticalSpacing;
		private int? _width;
		private readonly List<TextLine> _lines = new List<TextLine>();
		private bool _calculateGlyphs, _supportsCommands = true;
		private Point _size;
		private bool _dirty = true;
		private StringBuilder _stringBuilder = new StringBuilder();
		private readonly Dictionary<int, Point> _measures = new Dictionary<int, Point>();

		private Color? _currentColor;
		private SpriteFontBase _currentFont;
		private int _currentVerticalOffset;

		public SpriteFontBase Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == _font)
				{
					return;
				}

				_font = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == _text)
				{
					return;
				}

				_text = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public int VerticalSpacing
		{
			get
			{
				return _verticalSpacing;
			}

			set
			{
				if (value == _verticalSpacing)
				{
					return;
				}

				_verticalSpacing = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public int? Width
		{
			get
			{
				return _width;
			}
			set
			{
				if (value == _width)
				{
					return;
				}

				_width = value;
				InvalidateLayout();
			}
		}

		public List<TextLine> Lines
		{
			get
			{
				Update();
				return _lines;
			}
		}

		public Point Size
		{
			get
			{
				Update();
				return _size;
			}
		}

		public bool CalculateGlyphs
		{
			get
			{
				return _calculateGlyphs;
			}

			set
			{
				if (value == _calculateGlyphs)
				{
					return;
				}

				_calculateGlyphs = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public bool SupportsCommands
		{
			get
			{
				return _supportsCommands;
			}

			set
			{
				if (value == _supportsCommands)
				{
					return;
				}

				_supportsCommands = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public bool IgnoreColorCommand { get; set; } = false;

		public Func<string, SpriteFontBase> FontResolver { get; set; }

		private bool ProcessCommand(ref int i, ref ChunkInfo r)
		{
			if (i > r.StartIndex)
			{
				// Break right here, so the command
				// would be processed in the next chunk
				r.LineEnd = false;
				return true;
			}

			var result = false;

			var command = _text[i + 1].ToString();
			if (!Commands.Contains(command))
			{
				throw new Exception($"Unknown command '{command}'.");
			}

			int j;
			if (_text[i + 2] == 'd')
			{
				switch (command)
				{
					case "f":
						// Switch to default font
						_currentFont = Font;
						break;
					case "v":
						_currentVerticalOffset = 0;
						break;
					default:
						throw new Exception($"Can't use 'd' parameter for command {command}");
				}
				j = i + 2;
			}
			else
			{
				// Find end
				var startPos = i + 3;
				j = _text.IndexOf(']', startPos);

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
						if (FontResolver == null)
						{
							throw new Exception($"FontResolver isnt set");
						}

						_currentFont = FontResolver(parameters);
						break;
					case "s":
						var size = int.Parse(parameters);
						r.Type = ChunkInfoType.Space;
						r.X = size;
						r.Y = 0;
						r.LineEnd = false;
						result = true;
						break;
					case "v":
						_currentVerticalOffset = int.Parse(parameters);
						break;

				}
			}

			r.StartIndex = j + 1;
			i = j;

			return result;
		}

		private ChunkInfo LayoutRow(int startIndex, int? width)
		{
			var r = new ChunkInfo
			{
				StartIndex = startIndex,
				LineEnd = true
			};

			if (string.IsNullOrEmpty(_text))
			{
				return r;
			}

			_stringBuilder.Clear();
			int? lastBreakPosition = null;
			Point? lastBreakMeasure = null;

			for (var i = r.StartIndex; i < _text.Length; ++i)
			{
				var c = _text[i];

				if (char.IsHighSurrogate(c))
				{
					_stringBuilder.Append(c);
					++r.CharsCount;
					continue;
				}

				if (c == '\\' && SupportsCommands)
				{
					if (i < _text.Length - 1 && _text[i + 1] == 'n')
					{
						var sz2 = new Point(r.X + NewLineWidth, Math.Max(r.Y, _currentFont.LineHeight));

						// Break right here
						r.SkipCount += 2;
						r.X = sz2.X;
						r.Y = sz2.Y;
						break;
					}

					if (i < _text.Length - 2)
					{
						if (ProcessCommand(ref i, ref r))
						{
							return r;
						}

						continue;
					}
				}

				_stringBuilder.Append(c);

				Point sz;
				if (c != '\n')
				{
					var v = _currentFont.MeasureString(_stringBuilder);
					sz = new Point((int)v.X, _currentFont.LineHeight);
				}
				else
				{
					sz = new Point(r.X + NewLineWidth, Math.Max(r.Y, _currentFont.LineHeight));

					// Break right here
					++r.CharsCount;
					r.X = sz.X;
					r.Y = sz.Y;
					break;
				}

				if (width != null && sz.X > width.Value)
				{
					if (lastBreakPosition != null)
					{
						r.CharsCount = lastBreakPosition.Value - r.StartIndex;
					}

					if (lastBreakMeasure != null)
					{
						r.X = lastBreakMeasure.Value.X;
						r.Y = lastBreakMeasure.Value.Y;
					}

					break;
				}

				if (char.IsWhiteSpace(c))
				{
					lastBreakPosition = i + 1;
					lastBreakMeasure = sz;
				}

				++r.CharsCount;
				r.X = sz.X;
				r.Y = sz.Y;
			}

			return r;
		}

		private static int GetMeasureKey(int? width)
		{
			return width != null ? width.Value : -1;
		}

		private void ResetCurrents()
		{
			_currentColor = null;
			_currentFont = Font;
			_currentVerticalOffset = 0;
		}

		public Point Measure(int? width)
		{
			var key = GetMeasureKey(width);

			Point result;
			if (_measures.TryGetValue(key, out result))
			{
				return result;
			}

			ResetCurrents();
			if (!string.IsNullOrEmpty(_text))
			{
				var i = 0;
				var y = 0;

				var remainingWidth = width;
				var lineWidth = 0;
				while (i < _text.Length)
				{
					var chunkInfo = LayoutRow(i, remainingWidth);
					if (i == chunkInfo.StartIndex && chunkInfo.CharsCount == 0)
						break;

					lineWidth += chunkInfo.X;
					i = chunkInfo.StartIndex + chunkInfo.CharsCount + chunkInfo.SkipCount;

					if (remainingWidth.HasValue)
					{
						remainingWidth = remainingWidth.Value - chunkInfo.X;
					}

					if (chunkInfo.LineEnd)
					{
						if (lineWidth > result.X)
						{
							result.X = lineWidth;
						}

						lineWidth = 0;
						remainingWidth = width;

						y += chunkInfo.Y;
						y += _verticalSpacing;
					}
				}

				// If text ends with '\n', then add additional line to the measure
				if (_text[_text.Length - 1] == '\n')
				{
					var lineSize = _currentFont.MeasureString(" ");
					y += (int)lineSize.Y;
				}

				result.Y = y;
			}

			if (result.Y == 0)
			{
				result.Y = _currentFont.LineHeight;
			}

			_measures[key] = result;

			return result;
		}

		private void Update()
		{
			if (!_dirty)
			{
				return;
			}

			_lines.Clear();

			if (string.IsNullOrEmpty(_text))
			{ 
				_dirty = false;
				return;
			}

			ResetCurrents();

			var i = 0;
			var line = new TextLine
			{
				TextStartIndex = i
			};

			var width = Width;
			while (i < _text.Length)
			{
				var c = LayoutRow(i, width);
				if (i == c.StartIndex && c.CharsCount == 0)
					break;

				BaseChunk chunk = null;
				switch (c.Type)
				{
					case ChunkInfoType.Text:
						chunk = new TextChunk(_currentFont, _text.Substring(c.StartIndex, c.CharsCount), new Point(c.X, c.Y), CalculateGlyphs)
						{
							TextStartIndex = i,
							Color = _currentColor,
							Top = _currentVerticalOffset,
						};
						break;
					case ChunkInfoType.Space:
						chunk = new SpaceChunk(c.X);
						break;
					case ChunkInfoType.Image:
						break;
				}

				width -= chunk.Size.X;

				i = c.StartIndex + c.CharsCount + c.SkipCount;

				line.Chunks.Add(chunk);

				var asText = chunk as TextChunk;
				if (asText != null)
				{
					line.Count += asText.Count;
				}

				line.Size.X += chunk.Size.X;
				if (chunk.Size.Y > line.Size.Y)
				{
					line.Size.Y = chunk.Size.Y;
				}

				if (c.LineEnd)
				{
					// Determine the line's top and bottom
					var lineTop = 0;
					var lineBottom = 0;

					foreach (var lineChunk in line.Chunks)
					{
						if (lineChunk.Top < lineTop)
						{
							lineTop = lineChunk.Top;
						}

						if (lineChunk.Top + lineChunk.Size.Y > lineBottom)
						{
							lineBottom = lineChunk.Top + lineChunk.Size.Y;
						}
					}

					// Shift all chunks top by lineTop
					foreach (var lineChunk in line.Chunks)
					{
						//						lineChunk.Top -= lineTop;
					}

					line.Size.Y = lineBottom - lineTop;

					// New line
					_lines.Add(line);

					line = new TextLine
					{
						TextStartIndex = i
					};

					width = Width;
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
			_size = Utility.PointZero;
			for (i = 0; i < _lines.Count; ++i)
			{
				line = _lines[i];

				line.LineIndex = i;
				line.Top = _size.Y;

				for (var j = 0; j < line.Chunks.Count; ++j)
				{
					var chunk = line.Chunks[j];
					chunk.LineIndex = line.LineIndex;
					chunk.ChunkIndex = j;
				}

				if (line.Size.X > _size.X)
				{
					_size.X = line.Size.X;
				}

				_size.Y += line.Size.Y;

				if (i < _lines.Count - 1)
				{
					_size.Y += _verticalSpacing;
				}
			}

			var key = GetMeasureKey(Width);
			_measures[key] = _size;

			_dirty = false;
		}

		public TextLine GetLineByCursorPosition(int cursorPosition)
		{
			Update();

			if (_lines.Count == 0)
			{
				return null;
			}

			if (cursorPosition < 0)
			{
				return _lines[0];
			}

			for (var i = 0; i < _lines.Count; ++i)
			{
				var s = _lines[i];
				if (s.TextStartIndex <= cursorPosition && cursorPosition < s.TextStartIndex + s.Count)
				{
					return s;
				}
			}

			return _lines[_lines.Count - 1];
		}

		public TextLine GetLineByY(int y)
		{
			if (string.IsNullOrEmpty(_text) || y < 0)
			{
				return null;
			}

			Update();

			for (var i = 0; i < _lines.Count; ++i)
			{
				var s = _lines[i];

				if (s.Top <= y && y < s.Top + s.Size.Y)
				{
					return s;
				}
			}

			return null;
		}

		public GlyphInfo GetGlyphInfoByIndex(int charIndex)
		{
			var strings = Lines;

			foreach (var si in strings)
			{
				if (charIndex >= si.Count)
				{
					charIndex -= si.Count;
				}
				else
				{
					return si.GetGlyphInfoByIndex(charIndex);
				}
			}

			return null;
		}

		public void Draw(IFontStashRenderer renderer, Vector2 position, Color color,
			Vector2? sourceScale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f)
		{
			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Utility.BuildTransform(position, ref scale, rotation, origin, out transformation);

			var pos = Utility.Vector2Zero;
			foreach (var line in Lines)
			{
				pos.X = 0;
				foreach (var chunk in line.Chunks)
				{
					if (!IgnoreColorCommand && chunk.Color != null)
					{
						color = chunk.Color.Value;
					}

					var p = pos;
					p.Y += chunk.Top;
					p = p.Transform(ref transformation);
					chunk.Draw(renderer, p, color, scale, rotation, layerDepth);

					pos.X += chunk.Size.X;
				}

				pos.Y += line.Size.Y;
				pos.Y += _verticalSpacing;
			}
		}

#if MONOGAME || FNA || STRIDE

		public void Draw(SpriteBatch batch, Vector2 position, Color color,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;
			Draw(renderer, position, color, scale, rotation, origin, layerDepth);
		}

#endif

		private void InvalidateLayout()
		{
			_dirty = true;
		}

		private void InvalidateMeasures()
		{
			_measures.Clear();
		}
	}
}


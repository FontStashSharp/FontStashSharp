using System;
using System.Collections.Generic;
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
		private SpriteFontBase _font;
		private string _text = string.Empty;
		private int _verticalSpacing;
		private int? _width;
		private List<TextLine> _lines;
		private bool _calculateGlyphs, _supportsCommands = true;
		private Point _size;
		private bool _dirty = true;
		private readonly Dictionary<int, Point> _measures = new Dictionary<int, Point>();
		private readonly LayoutBuilder _layoutBuilder = new LayoutBuilder();

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

		public static Func<string, SpriteFontBase> FontResolver { get; set; }
		public static Func<string, TextureInfo> ImageResolver { get; set; }

		private static int GetMeasureKey(int? width)
		{
			return width != null ? width.Value : -1;
		}

		private void Update()
		{
			if (!_dirty)
			{
				return;
			}

			Point size;
			_lines = _layoutBuilder.Layout(Text, Font, VerticalSpacing, Width, SupportsCommands, CalculateGlyphs, out size);
			_size = size;

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
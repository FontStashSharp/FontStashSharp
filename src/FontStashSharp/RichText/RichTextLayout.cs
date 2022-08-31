using System.Collections.Generic;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
#endif

namespace FontStashSharp.RichText
{
	public class RichTextLayout
	{
		private SpriteFontBase _font;
		private string _text = string.Empty;
		private int? _width;
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
				return _layoutBuilder.VerticalSpacing;
			}

			set
			{
				if (value == _layoutBuilder.VerticalSpacing)
				{
					return;
				}

				_layoutBuilder.VerticalSpacing = value;
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
				return _layoutBuilder.Lines;
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
				return _layoutBuilder.CalculateGlyphs;
			}

			set
			{
				if (value == _layoutBuilder.CalculateGlyphs)
				{
					return;
				}

				_layoutBuilder.CalculateGlyphs = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public bool SupportsCommands
		{
			get
			{
				return _layoutBuilder.SupportsCommands;
			}

			set
			{
				if (value == _layoutBuilder.SupportsCommands)
				{
					return;
				}

				_layoutBuilder.SupportsCommands = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}

		public bool IgnoreColorCommand { get; set; } = false;

		public char CommandPrefix
		{
			get => _layoutBuilder.CommandPrefix;

			set
			{
				if (value == _layoutBuilder.CommandPrefix)
				{
					return;
				}

				_layoutBuilder.CommandPrefix = value;
				InvalidateLayout();
				InvalidateMeasures();
			}
		}


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

			_size = _layoutBuilder.Layout(Text, Font, Width);

			var key = GetMeasureKey(Width);
			_measures[key] = _size;

			_dirty = false;
		}

		public Point Measure(int? width)
		{
			var result = Utility.PointZero;

			var key = GetMeasureKey(width);
			if (_measures.TryGetValue(key, out result))
			{
				return result;
			}

			result = _layoutBuilder.Layout(Text, Font, width, true);
			_measures[key] = result;

			return result;
		}

		public TextLine GetLineByCursorPosition(int cursorPosition)
		{
			Update();

			if (Lines.Count == 0)
			{
				return null;
			}

			if (cursorPosition < 0)
			{
				return Lines[0];
			}

			for (var i = 0; i < Lines.Count; ++i)
			{
				var s = Lines[i];
				if (s.TextStartIndex <= cursorPosition && cursorPosition < s.TextStartIndex + s.Count)
				{
					return s;
				}
			}

			return Lines[Lines.Count - 1];
		}

		public TextLine GetLineByY(int y)
		{
			if (string.IsNullOrEmpty(_text) || y < 0)
			{
				return null;
			}

			Update();

			var py = 0;
			for (var i = 0; i < Lines.Count; ++i)
			{
				var s = Lines[i];

				if (py <= y && y < py + s.Size.Y)
				{
					return s;
				}

				py += s.Size.Y;
				py += VerticalSpacing;
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
			Vector2? sourceScale = null, float rotation = 0,
			Vector2 origin = default(Vector2), float layerDepth = 0.0f)
		{
			Update();

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Utility.BuildTransform(position, scale, rotation, origin, out transformation);

			var pos = Utility.Vector2Zero;
			foreach (var line in Lines)
			{
				pos.X = 0;
				foreach (var chunk in line.Chunks)
				{
					var chunkColor = color;
					if (!IgnoreColorCommand && chunk.Color != null)
					{
						chunkColor = chunk.Color.Value;
					}

					var p = pos;
					p.Y += chunk.VerticalOffset;
					p = p.Transform(ref transformation);
					chunk.Draw(renderer, p, chunkColor, scale, rotation, layerDepth);

					pos.X += chunk.Size.X;
				}

				pos.Y += line.Size.Y;
				pos.Y += VerticalSpacing;
			}
		}

		public void Draw(IFontStashRenderer2 renderer, Vector2 position, Color color,
			Vector2? sourceScale = null, float rotation = 0,
			Vector2 origin = default(Vector2), float layerDepth = 0.0f)
		{
			Update();

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Utility.BuildTransform(position, scale, rotation, origin, out transformation);

			var pos = Utility.Vector2Zero;
			foreach (var line in Lines)
			{
				pos.X = 0;
				foreach (var chunk in line.Chunks)
				{
					var chunkColor = color;
					if (!IgnoreColorCommand && chunk.Color != null)
					{
						chunkColor = chunk.Color.Value;
					}

					var p = pos;
					p.Y += chunk.VerticalOffset;
					p = p.Transform(ref transformation);
					chunk.Draw(renderer, p, chunkColor, scale, rotation, layerDepth);

					pos.X += chunk.Size.X;
				}

				pos.Y += line.Size.Y;
				pos.Y += VerticalSpacing;
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
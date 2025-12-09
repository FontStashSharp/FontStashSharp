using System.Collections.Generic;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp.RichText
{
	public class RichTextLayout
	{
		private SpriteFontBase _font;
		private string _text = string.Empty;
		private int? _width, _height;
		private Point _size;
		private bool _dirty = true;
		private readonly Dictionary<int, Point> _measures = new Dictionary<int, Point>();
		private readonly LayoutBuilder _layoutBuilder;
		private readonly FSRenderContext _renderContext = new FSRenderContext();

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

		public int? Height
		{
			get
			{
				return _height;
			}

			set
			{
				if (value == _height)
				{
					return;
				}

				_height = value;
				InvalidateLayout();
			}
		}

		/// <summary>
		/// The method used to abbreviate overflowing text.
		/// </summary>
		public AutoEllipsisMethod AutoEllipsisMethod
		{
			get => _layoutBuilder.AutoEllipsisMethod;
			set
			{
				_layoutBuilder.AutoEllipsisMethod = value;
				InvalidateLayout();
			}
		}

		/// <summary>
		/// The string to use as ellipsis.
		/// </summary>
		public string AutoEllipsisString
		{
			get => _layoutBuilder.AutoEllipsisString;
			set
			{
				_layoutBuilder.AutoEllipsisString = value;
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

		public bool ShiftByTop
		{
			get
			{
				return _layoutBuilder.ShiftByTop;
			}

			set
			{
				if (value == _layoutBuilder.ShiftByTop)
				{
					return;
				}

				_layoutBuilder.ShiftByTop = value;
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

		public RichTextLayout()
		{
			_layoutBuilder = new LayoutBuilder(new RichTextSettings());
		}

		public RichTextLayout(RichTextSettings richTextSettings)
		{
			_layoutBuilder = new LayoutBuilder(richTextSettings);
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

			_size = _layoutBuilder.Layout(Text, Font, Width, Height);

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

			result = _layoutBuilder.Layout(Text, Font, width, null, true);
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

		public TextChunkGlyph? GetGlyphInfoByIndex(int charIndex)
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

		private void Draw(Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, TextHorizontalAlignment horizontalAlignment)
		{
			Update();

			var scale = sourceScale ?? Utility.DefaultScale;
			_renderContext.Prepare(position, rotation, origin, scale, layerDepth);

			var pos = Utility.Vector2Zero;
			foreach (var line in Lines)
			{
				pos.X = 0;

				if (horizontalAlignment == TextHorizontalAlignment.Center)
				{
					pos.X -= line.Size.X / 2;
				}
				else if (horizontalAlignment == TextHorizontalAlignment.Right)
				{
					pos.X -= line.Size.X;
				}
				foreach (var chunk in line.Chunks)
				{
					var chunkColor = color;
					if (!IgnoreColorCommand && chunk.Color != null)
					{
						chunkColor = chunk.Color.Value * (color.A / 255f);
					}

					chunk.Draw(_renderContext, pos + new Vector2(0, chunk.VerticalOffset), chunkColor);
					pos.X += chunk.Size.X;
				}

				pos.Y += line.Size.Y;
				pos.Y += VerticalSpacing;
			}
		}

		public void Draw(IFontStashRenderer renderer, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
		{
			_renderContext.SetRenderer(renderer);
			Draw(position, color, rotation, origin, scale, layerDepth, horizontalAlignment);
		}

		public void Draw(IFontStashRenderer2 renderer, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
		{
			_renderContext.SetRenderer(renderer);
			Draw(position, color, rotation, origin, scale, layerDepth, horizontalAlignment);
		}

#if MONOGAME || FNA || XNA || STRIDE

		public void Draw(SpriteBatch batch, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null, 
			float layerDepth = 0.0f, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
		{
			var renderer = SpriteBatchRenderer.Instance;
			renderer.Batch = batch;
			Draw(renderer, position, color, rotation, origin, scale, layerDepth, horizontalAlignment);
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
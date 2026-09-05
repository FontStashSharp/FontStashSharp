using System;
using System.Collections.Generic;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA || KNI || XNA
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
	/// <summary>
	/// Manages layout and rendering of richly-formatted text with support for fonts, colors, styles, and images.
	/// </summary>
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

		/// <summary>
		/// Gets or sets the default sprite font used for rendering.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the rich text string to layout and render.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the vertical spacing between lines in pixels.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the maximum width for text layout, or null for unlimited width.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the maximum height for text layout, or null for unlimited height.
		/// </summary>
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

		/// <summary>
		/// Gets the laid out text lines after updating the layout.
		/// </summary>
		public List<TextLine> Lines
		{
			get
			{
				Update();
				return _layoutBuilder.Lines;
			}
		}

		/// <summary>
		/// Gets the size of the laid out text.
		/// </summary>
		public Point Size
		{
			get
			{
				Update();
				return _size;
			}
		}

		/// <summary>
		/// Gets or sets whether to calculate glyph information during layout.
		/// </summary>
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

		/// <summary>
		/// Gets or sets whether to support text formatting commands.
		/// </summary>
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

		/// <summary>
		/// Gets or sets whether to shift text by the top of the font metrics.
		/// </summary>
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

		/// <summary>
		/// Gets or sets whether to ignore color commands in the text.
		/// </summary>
		public bool IgnoreColorCommand { get; set; } = false;

		/// <summary>
		/// Gets or sets the character used to prefix commands in the text.
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the RichTextLayout class with default settings.
		/// </summary>
		public RichTextLayout()
		{
			_layoutBuilder = new LayoutBuilder(new RichTextSettings());
		}

		/// <summary>
		/// Initializes a new instance of the RichTextLayout class with the specified settings.
		/// </summary>
		/// <param name="richTextSettings">The settings to use for text layout</param>
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

		/// <summary>
		/// Measures the size of the text with the specified width constraint.
		/// </summary>
		/// <param name="width">The maximum width for measurement, or null for unlimited width</param>
		/// <returns>The size of the measured text</returns>
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

		/// <summary>
		/// Gets the text line at the specified cursor position.
		/// </summary>
		/// <param name="cursorPosition">The cursor position in the text</param>
		/// <returns>The text line containing the cursor position</returns>
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

		/// <summary>
		/// Gets the text line at the specified Y coordinate.
		/// </summary>
		/// <param name="y">The Y coordinate to search for</param>
		/// <returns>The text line at the specified Y coordinate</returns>
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

		/// <summary>
		/// Gets the glyph information at the specified character index.
		/// </summary>
		/// <param name="charIndex">The character index to get glyph information for</param>
		/// <returns>The glyph information at the specified index</returns>
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

		/// <summary>
		/// Draws the text layout using the specified renderer.
		/// </summary>
		/// <param name="renderer">The font stash renderer to use</param>
		/// <param name="position">The position to draw at</param>
		/// <param name="color">The color to render the text in</param>
		/// <param name="rotation">The rotation in radians</param>
		/// <param name="origin">The center of rotation</param>
		/// <param name="scale">The scale factors, or null for default (1, 1)</param>
		/// <param name="layerDepth">The layer depth for drawing</param>
		/// <param name="horizontalAlignment">The horizontal alignment of the text</param>
		public void Draw(IFontStashRenderer renderer, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
		{
			_renderContext.SetRenderer(renderer);
			Draw(position, color, rotation, origin, scale, layerDepth, horizontalAlignment);
		}

		/// <summary>
		/// Draws the text layout using the specified renderer.
		/// </summary>
		/// <param name="renderer">The font stash renderer to use</param>
		/// <param name="position">The position to draw at</param>
		/// <param name="color">The color to render the text in</param>
		/// <param name="rotation">The rotation in radians</param>
		/// <param name="origin">The center of rotation</param>
		/// <param name="scale">The scale factors, or null for default (1, 1)</param>
		/// <param name="layerDepth">The layer depth for drawing</param>
		/// <param name="horizontalAlignment">The horizontal alignment of the text</param>
		public void Draw(IFontStashRenderer2 renderer, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
		{
			_renderContext.SetRenderer(renderer);
			Draw(position, color, rotation, origin, scale, layerDepth, horizontalAlignment);
		}

		/// <summary>
		/// Draws the text layout using the specified SDF text renderer.
		/// </summary>
		/// <param name="renderer">The SDF text renderer to use</param>
		/// <param name="position">The position to draw at</param>
		/// <param name="color">The color to render the text in</param>
		/// <param name="rotation">The rotation in radians</param>
		/// <param name="origin">The center of rotation</param>
		/// <param name="scale">The scale factors, or null for default (1, 1)</param>
		/// <param name="layerDepth">The layer depth for drawing</param>
		/// <param name="horizontalAlignment">The horizontal alignment of the text</param>
		public void Draw(ISDFTextRenderer renderer, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default(Vector2), Vector2? scale = null,
			float layerDepth = 0.0f, TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Left)
		{
			_renderContext.SetRenderer(renderer);
			Draw(position, color, rotation, origin, scale, layerDepth, horizontalAlignment);
		}

#if MONOGAME || FNA || KNI || XNA || STRIDE

		/// <summary>
		/// Draws the text layout using the specified sprite batch.
		/// </summary>
		/// <param name="batch">The sprite batch to draw with</param>
		/// <param name="position">The position to draw at</param>
		/// <param name="color">The color to render the text in</param>
		/// <param name="rotation">The rotation in radians</param>
		/// <param name="origin">The center of rotation</param>
		/// <param name="scale">The scale factors, or null for default (1, 1)</param>
		/// <param name="layerDepth">The layer depth for drawing</param>
		/// <param name="horizontalAlignment">The horizontal alignment of the text</param>
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
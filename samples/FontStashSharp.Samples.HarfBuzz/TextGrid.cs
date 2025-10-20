using System;
using System.Collections.Generic;

namespace FontStashSharp.Samples
{
	internal class TextGrid
	{
		public class GridCell
		{
			public int X { get; }
			public int Y { get; }
			public SpriteFontBase Font { get; }
			public string Text { get; }
			public int ScreenX { get; set; }
			public int ScreenY { get; set; }

			public GridCell(int x, int y, SpriteFontBase font, string text)
			{
				X = x;
				Y = y;
				Font = font ?? throw new ArgumentNullException(nameof(font));
				Text = text;
			}

			public override string ToString() => $"{X}, {Y}, '{Text}'";
		}

		private readonly Dictionary<int, GridCell> _cells = new Dictionary<int, GridCell>();
		private int _xSpacing, _ySpacing;
		private bool _dirty = true;

		public int XSpacing
		{
			get => _xSpacing;

			set
			{
				if (value == _xSpacing)
				{
					return;
				}

				_xSpacing = value;
				Invalidate();
			}
		}

		public int YSpacing
		{
			get => _ySpacing;

			set
			{
				if (value == _ySpacing)
				{
					return;
				}

				_ySpacing = value;
				Invalidate();
			}
		}

		public IEnumerable<GridCell> Cells
		{
			get
			{
				Update();

				return _cells.Values;
			}
		}


		private static int GetKey(int x, int y) => (y << 16) + x;

		public void SetCell(int x, int y, SpriteFontBase font, string text)
		{
			var cell = new GridCell(x, y, font, text);
			var key = GetKey(x, y);

			_cells[key] = cell;
			_dirty = true;
		}

		private void Update()
		{
			if (!_dirty)
			{
				return;
			}

			// Calculate maxes
			var maxX = 0;
			var maxY = 0;
			foreach (var pair in _cells)
			{
				var cell = pair.Value;
				maxX = Math.Max(maxX, cell.X);
				maxY = Math.Max(maxX, cell.Y);
			}

			// Set widths and heights
			var widths = new int[maxX + 1];
			var heights = new int[maxY + 1];

			foreach (var pair in _cells)
			{
				var cell = pair.Value;
				var size = cell.Font.MeasureString(cell.Text,
					characterSpacing: Game1.CharacterSpacing, lineSpacing: Game1.LineSpacing,
					effect: Game1.Instance.CurrentEffect, effectAmount: Game1.EffectAmount);

				widths[cell.X] = Math.Max(widths[cell.X], (int)size.X);
				heights[cell.Y] = Math.Max(heights[cell.Y], (int)size.Y);
			}

			// Do the layout
			foreach (var pair in _cells)
			{
				var cell = pair.Value;

				cell.ScreenX = 0;
				for (var x = 0; x < cell.X; ++x)
				{
					cell.ScreenX += widths[x];
					cell.ScreenX += XSpacing;
				}

				cell.ScreenY = 0;
				for (var y = 0; y < cell.Y; ++y)
				{
					cell.ScreenY += heights[y];
					cell.ScreenY += YSpacing;
				}
			}

			_dirty = false;
		}

		private void Invalidate()
		{
			_dirty = true;
		}
	}
}

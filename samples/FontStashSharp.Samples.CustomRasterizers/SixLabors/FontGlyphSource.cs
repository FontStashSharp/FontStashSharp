using System;
using System.Collections.Generic;
using System.Numerics;
using FontStashSharp.Samples.SixLabors;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TrippyGL.Fonts.Building
{
	/// <summary>
	/// An implementation of <see cref="IGlyphSource"/> that sources it's glyphs from
	/// a <see cref="SixLabors.Fonts"/> font.
	/// </summary>
	public sealed class FontGlyphSource
	{
		private const float Dpi = 96;
		private const float PointsPerInch = 72;

		/// <summary>The <see cref="IFontInstance"/> from which this <see cref="FontGlyphSource"/> gets glyph data.</summary>
		public readonly IFontInstance FontInstance;

		/// <summary>Configuration for how glyphs should be rendered.</summary>
		public DrawingOptions DrawingOptions;

		/// <summary>Whether to include kerning if present in the font. Default is true.</summary>
		public bool IncludeKerningIfPresent = true;

		/// <summary>
		/// Creates a <see cref="FontGlyphSource"/> instance.
		/// </summary>
		public FontGlyphSource(IFontInstance fontInstance)
		{
			FontInstance = fontInstance ?? throw new ArgumentNullException(nameof(fontInstance));

			DrawingOptions = new DrawingOptions
			{
				ShapeOptions = { IntersectionRule = IntersectionRule.Nonzero },
			};
		}

		/// <summary>
		/// Creates the <see cref="IPathCollection"/> for all the characters, also getting their colors,
		/// glyph sizes and render offsets.
		/// </summary>
		public GlyphPath CreatePath(int size, int codepoint)
		{
			ColorGlyphRenderer glyphRenderer = new ColorGlyphRenderer();
			glyphRenderer.Reset();
			GlyphInstance glyphInstance = FontInstance.GetGlyph(codepoint);
			var pointSize = size * PointsPerInch / Dpi;
			glyphInstance.RenderTo(glyphRenderer, pointSize, new Vector2(0, 0), new Vector2(Dpi, Dpi), 0);
			IPathCollection p = glyphRenderer.Paths;
			RectangleF bounds = p.Bounds;

			var area = bounds.Width * bounds.Height;
			if (area == 0)
			{
				return null;
			}

			if (char.IsWhiteSpace((char)codepoint))
			{
				p = null;
			}

			return new GlyphPath
			{
				Size = size,
				Codepoint = codepoint,
				Bounds = new Microsoft.Xna.Framework.Rectangle((int)bounds.X, (int)bounds.Y,
				(int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height)),
				Paths = p
			};
		}

		public float GetAdvance(int size, int codepoint)
		{
			GlyphInstance inst = FontInstance.GetGlyph(codepoint);
			return inst.AdvanceWidth * size / FontInstance.EmSize;
		}

		public Vector2 GetKerning(int size, int codepoint1, int codepoint2)
		{
			GlyphInstance aInstance = FontInstance.GetGlyph(codepoint1);
			Vector2 offset = FontInstance.GetOffset(FontInstance.GetGlyph(codepoint2), aInstance);
			return offset * size / FontInstance.EmSize;
		}

		public void DrawGlyphToImage(GlyphPath glyphPath, System.Drawing.Point location, Image<Rgba32> image)
		{
			var paths = glyphPath.Paths;
			if (paths == null)
			{
				return;
			}

			paths = paths.Translate(location.X - glyphPath.Bounds.X, location.Y - glyphPath.Bounds.Y);
			DrawColoredPaths(image, paths);
		}

		/// <summary>
		/// Draws a collection of paths with the given colors onto the image.
		/// </summary>
		private void DrawColoredPaths(Image<Rgba32> image, IPathCollection paths)
		{
			IEnumerator<IPath> pathEnumerator = paths.GetEnumerator();

			int i = 0;
			while (pathEnumerator.MoveNext())
			{
				IPath path = pathEnumerator.Current;
				image.Mutate(x => x.Fill(DrawingOptions, Color.White, path));
				i++;
			}
		}
	}
}
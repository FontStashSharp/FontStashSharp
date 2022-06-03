using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;

namespace TrippyGL.Fonts.Building
{
    /// <summary>
    /// A rendering surface that fonts can use to generate shapes.
    /// </summary>
    internal sealed class ColorGlyphRenderer : IColorGlyphRenderer
    {
        private readonly PathBuilder builder = new PathBuilder();
        private readonly List<IPath> paths = new List<IPath>();
        private readonly List<Color?> colors = new List<Color?>();

        private Vector2 currentPoint = default;
        private Color? currentColor = null;

        /// <summary>
        /// Creates a new <see cref="ColorGlyphRenderer"/> instance.
        /// </summary>
        public ColorGlyphRenderer()
        {
            builder = new PathBuilder();
        }

        /// <summary>Get the colors for each path, where null means default color.</summary>
        public Color?[] PathColors => colors.ToArray();

        /// <summary>Gets the paths that have been rendered by this <see cref="ColorGlyphRenderer"/>.</summary>
        public IPathCollection Paths => new PathCollection(paths);

        void IGlyphRenderer.EndText() { }

        void IGlyphRenderer.BeginText(FontRectangle rect) { }

        bool IGlyphRenderer.BeginGlyph(FontRectangle rect, GlyphRendererParameters cachKey)
        {
            currentColor = null;
            builder.Clear();
            return true;
        }

        void IGlyphRenderer.BeginFigure()
        {
            builder.StartFigure();
        }

        void IGlyphRenderer.CubicBezierTo(Vector2 secondControlPoint, Vector2 thirdControlPoint, Vector2 point)
        {
            builder.AddBezier(currentPoint, secondControlPoint, thirdControlPoint, point);
            currentPoint = point;
        }

        void IGlyphRenderer.EndGlyph()
        {
            paths.Add(builder.Build());
            colors.Add(currentColor);
        }

        void IColorGlyphRenderer.SetColor(GlyphColor color)
        {
            currentColor = new Color(new Rgba32(color.Red, color.Green, color.Blue, color.Alpha));
        }

        void IGlyphRenderer.EndFigure()
        {
            builder.CloseFigure();
        }

        void IGlyphRenderer.LineTo(Vector2 point)
        {
            builder.AddLine(currentPoint, point);
            currentPoint = point;
        }

        void IGlyphRenderer.MoveTo(Vector2 point)
        {
            builder.StartFigure();
            currentPoint = point;
        }

        void IGlyphRenderer.QuadraticBezierTo(Vector2 secondControlPoint, Vector2 endPoint)
        {
            Vector2 startPointVector = currentPoint;
            Vector2 controlPointVector = secondControlPoint;
            Vector2 endPointVector = endPoint;

            Vector2 c1 = ((controlPointVector - startPointVector) * 2 / 3) + startPointVector;
            Vector2 c2 = ((controlPointVector - endPointVector) * 2 / 3) + endPointVector;

            builder.AddBezier(startPointVector, c1, c2, endPoint);
            currentPoint = endPoint;
        }

        /// <summary>
        /// Returns whether this <see cref="ColorGlyphRenderer"/> currently has any path colors. That is,
        /// whether the colors list is empty or all it's contents are null.
        /// </summary>
        public bool HasAnyPathColors()
        {
            for (int i = 0; i < colors.Count; i++)
                if (colors[i].HasValue)
                    return true;
            return false;
        }

        /// <summary>
        /// Clears any lists and sets the origin for future renders.
        /// </summary>
        public void Reset(float x, float y)
        {
            builder.Reset();
            builder.SetOrigin(new PointF(x, y));
            paths.Clear();
            colors.Clear();
            currentPoint = default;
            currentColor = null;
        }

        /// <summary>
        /// Clears any lists and sets the origin for future renders to (0, 0).
        /// </summary>
        public void Reset()
        {
            Reset(0, 0);
        }
    }
}

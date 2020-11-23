using Microsoft.Xna.Framework;

namespace FontStashSharp
{
	static class Utility
	{
		public static Vector2 ToVector2(this System.Drawing.PointF r)
		{
			return new Vector2(r.X, r.Y);
		}

		public static Rectangle ToRectangle(this System.Drawing.Rectangle r)
		{
			return new Rectangle(r.Left, r.Top, r.Width, r.Height);
		}

		public static Color ToColor(this System.Drawing.Color c)
		{
			return new Color(c.R, c.G, c.B, c.A);
		}

		public static System.Drawing.Color ToColor(this Color c)
		{
			return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
		}
	}
}

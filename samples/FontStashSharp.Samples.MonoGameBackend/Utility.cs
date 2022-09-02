using Microsoft.Xna.Framework;

namespace FontStashSharp
{
	static class Utility
	{
		public static Vector2 ToXNA(this System.Numerics.Vector2 r)
		{
			return new Vector2(r.X, r.Y);
		}

		public static System.Numerics.Vector2 ToSystemNumerics(this Vector2 r)
		{
			return new System.Numerics.Vector2(r.X, r.Y);
		}

		public static Rectangle ToXNA(this System.Drawing.Rectangle r)
		{
			return new Rectangle(r.Left, r.Top, r.Width, r.Height);
		}

		public static System.Drawing.Rectangle ToSystemDrawing(this Rectangle r)
		{
			return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
		}


		public static Color ToXNA(this FSColor c)
		{
			return new Color(c.R, c.G, c.B, c.A);
		}

		public static FSColor ToFontStashSharp(this Color c)
		{
			return new FSColor(c.R, c.G, c.B, c.A);
		}
	}
}

using Microsoft.Xna.Framework;

namespace FontStashSharp.Samples.MonoGame
{
	static class Utility
	{
		public static Rectangle ToRectangle(this System.Drawing.Rectangle r)
		{
			return new Rectangle(r.Left, r.Top, r.Width, r.Height);
		}

		public static Color ToColor(this FssColor c)
		{
			return new Color(c.PackedValue);
		}

		public static FssColor ToFssColor(this Color c)
		{
			return new FssColor(c.PackedValue);
		}
	}
}

using Microsoft.Xna.Framework;
using System;

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Provides default resolvers for fonts and images in rich text.
	/// </summary>
	public static class RichTextDefaults
	{
		/// <summary>
		/// Gets or sets the function used to resolve font names to sprite fonts.
		/// </summary>
		public static Func<string, SpriteFontBase> FontResolver { get; set; }
		/// <summary>
		/// Gets or sets the function used to resolve image names to renderable objects.
		/// </summary>
		public static Func<string, IRenderable> ImageResolver { get; set; }

		public static Color SDFShadowColor { get; set; } = Color.Black;
		public static Vector2 SDFShadowOffset { get; set; } = new Vector2(1, 1);
		public static Color SDFStrokeColor { get; set; } = Color.Black;
		public static float SDFStrokeThickness { get; set; } = 0.5f;
		public static float SDFStrokeSmoothness { get; set; } = 0.05f;
	}
}

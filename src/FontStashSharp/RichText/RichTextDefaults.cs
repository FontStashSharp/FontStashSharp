using System;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Numerics;
using Color = FontStashSharp.FSColor;
#endif

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

		/// <summary>
		/// Gets or sets the default color used for SDF shadow effects.
		/// </summary>
		public static Color SDFShadowColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the default offset used for SDF shadow effects.
		/// </summary>
		public static Vector2 SDFShadowOffset { get; set; } = new Vector2(1, 1);

		/// <summary>
		/// Gets or sets the default color used for SDF stroke effects.
		/// </summary>
		public static Color SDFStrokeColor { get; set; } = Color.Black;

		/// <summary>
		/// Gets or sets the default thickness used for SDF stroke effects.
		/// </summary>
		public static float SDFStrokeThickness { get; set; } = 0.5f;

		/// <summary>
		/// Gets or sets the default smoothness used for SDF stroke effects.
		/// </summary>
		public static float SDFStrokeSmoothness { get; set; } = 0.05f;
	}
}

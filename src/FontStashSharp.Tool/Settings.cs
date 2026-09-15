using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace FontStashSharp.Tool
{
	internal class Settings
	{
		public static Settings Instance { get; } = new Settings();

		[Category("General")]
		public Color TextColor { get; set; } = Color.White;

		[Category("Supersampling")]
		public float? FontResolutionFactor { get; set; }

		[Category("Supersampling")]
		public int KernelWidth { get; set; }

		[Category("Supersampling")]
		public int KernelHeight { get; set; }

		[Category("SDF")]
		public bool UseSDF { get; set; }

		[Category("SDF")]
		public bool SDFSupersampling { get; set; } = true;

		[Category("SDF")]
		public Color ShadowColor { get; set; } = Color.Black;

		[Category("SDF")]
		public Vector2 ShadowOffset { get; set; } = new Vector2(2, 2);


		private Settings()
		{
		}
	}
}

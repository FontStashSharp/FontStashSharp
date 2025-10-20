using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace FontStashSharp
{
	internal static class Utility
	{
		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static Color ToSystemDrawing(this FSColor color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}
	}
}

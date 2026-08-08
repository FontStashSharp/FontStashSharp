using System;
using System.IO;

namespace FontStashSharp
{
  internal static class Utility
  {
		public static string AssetsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
	}
}

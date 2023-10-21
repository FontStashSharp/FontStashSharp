using System;
using System.IO;
using System.Reflection;

namespace FontStashSharp
{
  internal static class Utility
  {
		public static string AssetsDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				var result = Path.GetDirectoryName(path);
				return Path.Combine(result, "Assets");
			}
		}
	}
}

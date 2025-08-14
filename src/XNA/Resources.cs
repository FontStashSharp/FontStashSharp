using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace FontStashSharp
{
	internal static class Resources
	{
		public static byte[] GetSdfEffectSource()
		{
			var assembly = typeof(Resources).Assembly;

			var names = assembly.GetManifestResourceNames();

#if MONOGAME
			var isOpenGL = PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL;
			var path = PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL? 
				"FontStashSharp.Resources.Effects.MonoGameOGL.bin.sdf-effect.efb":
				"FontStashSharp.Resources.Effects.MonoGameDX11.bin.sdf-effect.efb";
#endif

			byte[] result;

			var ms = new MemoryStream();
			using (var stream = assembly.GetManifestResourceStream(path))
			{
				stream.CopyTo(ms);
				result = ms.ToArray();
			}

			return result;
		}
	}
}

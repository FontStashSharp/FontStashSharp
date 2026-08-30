#if MONOGAME || FNA

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FontStashSharp
{
	internal enum EffectType
	{
		None,
		Stroke,
		Shadow
	}

	internal static class Resources
	{
		private static readonly Effect[] _cache = new Effect[8];

#if FNA
		private const string EffectsResourcePath = "FontStashSharp.Effects.FNA.bin";
#elif MONOGAME
		private static string EffectsResourcePath
		{
			get
			{
				switch (PlatformInfo.GraphicsBackend)
				{
					case GraphicsBackend.OpenGL:
						return "FontStashSharp.Effects.MonoGameOGL.bin";
					case GraphicsBackend.DirectX:
						return "FontStashSharp.Effects.MonoGameDX11.bin";
				}

				throw new NotFiniteNumberException($"Graphics Backend {PlatformInfo.GraphicsBackend} is not supported.");
			}
		}
#endif


		/// <summary>
		/// Open assembly resource stream by relative name
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static Stream OpenResourceStream(this Assembly assembly, string path)
		{
			// Once you figure out the name, pass it in as the argument here.
			var stream = assembly.GetManifestResourceStream(path);
			if (stream == null)
			{
				throw new Exception($"Could not find resource at path '{path}'");
			}

			return stream;
		}

		/// <summary>
		/// Reads assembly resource as byte array by relative name
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static byte[] ReadResourceAsBytes(this Assembly assembly, string path)
		{
			var ms = new MemoryStream();
			using (var input = assembly.OpenResourceStream(path))
			{
				input.CopyTo(ms);

				return ms.ToArray();
			}
		}

		private static Effect LoadEffect(GraphicsDevice graphicsDevice, string name, Dictionary<string, string> defines)
		{
			var key = new StringBuilder();

			var nameWithoutExt = name;
			var ext = string.Empty;
			var extPos = name.LastIndexOf('.');

			if (extPos != -1)
			{
				nameWithoutExt = name.Substring(0, extPos);
				ext = name.Substring(extPos + 1);
			}

			key.Append(nameWithoutExt);

			if (defines != null && defines.Count > 0)
			{
				var keys = (from def in defines.Keys orderby def select def).ToArray();
				foreach (var k in keys)
				{
					key.Append("_");
					key.Append(k);
					var value = defines[k];
					if (value != "1")
					{
						key.Append("_");
						key.Append(value);
					}
				}
			}

			name = Path.ChangeExtension(key.ToString(), "efb");

			var bytes = typeof(Resources).Assembly.ReadResourceAsBytes(EffectsResourcePath + "." + name);

			return new Effect(graphicsDevice, bytes);
		}

		public static Effect GetEffect(GraphicsDevice graphicsDevice, bool superSampling, bool shadow, bool stroke)
		{
			var key = 0;
			if (superSampling)
			{
				key |= 1;
			}

			if (shadow)
			{
				key |= 2;
			}

			if (stroke)
			{
				key |= 4;
			}

			if (_cache[key] != null)
			{
				return _cache[key];
			}

			var defines = new Dictionary<string, string>();

			if (superSampling)
			{
				defines["SUPERSAMPLING"] = "1";
			}

			if (shadow)
			{
				defines["EFFECTSHADOW"] = "1";
			}

			if (stroke)
			{
				defines["EFFECTSTROKE"] = "1";
			}

			_cache[key] = LoadEffect(graphicsDevice, "Text", defines);

			return _cache[key];
		}
	}
}

#endif
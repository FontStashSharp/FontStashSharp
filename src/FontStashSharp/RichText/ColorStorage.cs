using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp.RichText
{
	/// <summary>
	/// Provides storage and utilities for working with named colors.
	/// </summary>
	public static class ColorStorage
	{
		/// <summary>
		/// Stores information about a named color.
		/// </summary>
		public class ColorInfo
		{
			/// <summary>
			/// Gets or sets the color value.
			/// </summary>
			public Color Color { get; set; }
			/// <summary>
			/// Gets or sets the name of the color.
			/// </summary>
			public string Name { get; set; }
		}

		/// <summary>
		/// A dictionary of named colors available for use in rich text.
		/// </summary>
		public static readonly Dictionary<string, ColorInfo> Colors = new Dictionary<string, ColorInfo>();

		static ColorStorage()
		{
			var type = typeof(Color);

#if !STRIDE
			var colors = type.GetRuntimeProperties();
			foreach (var c in colors)
			{
				if (c.PropertyType != typeof(Color))
				{
					continue;
				}

				var value = (Color)c.GetValue(null, null);
				Colors[c.Name.ToLower()] = new ColorInfo
				{
					Color = value,
					Name = c.Name
				};
			}
#else
			var colors = type.GetRuntimeFields();
			foreach (var c in colors)
			{
				if (c.FieldType != typeof(Color))
				{
					continue;
				}

				var value = (Color)c.GetValue(null);
				Colors[c.Name.ToLower()] = new ColorInfo
				{
					Color = value,
					Name = c.Name
				};
			}
#endif
		}

		/// <summary>
		/// Converts a color to its hexadecimal string representation.
		/// </summary>
		/// <param name="c">The color to convert.</param>
		/// <returns>A hexadecimal string in the format #RRGGBBAA.</returns>
		public static string ToHexString(this Color c)
		{
			return string.Format("#{0}{1}{2}{3}",
				c.R.ToString("X2"),
				c.G.ToString("X2"),
				c.B.ToString("X2"),
				c.A.ToString("X2"));
		}

		/// <summary>
		/// Gets the name of the color if it exists in the color storage.
		/// </summary>
		/// <param name="color">The color to look up.</param>
		/// <returns>The name of the color, or null if not found.</returns>
		public static string GetColorName(this Color color)
		{
			foreach (var c in Colors)
			{
				if (c.Value.Color == color)
				{
					return c.Value.Name;
				}
			}

			return null;
		}

		/// <summary>
		/// Parses a color from its name or hexadecimal representation.
		/// </summary>
		/// <param name="name">The color name (e.g., "Red") or hex string (e.g., "#FF0000" or "#FF0000FF").</param>
		/// <returns>The parsed color, or null if the name is not recognized.</returns>
		public static Color? FromName(string name)
		{
			if (name.StartsWith("#"))
			{
				name = name.Substring(1);
				uint u;
				if (uint.TryParse(name, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out u))
				{
					// Parsed value contains color in RGBA form
					// Extract color components

					byte r = 0, g = 0, b = 0, a = 0;

					unchecked
					{
						if (name.Length == 6)
						{
							r = (byte)(u >> 16);
							g = (byte)(u >> 8);
							b = (byte)u;
							a = 255;
						}
						else if (name.Length == 8)
						{
							r = (byte)(u >> 24);
							g = (byte)(u >> 16);
							b = (byte)(u >> 8);
							a = (byte)u;
						}
					}

					return new Color(r, g, b, a);
				}
			}
			else
			{
				ColorInfo result;
				if (Colors.TryGetValue(name.ToLower(), out result))
				{
					return result.Color;
				}
			}

			return null;
		}

		/// <summary>
		/// Creates a color from the specified RGBA components.
		/// </summary>
		/// <param name="r">The red component (0-255).</param>
		/// <param name="g">The green component (0-255).</param>
		/// <param name="b">The blue component (0-255).</param>
		/// <param name="a">The alpha component (0-255). Defaults to 255 (opaque).</param>
		/// <returns>A new color with the specified components.</returns>
		public static Color CreateColor(int r, int g, int b, int a = 255)
		{
			return new Color((byte)r, (byte)g, (byte)b, (byte)a);
		}
	}
}
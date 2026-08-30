using System;
using FontStashSharp.Interfaces;

namespace FontStashSharp
{
	/// <summary>
	/// Provides default settings for FontSystem instances.
	/// These values are used when creating new FontSystemSettings without explicit configuration.
	/// </summary>
	public static class FontSystemDefaults
	{
		private static int _textureWidth = 1024, _textureHeight = 1024;
		private static float _fontResolutionFactor = 1.0f;
		private static int _kernelWidth = 0, _kernelHeight = 0;
		private static int _shapedTextCacheSize = 100;

		/// <summary>
		/// Gets or sets the default width of textures used to store glyph atlases.
		/// </summary>
		public static int TextureWidth
		{
			get => _textureWidth;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));

				}

				_textureWidth = value;
			}
		}

		/// <summary>
		/// Gets or sets the default height of textures used to store glyph atlases.
		/// </summary>
		public static int TextureHeight
		{
			get => _textureHeight;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));

				}

				_textureHeight = value;
			}
		}

		/// <summary>
		/// Determines how to produce final image(RGBA) from the rasterizer 8-bit source value
		/// </summary>
		public static GlyphRenderResult GlyphRenderResult { get; set; } = GlyphRenderResult.Premultiplied;

		/// <summary>
		/// Gets or sets whether alpha should be premultiplied.
		/// </summary>
		[Obsolete("Use GlyphRenderResult instead")]
		public static bool PremultiplyAlpha
		{
			get => GlyphRenderResult == GlyphRenderResult.Premultiplied;

			set
			{
				if (value)
				{
					GlyphRenderResult = GlyphRenderResult.Premultiplied;
				}
				else
				{
					GlyphRenderResult = GlyphRenderResult.NonPremultiplied;
				}
			}
		}

		/// <summary>
		/// Gets or sets the default font resolution factor for scaling glyphs.
		/// </summary>
		public static float FontResolutionFactor
		{
			get => _fontResolutionFactor;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "This cannot be smaller than 0");
				}

				_fontResolutionFactor = value;
			}
		}

		/// <summary>
		/// Gets or sets the default kernel width for glyph effects.
		/// </summary>
		public static int KernelWidth
		{
			get => _kernelWidth;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "This cannot be smaller than 0");
				}

				_kernelWidth = value;
			}
		}

		/// <summary>
		/// Gets or sets the default kernel height for glyph effects.
		/// </summary>
		public static int KernelHeight
		{
			get => _kernelHeight;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "This cannot be smaller than 0");
				}

				_kernelHeight = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use the old StbTrueType rasterizer by default.
		/// </summary>
		public static bool StbTrueTypeUseOldRasterizer { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use EM to pixels scale conversion by default.
		/// </summary>
		public static bool UseEmToPixelsScale { get; set; }

		/// <summary>
		/// Gets or sets the default text shaper for complex text layout.
		/// </summary>
		public static ITextShaper TextShaper { get; set; }

		/// <summary>
		/// Gets or sets the default font loader.
		/// If set to null, the default rasterizer (StbTrueTypeSharp) is used.
		/// </summary>
		public static IFontLoader FontLoader { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use kerning by default.
		/// </summary>
		public static bool UseKernings { get; set; } = true;

		/// <summary>
		/// Gets or sets the default character used as a fallback for missing glyphs.
		/// </summary>
		public static int? DefaultCharacter { get; set; } = ' ';

		/// <summary>
		/// Gets or sets the default line height for text style decorations (underline, strikethrough).
		/// </summary>
		public static int TextStyleLineHeight { get; set; } = 2;

		/// <summary>
		/// Maximum number of entries in the shaped text cache (for HarfBuzz text shaping)
		/// Higher values use more memory but reduce shaping overhead for repeated text
		/// Default: 100
		/// </summary>
		public static int ShapedTextCacheSize
		{
			get => _shapedTextCacheSize;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, "Cache size must be at least 1");
				}

				_shapedTextCacheSize = value;
			}
		}

#if MONOGAME || FNA
		/// <summary>
		/// Gets or sets the mode used to rasterize glyph bitmaps.
		/// </summary>
		public static FontRasterizationMode FontRasterizationMode { get; set; }
#endif
	}
}
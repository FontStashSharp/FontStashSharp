using System;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using Texture2D = System.Object;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Specifies the visual effect to apply when rendering glyphs.
	/// </summary>
	public enum FontSystemEffect
	{
		/// <summary>
		/// No effect applied to the glyph rendering.
		/// </summary>
		None,
		/// <summary>
		/// A blur effect is applied to soften the glyph edges.
		/// </summary>
		Blurry,
		/// <summary>
		/// A stroke/outline effect is applied to the glyph.
		/// </summary>
		Stroked
	}

	/// <summary>
	/// Determines how to produce final image(RGBA) from the rasterizer 8-bit source value
	/// </summary>
	public enum GlyphRenderResult
	{
		/// <summary>
		/// RGBA set to the source value. Default option
		/// </summary>
		Premultiplied,

		/// <summary>
		/// RGB set to 255 and A set to the source value
		/// </summary>
		NonPremultiplied,

		/// <summary>
		/// RGBA set to 255 if the source value is non-zero. Otherwise RGBA set to 0
		/// </summary>
		NoAntialiasing
	}

	/// <summary>
	/// Configuration settings for a FontSystem instance.
	/// </summary>
	public class FontSystemSettings
	{
		private int _textureWidth = 1024, _textureHeight = 1024;
		private float _fontResolutionFactor = 1.0f;
		private int _kernelWidth = 0, _kernelHeight = 0;
		private int _shapedTextCacheSize = 100;

		/// <summary>
		/// Gets or sets the width of the texture used to store glyph atlases.
		/// </summary>
		public int TextureWidth
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
		/// Gets or sets the height of the texture used to store glyph atlases.
		/// </summary>
		public int TextureHeight
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
		public GlyphRenderResult GlyphRenderResult { get; set; } = GlyphRenderResult.Premultiplied;

		/// <summary>
		/// Gets or sets whether alpha should be premultiplied.
		/// </summary>
		[Obsolete("Use GlyphRenderResult instead")]
		public bool PremultiplyAlpha
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
		/// Gets or sets the custom glyph renderer to use for rendering glyphs.
		/// </summary>
		public GlyphRenderer GlyphRenderer { get; set; } = GlyphRenderers.Default;

		/// <summary>
		/// Gets or sets the font resolution factor for scaling glyphs.
		/// A value greater than 1.0 renders glyphs at higher resolution for better quality.
		/// </summary>
		public float FontResolutionFactor
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
		/// Gets or sets the kernel width for glyph effects (blur or stroke).
		/// </summary>
		public int KernelWidth
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
		/// Gets or sets the kernel height for glyph effects (blur or stroke).
		/// </summary>
		public int KernelHeight
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
		/// Gets or sets a value indicating whether to use the old StbTrueType rasterizer implementation.
		/// </summary>
		public bool StbTrueTypeUseOldRasterizer { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use EM to pixels scale conversion.
		/// </summary>
		public bool UseEmToPixelsScale { get; set; }

		/// <summary>
		/// Enable HarfBuzz text shaping for complex scripts (Arabic, Indic, emoji sequences, etc.)
		/// When false, uses simple codepoint-to-glyph rendering
		/// Default: false
		/// </summary>
		public bool UseTextShaping => TextShaper != null;

		/// <summary>
		/// Gets or sets the text shaper for complex text layout (e.g., HarfBuzz).
		/// </summary>
		public ITextShaper TextShaper { get; set; }

		/// <summary>
		/// Gets or sets an existing texture for storing glyphs.
		/// When set, TextureWidth and TextureHeight are ignored.
		/// </summary>
		public Texture2D ExistingTexture { get; set; }

		/// <summary>
		/// Gets or sets the rectangle defining the used space in the ExistingTexture.
		/// </summary>
		public Rectangle ExistingTextureUsedSpace { get; set; }

		/// <summary>
		/// Gets or sets the font loader for rasterizing fonts.
		/// If null, the default rasterizer (StbTrueTypeSharp) is used.
		/// </summary>
		public IFontLoader FontLoader { get; set; }

		/// <summary>
		/// Maximum number of entries in the shaped text cache (for HarfBuzz text shaping)
		/// Higher values use more memory but reduce shaping overhead for repeated text
		/// Default: 100
		/// </summary>
		public int ShapedTextCacheSize
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

		public FontRasterizationMode FontRasterizationMode { get; set; }

		/// <summary>
		/// Initializes a new instance of the FontSystemSettings class with default values.
		/// </summary>
		public FontSystemSettings()
		{
			TextureWidth = FontSystemDefaults.TextureWidth;
			TextureHeight = FontSystemDefaults.TextureHeight;
			GlyphRenderResult = FontSystemDefaults.GlyphRenderResult;
			FontResolutionFactor = FontSystemDefaults.FontResolutionFactor;
			KernelWidth = FontSystemDefaults.KernelWidth;
			KernelHeight = FontSystemDefaults.KernelHeight;
			StbTrueTypeUseOldRasterizer = FontSystemDefaults.StbTrueTypeUseOldRasterizer;
			UseEmToPixelsScale = FontSystemDefaults.UseEmToPixelsScale;
			TextShaper = FontSystemDefaults.TextShaper;
			FontLoader = FontSystemDefaults.FontLoader;
			ShapedTextCacheSize = FontSystemDefaults.ShapedTextCacheSize;
		}

		/// <summary>
		/// Creates a deep copy of these settings.
		/// </summary>
		/// <returns>A new FontSystemSettings instance with the same configuration.</returns>
		public FontSystemSettings Clone()
		{
			return new FontSystemSettings
			{
				TextureWidth = TextureWidth,
				TextureHeight = TextureHeight,
				GlyphRenderResult = GlyphRenderResult,
				GlyphRenderer = GlyphRenderer,
				FontResolutionFactor = FontResolutionFactor,
				KernelWidth = KernelWidth,
				KernelHeight = KernelHeight,
				StbTrueTypeUseOldRasterizer = StbTrueTypeUseOldRasterizer,
				UseEmToPixelsScale = UseEmToPixelsScale,
				ExistingTexture = ExistingTexture,
				ExistingTextureUsedSpace = ExistingTextureUsedSpace,
				FontLoader = FontLoader,
				TextShaper = TextShaper,
				ShapedTextCacheSize = ShapedTextCacheSize,
				FontRasterizationMode = FontRasterizationMode
			};
		}
	}
}
using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using FontStashSharp.Rasterizers.StbTrueTypeSharp;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using Texture2D = System.Object;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Manages font loading, rendering, and glyph atlas management for sprite-based font rendering.
	/// This class handles dynamic glyph caching, text shaping, and texture atlas allocation.
	/// </summary>
	public class FontSystem : ITextShapingInfoProvider, IDisposable
	{
		/// <summary>
		/// Padding in pixels around glyphs in the texture atlas.
		/// </summary>
		public const int GlyphPad = 2;

		private readonly List<IFontSource> _fontSources = new List<IFontSource>();
		private readonly Int32Map<DynamicSpriteFont> _fonts = new Int32Map<DynamicSpriteFont>();
		private readonly FontSystemSettings _settings;

		private FontAtlas _currentAtlas;

		private readonly List<int> _textShaperFonts = new List<int>();

		/// <summary>
		/// Gets the width of texture atlases in pixels.
		/// </summary>
		public int TextureWidth => _settings.TextureWidth;

		/// <summary>
		/// Gets the height of texture atlases in pixels.
		/// </summary>
		public int TextureHeight => _settings.TextureHeight;

		/// <summary>
		/// Gets the glyph render result format.
		/// </summary>
		public GlyphRenderResult GlyphRenderResult => _settings.GlyphRenderResult;

		/// <summary>
		/// Gets a value indicating whether alpha premultiplication is enabled. Obsolete.
		/// </summary>
		[Obsolete]
		public bool PremultiplyAlpha => _settings.PremultiplyAlpha;

		/// <summary>
		/// Gets the glyph renderer used for rendering glyphs to the atlas.
		/// </summary>
		public GlyphRenderer GlyphRenderer => _settings.GlyphRenderer;

		/// <summary>
		/// Gets the font resolution scaling factor.
		/// </summary>
		public float FontResolutionFactor => _settings.FontResolutionFactor;

		/// <summary>
		/// Gets the kernel width for glyph rendering.
		/// </summary>
		public int KernelWidth => _settings.KernelWidth;

		/// <summary>
		/// Gets the kernel height for glyph rendering.
		/// </summary>
		public int KernelHeight => _settings.KernelHeight;

		/// <summary>
		/// Gets the existing texture to use for the first atlas, if any.
		/// </summary>
		public Texture2D ExistingTexture => _settings.ExistingTexture;

		/// <summary>
		/// Gets the already-used rectangle in the existing texture, if any.
		/// </summary>
		public Rectangle ExistingTextureUsedSpace => _settings.ExistingTextureUsedSpace;

		/// <summary>
		/// Gets or sets a value indicating whether kerning is applied when rendering text.
		/// </summary>
		public bool UseKernings { get; set; } = true;

		/// <summary>
		/// Gets or sets the codepoint to render for missing glyphs, if any.
		/// </summary>
		public int? DefaultCharacter { get; set; } = ' ';

		/// <summary>
		/// Gets a value indicating whether text shaping (HarfBuzz) is enabled.
		/// </summary>
		public bool UseTextShaping => _settings.UseTextShaping;

		/// <summary>
		/// Gets the maximum size of the shaped text cache.
		/// </summary>
		public int ShapedTextCacheSize => _settings.ShapedTextCacheSize;

		public FontRasterizationMode FontRasterizationMode => _settings.FontRasterizationMode;

		/// <summary>
		/// Gets the list of font sources loaded in this system.
		/// </summary>
		internal List<IFontSource> FontSources => _fontSources;

		/// <summary>
		/// Gets the list of all texture atlases managed by this system.
		/// </summary>
		public List<FontAtlas> Atlases { get; } = new List<FontAtlas>();

		/// <summary>
		/// Gets the current active texture atlas.
		/// </summary>
		public FontAtlas CurrentAtlas => _currentAtlas;

		/// <summary>
		/// Raised when the current texture atlas is full and cannot accommodate more glyphs.
		/// </summary>
		public event EventHandler CurrentAtlasFull;
		private readonly IFontLoader _fontLoader;

		/// <summary>
		/// Initializes a new instance of the <see cref="FontSystem"/> class with the specified settings.
		/// </summary>
		/// <param name="settings">The font system settings. Cannot be null.</param>
		public FontSystem(FontSystemSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_settings = settings.Clone();

			if (_settings.FontLoader == null)
			{
				var loaderSettings = new StbTrueTypeSharpSettings
				{
					KernelWidth = _settings.KernelWidth,
					KernelHeight = _settings.KernelHeight,
					UseOldRasterizer = _settings.StbTrueTypeUseOldRasterizer,
					UseEmToPixelsScale = _settings.UseEmToPixelsScale
				};
				_fontLoader = new StbTrueTypeSharpLoader(loaderSettings);
			}
			else
			{
				_fontLoader = _settings.FontLoader;
			}

			UseKernings = FontSystemDefaults.UseKernings;
			DefaultCharacter = FontSystemDefaults.DefaultCharacter;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FontSystem"/> class with default settings.
		/// </summary>
		public FontSystem() : this(new FontSystemSettings())
		{
		}

		/// <summary>
		/// Releases all resources used by the font system, including font sources and texture atlases.
		/// </summary>
		public void Dispose()
		{
			if (_fontSources != null)
			{
				foreach (var font in _fontSources)
					font.Dispose();
				_fontSources.Clear();
			}

			if (Atlases != null)
			{
				foreach (var atlas in Atlases)
					if (atlas.Texture is IDisposable dispTexture)
						dispTexture.Dispose();

				Atlases.Clear();
			}

			if (_settings.TextShaper != null)
			{
				foreach (var hbFont in _textShaperFonts)
				{
					_settings.TextShaper.RemoveFont(hbFont);
				}

				_textShaperFonts.Clear();
			}

			SetFontAtlas(null);
			_fonts.Clear();
		}

		/// <summary>
		/// Adds a font from a byte array.
		/// </summary>
		/// <param name="data">The font file data as a byte array.</param>
		public void AddFont(byte[] data)
		{
			var fontSource = _fontLoader.Load(data);
			_fontSources.Add(fontSource);

			if (_settings.UseTextShaping)
			{
				// Create HarfBuzz font
				_textShaperFonts.Add(_settings.TextShaper.RegisterTtfFont(data));

				foreach (var kvp in _fonts)
				{
					kvp.Value.ClearShapedTextCache();
				}
			}
		}

		/// <summary>
		/// Adds a font from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the font file data.</param>
		public void AddFont(Stream stream)
		{
			AddFont(stream.ToByteArray());
		}

		/// <summary>
		/// Gets or creates a dynamic sprite font with the specified size.
		/// </summary>
		/// <param name="fontSize">The font size in points.</param>
		/// <returns>A <see cref="DynamicSpriteFont"/> for the specified size.</returns>
		public DynamicSpriteFont GetFont(float fontSize)
		{
			var intSize = fontSize.FloatAsInt();
			DynamicSpriteFont result;
			if (_fonts.TryGetValue(intSize, out result))
			{
				return result;
			}

			if (_fontSources.Count == 0)
			{
				throw new Exception("Could not create a font without a single font source. Use AddFont to add at least one font source.");
			}

			var fontSource = _fontSources[0];

			int ascent, descent, lineHeight;
			fontSource.GetMetricsForSize(fontSize, out ascent, out descent, out lineHeight);

			result = new DynamicSpriteFont(this, fontSize, lineHeight);
			_fonts[intSize] = result;
			return result;
		}

		/// <summary>
		/// Sets the current active texture atlas.
		/// </summary>
		/// <param name="fontAtlas">The font atlas to set as active, or null to clear the current atlas.</param>
		public void SetFontAtlas(FontAtlas fontAtlas)
		{
			if (fontAtlas != null && !Atlases.Contains(fontAtlas))
			{
				Atlases.Add(fontAtlas);
			}
			_currentAtlas = fontAtlas;
		}

		/// <summary>
		/// Clears all loaded fonts and texture atlases.
		/// </summary>
		public void Reset()
		{
			Atlases.Clear();
			_fonts.Clear();
			SetFontAtlas(null);
		}

		internal int? GetCodepointIndex(int codepoint, out int fontSourceIndex)
		{
			fontSourceIndex = 0;
			var g = default(int?);

			for (var i = 0; i < _fontSources.Count; ++i)
			{
				var f = _fontSources[i];
				g = f.GetGlyphId(codepoint);
				if (g != null)
				{
					fontSourceIndex = i;
					break;
				}
			}
			return g;
		}

		/// <summary>
		/// Shape text using HarfBuzz
		/// </summary>
		internal ShapedText ShapeText(string text, float fontSize)
		{
			if (!_settings.UseTextShaping)
			{
				throw new InvalidOperationException("Text shaping is not enabled. Set UseTextShaping = true in FontSystemSettings.");
			}

			return _settings.TextShaper.Shape(text, fontSize, this);
		}

#if MONOGAME || FNA || XNA || STRIDE
		private FontAtlas CreateFontAtlas(GraphicsDevice device, int textureWidth, int textureHeight)
#else
		private FontAtlas CreateFontAtlas(ITexture2DManager device, int textureWidth, int textureHeight)
#endif
		{
			Texture2D existingTexture = null;
			if (ExistingTexture != null && Atlases.Count == 0)
			{
				existingTexture = ExistingTexture;
			}

			FontAtlas fontAtlas = new FontAtlas(textureWidth, textureHeight, 256, existingTexture);

			// If existing texture is used, mark existing used rect as used
			if (existingTexture != null && !ExistingTextureUsedSpace.IsEmpty)
			{
				if (!fontAtlas.AddSkylineLevel(0, ExistingTextureUsedSpace.X, ExistingTextureUsedSpace.Y, ExistingTextureUsedSpace.Width, ExistingTextureUsedSpace.Height))
				{
					throw new Exception(string.Format("Unable to specify existing texture used space: {0}", ExistingTextureUsedSpace));
				}

				// TODO: Clear remaining space
			}

			return fontAtlas;
		}

#if MONOGAME || FNA || XNA || STRIDE
		internal void RenderGlyphOnAtlas(GraphicsDevice device, DynamicFontGlyph glyph)
#else
		internal void RenderGlyphOnAtlas(ITexture2DManager device, DynamicFontGlyph glyph)
#endif
		{
			var textureSize = new Point(TextureWidth, TextureHeight);

			if (ExistingTexture != null)
			{
#if MONOGAME || FNA || XNA || STRIDE
				textureSize = new Point(ExistingTexture.Width, ExistingTexture.Height);
#else
				textureSize = device.GetTextureSize(ExistingTexture);
#endif
			}

			int gx = 0, gy = 0;
			var gw = glyph.Size.X + GlyphPad * 2;
			var gh = glyph.Size.Y + GlyphPad * 2;

			// If CurrentAtlas is null create a new one
			if (CurrentAtlas == null)
			{
				SetFontAtlas(CreateFontAtlas(device, textureSize.X, textureSize.Y));
			}
			var atlas = CurrentAtlas;
			if (!atlas.AddRect(gw, gh, ref gx, ref gy))
			{
				CurrentAtlasFull?.Invoke(this, EventArgs.Empty);

				// Create a new atlas if it was not set during the CurrentAtlasFull event
				if (CurrentAtlas == atlas)
				{
					SetFontAtlas(CreateFontAtlas(device, textureSize.X, textureSize.Y));
				}
				atlas = CurrentAtlas;

				// Try to add again
				if (!atlas.AddRect(gw, gh, ref gx, ref gy))
				{
					throw new Exception(string.Format("Could not add rect to the newly created atlas. gw={0}, gh={1}", gw, gh));
				}
			}

			glyph.TextureOffset.X = gx + GlyphPad;
			glyph.TextureOffset.Y = gy + GlyphPad;

			atlas.RenderGlyph(device, glyph, FontSources[glyph.FontSourceIndex], GlyphRenderer, GlyphRenderResult, KernelWidth, KernelHeight, FontRasterizationMode);

			glyph.Texture = atlas.Texture;
		}

		int? ITextShapingInfoProvider.GetFontSourceId(int codepoint)
		{
			var glyphId = GetCodepointIndex(codepoint, out int fontSourceIndex);

			return glyphId != null ? fontSourceIndex : (int?)null;
		}

		int ITextShapingInfoProvider.GetTextShaperFontId(int fontSourceId) => _textShaperFonts[fontSourceId];

		float ITextShapingInfoProvider.CalculateScale(int fontSourceId, float fontSize) => _fontSources[fontSourceId].CalculateScaleForTextShaper(fontSize);
	}
}
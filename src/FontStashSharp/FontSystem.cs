using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

#if MONOGAME || FNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
#endif

namespace FontStashSharp
{
	public class FontSystem : IDisposable
	{
		private readonly List<IFontSource> _fontSources = new List<IFontSource>();
		private readonly Int32Map<DynamicSpriteFont> _fonts = new Int32Map<DynamicSpriteFont>();
		private readonly FontSystemSettings _settings;

		private readonly IFontLoader _fontLoader;

#if MONOGAME || FNA || STRIDE
		private readonly GraphicsDevice _graphicsDevice;
#else
		private readonly ITexture2DManager _textureCreator;
#endif

		private FontAtlas _currentAtlas;

		public FontSystemEffect Effect => _settings.Effect;
		public int EffectAmount => _settings.EffectAmount;

		public int TextureWidth => _settings.TextureWidth;
		public int TextureHeight => _settings.TextureHeight;

		public bool PremultiplyAlpha => _settings.PremultiplyAlpha;

		public float FontResolutionFactor => _settings.FontResolutionFactor;

		public int KernelWidth => _settings.KernelWidth;
		public int KernelHeight => _settings.KernelHeight;

		public bool UseKernings = true;
		public int? DefaultCharacter = ' ';

		public int CharacterSpacing = 0;
		public int LineSpacing = 0;

		internal int BlurAmount => Effect == FontSystemEffect.Blurry ? EffectAmount : 0;
		internal int StrokeAmount => Effect == FontSystemEffect.Stroked ? EffectAmount : 0;

		public FontAtlas CurrentAtlas
		{
			get
			{
				if (_currentAtlas == null)
				{
					_currentAtlas = new FontAtlas(TextureWidth, TextureHeight, 256);
					Atlases.Add(_currentAtlas);
				}

				return _currentAtlas;
			}
		}

		public List<FontAtlas> Atlases { get; } = new List<FontAtlas>();

		public event EventHandler CurrentAtlasFull;

#if MONOGAME || FNA || STRIDE
		public FontSystem(IFontLoader fontLoader, GraphicsDevice graphicsDevice, FontSystemSettings settings)
#else
		public FontSystem(IFontLoader fontLoader, ITexture2DManager textureCreator, FontSystemSettings settings)
#endif
		{
			if (fontLoader == null)
			{
				throw new ArgumentNullException(nameof(fontLoader));
			}

#if MONOGAME || FNA || STRIDE
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException(nameof(graphicsDevice));
			}

			_graphicsDevice = graphicsDevice;
#else
			if (textureCreator == null)
			{
				throw new ArgumentNullException(nameof(textureCreator));
			}

			_textureCreator = textureCreator;
#endif

			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_fontLoader = fontLoader;
			_settings = settings.Clone();
		}

#if MONOGAME || FNA || STRIDE
		public FontSystem(GraphicsDevice graphicsDevice, FontSystemSettings settings) : this(StbTrueTypeSharpFontLoader.Instance, graphicsDevice, settings)
		{
		}
#else
		public FontSystem(ITexture2DManager textureCreator, FontSystemSettings settings): this(StbTrueTypeSharpFontLoader.Instance, textureCreator, settings)
		{
		}
#endif

#if MONOGAME || FNA || STRIDE
		public FontSystem(GraphicsDevice graphicsDevice) : this(graphicsDevice, new FontSystemSettings())
		{
		}
#else
		public FontSystem(ITexture2DManager textureCreator): this(textureCreator, new FontSystemSettings())
		{
		}
#endif

		public void Dispose()
		{
			if (_fontSources != null)
			{
				foreach (var font in _fontSources)
					font.Dispose();
				_fontSources.Clear();
			}

			Atlases?.Clear();
			_currentAtlas = null;
			_fonts.Clear();
		}

		public void AddFont(byte[] data)
		{
			var fontSource = _fontLoader.Load(data);
			_fontSources.Add(fontSource);
		}

		public void AddFont(Stream stream)
		{
			AddFont(stream.ToByteArray());
		}

		public DynamicSpriteFont GetFont(int fontSize)
		{
			DynamicSpriteFont result;
			if (_fonts.TryGetValue(fontSize, out result))
			{
				return result;
			}

			result = new DynamicSpriteFont(this, fontSize);
			_fonts[fontSize] = result;
			return result;
		}

		public void Reset()
		{
			Atlases.Clear();
			_fonts.Clear();
		}

		internal int? GetCodepointIndex(int codepoint, out IFontSource font)
		{
			font = null;

			var g = default(int?);
			foreach (var f in _fontSources)
			{
				g = f.GetGlyphId(codepoint);
				if (g != null)
				{
					font = f;
					break;
				}
			}

			return g;
		}

		internal void RenderGlyphOnAtlas(DynamicFontGlyph glyph)
		{
			var currentAtlas = CurrentAtlas;
			int gx = 0, gy = 0;
			var gw = glyph.Bounds.Width;
			var gh = glyph.Bounds.Height;
			if (!currentAtlas.AddRect(gw, gh, ref gx, ref gy))
			{
				CurrentAtlasFull?.Invoke(this, EventArgs.Empty);

				// This code will force creation of new atlas
				_currentAtlas = null;
				currentAtlas = CurrentAtlas;

				// Try to add again
				if (!currentAtlas.AddRect(gw, gh, ref gx, ref gy))
				{
					throw new Exception(string.Format("Could not add rect to the newly created atlas. gw={0}, gh={1}", gw, gh));
				}
			}

			glyph.Bounds.X = gx;
			glyph.Bounds.Y = gy;

#if MONOGAME || FNA || STRIDE
			currentAtlas.RenderGlyph(_graphicsDevice, glyph, BlurAmount, StrokeAmount, PremultiplyAlpha, KernelWidth, KernelHeight);
#else
			currentAtlas.RenderGlyph(_textureCreator, glyph, BlurAmount, StrokeAmount, PremultiplyAlpha, KernelWidth, KernelHeight);
#endif

			glyph.Texture = currentAtlas.Texture;
		}
	}
}

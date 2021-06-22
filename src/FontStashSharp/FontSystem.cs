using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
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

		private readonly IFontLoader _fontLoader;

#if MONOGAME || FNA || STRIDE
		private readonly GraphicsDevice _graphicsDevice;
#else
		private readonly ITexture2DManager _textureCreator;
#endif

		private FontAtlas _currentAtlas;
		private Point _size;

		public readonly bool PremultiplyAlpha;

		public readonly int BlurAmount;
		public readonly int StrokeAmount;

		public bool UseKernings = true;
		public int? DefaultCharacter = ' ';

		public int CharacterSpacing = 0;
		public int LineSpacing = 0;

		public int KernelWidth { get; protected set; }
		public int KernelHeight { get; protected set; }

		public FontAtlas CurrentAtlas
		{
			get
			{
				if (_currentAtlas == null)
				{
					_currentAtlas = new FontAtlas(_size.X, _size.Y, 256);
					Atlases.Add(_currentAtlas);
				}

				return _currentAtlas;
			}
		}

		public List<FontAtlas> Atlases { get; } = new List<FontAtlas>();

		public float FontResolutionFactor { get; protected set; }

		public event EventHandler CurrentAtlasFull;

#if MONOGAME || FNA || STRIDE
		public FontSystem(IFontLoader fontLoader, GraphicsDevice graphicsDevice, int width = 1024, int height = 1024, int blurAmount = 0, int strokeAmount = 0, bool premultiplyAlpha = true, float fontResolutionFactor = 1f, int kernelWidth = 0, int kernelHeight = 0)
#else
		public FontSystem(IFontLoader fontLoader, ITexture2DManager textureCreator, int width = 1024, int height = 1024, int blurAmount = 0, int strokeAmount = 0, bool premultiplyAlpha = true, float fontResolutionFactor = 1f, int kernelWidth = 0, int kernelHeight = 0)
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

			_fontLoader = fontLoader;

			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}

			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}

			if (blurAmount < 0 || blurAmount > 20)
			{
				throw new ArgumentOutOfRangeException(nameof(blurAmount));
			}

			if (strokeAmount < 0 || strokeAmount > 20)
			{
				throw new ArgumentOutOfRangeException(nameof(strokeAmount));
			}

			if (strokeAmount != 0 && blurAmount != 0)
			{
				throw new ArgumentException("Cannot have both blur and stroke.");
			}

			if(fontResolutionFactor < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(fontResolutionFactor), fontResolutionFactor, "This cannot be smaller than 0");
			}

			if (kernelWidth < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(kernelWidth), kernelWidth, "This cannot be smaller than 0");
			}

			if (kernelHeight < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(kernelHeight), kernelHeight, "This cannot be smaller than 0");
			}

			BlurAmount = blurAmount;
			StrokeAmount = strokeAmount;
			PremultiplyAlpha = premultiplyAlpha;
			FontResolutionFactor = fontResolutionFactor;
			KernelWidth = kernelWidth;
			KernelHeight = kernelHeight;

			_size = new Point(width, height);
		}

#if MONOGAME || FNA || STRIDE
		public FontSystem(GraphicsDevice graphicsDevice, int width, int height, int blurAmount = 0, int strokeAmount = 0, bool premultiplyAlpha = true) :
			this(StbTrueTypeSharpFontLoader.Instance, graphicsDevice, width, height, blurAmount, strokeAmount, premultiplyAlpha)
		{
		}
#else
		public FontSystem(ITexture2DManager textureCreator, int width, int height, int blurAmount = 0, int strokeAmount = 0, bool premultiplyAlpha = true):
			this(StbTrueTypeSharpFontLoader.Instance, textureCreator, width, height, blurAmount, strokeAmount, premultiplyAlpha)
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

		public void Reset(int width, int height)
		{
			Atlases.Clear();
			_fonts.Clear();

			if (width == _size.X && height == _size.Y)
				return;

			_size = new Point(width, height);
		}

		public void Reset()
		{
			Reset(_size.X, _size.Y);
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

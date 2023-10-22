using FontStashSharp.Interfaces;
using System;
using System.Collections.Generic;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Graphics;
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using Texture2D = System.Object;
using System.Drawing;
#endif

namespace FontStashSharp
{
	public class FontAtlasProvider
	{
		private readonly FontSystemSettings _settings;
		private FontAtlas _currentAtlas;
		public List<FontAtlas> Atlases { get; } = new List<FontAtlas>();

		public int TextureWidth => _settings.TextureWidth;
		public int TextureHeight => _settings.TextureHeight;
		public Texture2D ExistingTexture => _settings.ExistingTexture;
		public Rectangle ExistingTextureUsedSpace => _settings.ExistingTextureUsedSpace;

		public event EventHandler CurrentAtlasFull;

		public FontAtlasProvider(FontSystemSettings settings)
		{
			if (settings == null)
			{
			  throw new ArgumentNullException(nameof(settings));
			}

			_settings = settings.Clone();
		}

		public void Clear()
		{
			_currentAtlas = null;
			Atlases.Clear();
		}

#if MONOGAME || FNA || STRIDE
		public FontAtlas CreateNewAtlas(GraphicsDevice device)
#else
		public FontAtlas CreateNewAtlas(ITexture2DManager device)
#endif
		{
			var textureSize = new Point(TextureWidth, TextureHeight);
			Texture2D existingTexture = null;

			if (ExistingTexture != null && Atlases.Count == 0)
			{
#if MONOGAME || FNA || STRIDE
				textureSize = new Point(ExistingTexture.Width, ExistingTexture.Height);
#else
				textureSize = device.GetTextureSize(ExistingTexture);
#endif
			    existingTexture = ExistingTexture;
			}

			_currentAtlas = new FontAtlas(textureSize.X, textureSize.Y, 256, existingTexture);
			
			// If existing texture is used, mark existing used rect as used
			if (existingTexture != null && !ExistingTextureUsedSpace.IsEmpty)
			{
				if (!_currentAtlas.AddSkylineLevel(0, ExistingTextureUsedSpace.X, ExistingTextureUsedSpace.Y, ExistingTextureUsedSpace.Width, ExistingTextureUsedSpace.Height))
				{
					throw new Exception(string.Format("Unable to specify existing texture used space: {0}", ExistingTextureUsedSpace));
				}

				// TODO: Clear remaining space
			}
			Atlases.Add(_currentAtlas);
			return _currentAtlas;
		}

#if MONOGAME || FNA || STRIDE
		public FontAtlas GetCurrentAtlas(GraphicsDevice device)
#else
		public FontAtlas GetCurrentAtlas(ITexture2DManager device)
#endif
		{
			if (_currentAtlas == null)
			{
				CreateNewAtlas(device);
			}
			return _currentAtlas;
		}
	}
}

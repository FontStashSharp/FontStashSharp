using Cyotek.Drawing.BitmapFont;
using FontStashSharp.Interfaces;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using Texture2D = System.Object;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// Represents a texture with an offset for use in static sprite fonts.
	/// </summary>
	public class TextureWithOffset
	{
		/// <summary>
		/// Gets or sets the texture.
		/// </summary>
		public Texture2D Texture { get; set; }

		/// <summary>
		/// Gets or sets the offset of the texture in pixels.
		/// </summary>
		public Point Offset { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TextureWithOffset"/> class.
		/// </summary>
		/// <param name="texture">The texture. Cannot be null.</param>
		public TextureWithOffset(Texture2D texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

			Texture = texture;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextureWithOffset"/> class with a texture and offset.
		/// </summary>
		/// <param name="texture">The texture. Cannot be null.</param>
		/// <param name="offset">The offset in pixels.</param>
		public TextureWithOffset(Texture2D texture, Point offset) : this(texture)
		{
			Offset = offset;
		}
	}

	/// <summary>
	/// A sprite font with static, pre-rendered glyphs loaded from bitmap font files.
	/// </summary>
	public partial class StaticSpriteFont : SpriteFontBase
	{
		private readonly Int32Map<int> _kernings = new Int32Map<int>();

		/// <summary>
		/// Gets the glyphs in this font, indexed by codepoint.
		/// </summary>
		public Int32Map<FontGlyph> Glyphs { get; } = new Int32Map<FontGlyph>();

		/// <summary>
		/// Gets or sets the codepoint to render for missing glyphs, if any.
		/// </summary>
		public int? DefaultCharacter { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether kerning is applied when rendering text.
		/// </summary>
		public bool UseKernings { get; set; } = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="StaticSpriteFont"/> class.
		/// </summary>
		/// <param name="fontSize">The font size in pixels.</param>
		/// <param name="lineHeight">The line height in pixels.</param>
		public StaticSpriteFont(int fontSize, int lineHeight) : base(fontSize, lineHeight)
		{
		}

		private FontGlyph InternalGetGlyph(int codepoint)
		{
			FontGlyph result;
			Glyphs.TryGetValue(codepoint, out result);

			return result;
		}

#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Gets a glyph for the specified codepoint with optional effects applied.
		/// </summary>
		/// <param name="device">The graphics device</param>
		/// <param name="codepoint">The Unicode codepoint for the character</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The font glyph for the specified codepoint</returns>
		protected internal override FontGlyph GetGlyph(GraphicsDevice device, int codepoint, FontSystemEffect effect, int effectAmount)
#else
		/// <summary>
		/// Gets a glyph for the specified codepoint with optional effects applied.
		/// </summary>
		/// <param name="device">The texture manager</param>
		/// <param name="codepoint">The Unicode codepoint for the character</param>
		/// <param name="effect">The font system effect to apply</param>
		/// <param name="effectAmount">The amount of the effect to apply</param>
		/// <returns>The font glyph for the specified codepoint</returns>
		protected internal override FontGlyph GetGlyph(ITexture2DManager device, int codepoint, FontSystemEffect effect, int effectAmount)
#endif
		{
			var result = InternalGetGlyph(codepoint);
			if (result == null && DefaultCharacter != null)
			{
				result = InternalGetGlyph(DefaultCharacter.Value);
			}
			return result;
		}

		internal override void PreDraw(TextSource source, FontSystemEffect effect, int effectAmount, out int ascent, out int lineHeight)
		{
			ascent = 0;
			lineHeight = LineHeight;
		}

		private static int KerningKey(int codepoint1, int codepoint2)
		{
			return ((codepoint1 << 16) | (codepoint1 >> 16)) ^ codepoint2;
		}

		/// <summary>
		/// Gets the kerning advance value between two glyphs.
		/// </summary>
		/// <param name="codepoint1">The first character codepoint.</param>
		/// <param name="codepoint2">The second character codepoint.</param>
		/// <returns>The kerning advance value, or 0 if no kerning is defined.</returns>
		public int GetGlyphKernAdvance(int codepoint1, int codepoint2)
		{
			var key = KerningKey(codepoint1, codepoint2);
			int result = 0;
			_kernings.TryGetValue(key, out result);

			return result;
		}

		/// <summary>
		/// Sets the kerning advance value between two glyphs.
		/// </summary>
		/// <param name="codepoint1">The first character codepoint.</param>
		/// <param name="codepoint2">The second character codepoint.</param>
		/// <param name="value">The kerning advance value in pixels.</param>
		public void SetGlyphKernAdvance(int codepoint1, int codepoint2, int value)
		{
			var key = KerningKey(codepoint1, codepoint2);
			_kernings[key] = value;
		}

		internal override float GetKerning(FontGlyph glyph, FontGlyph prevGlyph)
		{
			if (!UseKernings)
			{
				return 0.0f;
			}

			return GetGlyphKernAdvance(prevGlyph.Codepoint, glyph.Codepoint);
		}

		private static BitmapFont LoadBMFont(string data)
		{
			var bmFont = new BitmapFont();
			if (data.StartsWith("<"))
			{
				// xml
				bmFont.LoadXml(data);
			}
			else if (data.StartsWith("info"))
			{
				// text
				bmFont.LoadText(data);
			}
			else
			{
				// binary (expects base64-encoded string)
				using (var stream = new MemoryStream(Convert.FromBase64String(data)))
				{
					bmFont.LoadBinary(stream);
				}
			}

			return bmFont;
		}

		private static StaticSpriteFont FromBMFont(BitmapFont bmFont, Func<string, TextureWithOffset> textureGetter)
		{
			var result = new StaticSpriteFont(bmFont.LineHeight, bmFont.LineHeight);

			var characters = bmFont.Characters.Values.OrderBy(c => c.Char);

			foreach (var ch in characters)
			{
				var texture = textureGetter(bmFont.Pages[ch.TexturePage].FileName);

				var glyph = new FontGlyph
				{
					Id = ch.Char,
					Codepoint = ch.Char,
					RenderOffset = new Point(ch.XOffset, ch.YOffset),
					TextureOffset = new Point(ch.X + texture.Offset.X, ch.Y + texture.Offset.Y),
					Size = new Point(ch.Width, ch.Height),
					XAdvance = ch.XAdvance,
					Texture = texture.Texture
				};

				result.Glyphs[glyph.Codepoint] = glyph;
			}

			foreach (var kern in bmFont.Kernings)
			{
				result.SetGlyphKernAdvance(kern.Key.FirstCharacter, kern.Key.SecondCharacter, kern.Value);
			}

			return result;
		}

		/// <summary>
		/// Creates a static sprite font from bitmap font data.
		/// </summary>
		/// <param name="data">The bitmap font data (XML, text, or base64-encoded binary format).</param>
		/// <param name="textureGetter">A function that provides textures for font pages.</param>
		/// <returns>A <see cref="StaticSpriteFont"/> loaded from the bitmap font data.</returns>
		public static StaticSpriteFont FromBMFont(string data, Func<string, TextureWithOffset> textureGetter)
		{
			var bmFont = LoadBMFont(data);
			return FromBMFont(bmFont, textureGetter);
		}

#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Creates a static sprite font from bitmap font data with texture loading from streams.
		/// </summary>
		/// <param name="data">The bitmap font data (XML, text, or base64-encoded binary format).</param>
		/// <param name="imageStreamOpener">A function that opens image streams for font pages.</param>
		/// <param name="device">The graphics device.</param>
		/// <returns>A <see cref="StaticSpriteFont"/> loaded from the bitmap font data.</returns>
		public static StaticSpriteFont FromBMFont(string data, Func<string, Stream> imageStreamOpener, GraphicsDevice device)
#else
		/// <summary>
		/// Creates a static sprite font from bitmap font data with texture loading from streams.
		/// </summary>
		/// <param name="data">The bitmap font data (XML, text, or base64-encoded binary format).</param>
		/// <param name="imageStreamOpener">A function that opens image streams for font pages.</param>
		/// <param name="textureManager">The texture manager.</param>
		/// <returns>A <see cref="StaticSpriteFont"/> loaded from the bitmap font data.</returns>
		public static StaticSpriteFont FromBMFont(string data, Func<string, Stream> imageStreamOpener, ITexture2DManager textureManager)
#endif
		{
			var bmFont = LoadBMFont(data);

			var textures = new Dictionary<string, Texture2D>();
			for (var i = 0; i < bmFont.Pages.Length; ++i)
			{
				var fileName = bmFont.Pages[i].FileName;
				Stream stream = null;
				try
				{
					stream = imageStreamOpener(fileName);
					if (!stream.CanSeek)
					{
						// If stream isn't seekable, use MemoryStream instead
						var ms = new MemoryStream();
						stream.CopyTo(ms);
						ms.Seek(0, SeekOrigin.Begin);
						stream.Dispose();
						stream = ms;
					}

					var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
					if (image.SourceComp == ColorComponents.Grey)
					{
						// If input image is single byte per pixel, then StbImageSharp will set alpha to 255 in the resulting 32-bit image
						// Such behavior isn't acceptable for us
						// So reset alpha to color value
						for (var j = 0; j < image.Data.Length / 4; ++j)
						{
							image.Data[j * 4 + 3] = image.Data[j * 4];
						}
					}

#if MONOGAME || FNA || KNI || XNA || STRIDE
					var texture = Texture2DManager.CreateTexture(device, image.Width, image.Height);
					Texture2DManager.SetTextureData(texture, new Rectangle(0, 0, image.Width, image.Height), image.Data);
#else
					var texture = textureManager.CreateTexture(image.Width, image.Height);
					textureManager.SetTextureData(texture, new Rectangle(0, 0, image.Width, image.Height), image.Data);
#endif

					textures[fileName] = texture;
				}
				finally
				{
					stream.Dispose();
				}
			}

			return FromBMFont(bmFont, fileName => new TextureWithOffset(textures[fileName]));
		}
	}
}

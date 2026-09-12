using FontStashSharp.Interfaces;
using System.Collections.Generic;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp
{
	/// <summary>
	/// A font that renders a base font at a different scale, typically used when a font resolution factor is applied.
	/// </summary>
	public class ScaledSpriteFont : SpriteFontBase
	{
		private readonly SpriteFontBase _baseFont;
		private readonly float _fontSize;
		private readonly int _lineHeight;

		/// <inheritdoc/>
		public override FontRasterizationMode FontRasterizationMode => _baseFont.FontRasterizationMode;

		/// <summary>
		/// Gets the base font that this font is scaled from.
		/// </summary>
		public SpriteFontBase BaseFont => _baseFont;

		/// <summary>
		/// Gets the scale factor applied to the base font.
		/// </summary>
		public float Scale { get; }

		/// <inheritdoc/>
		public override FontSystem FontSystem => _baseFont.FontSystem;

		/// <inheritdoc/>
		public override float FontSize => _fontSize;

		/// <inheritdoc/>
		public override int LineHeight => _lineHeight;

		/// <inheritdoc/>
		public override Point TextureSize => _baseFont.TextureSize;

		private float InverseScale { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaledSpriteFont"/> class.
		/// </summary>
		/// <param name="baseFont">The base font to scale.</param>
		/// <param name="scale">The scale factor to apply.</param>
		/// <param name="fontSize">The font size in points of the scaled font.</param>
		/// <param name="lineHeight">The line height in pixels of the scaled font.</param>
		public ScaledSpriteFont(SpriteFontBase baseFont, float scale, float fontSize, int lineHeight)
		{
			_baseFont = baseFont;
			Scale = scale;
			InverseScale = 1f / scale;
			_fontSize = fontSize;
			_lineHeight = lineHeight;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaledSpriteFont"/> class.
		/// </summary>
		/// <param name="baseFont">The base font to scale.</param>
		/// <param name="scale">The scale factor to apply.</param>
		/// <param name="fontSize">The font size in points of the scaled font.</param>
		/// <remarks>
		/// The line height is derived from the <paramref name="baseFont"/> by scaling its line height with the <paramref name="scale"/> factor.
		/// </remarks>
		public ScaledSpriteFont(SpriteFontBase baseFont, float scale, float fontSize) : this(baseFont, scale, fontSize, (int)(baseFont.LineHeight * scale))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaledSpriteFont"/> class.
		/// </summary>
		/// <param name="baseFont">The base font to scale.</param>
		/// <param name="scale">The scale factor to apply.</param>
		/// <remarks>
		/// This is the simplest way to create a scaled font. The font size is derived from the <paramref name="baseFont"/> by scaling its font size with the <paramref name="scale"/> factor, and, as with the constructor above, the line height is derived from the base font's line height scaled by the same factor.
		/// </remarks>
		public ScaledSpriteFont(SpriteFontBase baseFont, float scale) : this(baseFont, scale, baseFont.FontSize * scale)
		{
		}

		internal override float GetKerning(FontGlyph glyph, FontGlyph prevGlyph) => _baseFont.GetKerning(glyph, prevGlyph) * Scale;

		internal override float InternalDrawText(IFontStashRenderer renderer, TextColorSource source, Vector2 position, float rotation, Vector2 origin, Vector2 scale, float layerDepth, float characterSpacing, float lineSpacing, TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			origin *= InverseScale;
			characterSpacing *= InverseScale;
			lineSpacing *= InverseScale;

			return _baseFont.InternalDrawText(renderer, source, position, rotation, origin, scale * Scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		internal override float InternalDrawText2(IFontStashRenderer2 renderer, TextColorSource source, Vector2 position, float rotation, Vector2 origin, Vector2 scale, float layerDepth, float characterSpacing, float lineSpacing, TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			origin *= InverseScale;
			characterSpacing *= InverseScale;
			lineSpacing *= InverseScale;

			return _baseFont.InternalDrawText2(renderer, source, position, rotation, origin, scale * Scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		internal override void InternalGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2 scale, float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount, List<Glyph> result)
		{
			origin *= InverseScale;
			characterSpacing *= InverseScale;
			lineSpacing *= InverseScale;

			_baseFont.InternalGetGlyphs(source, position, origin, scale * Scale, characterSpacing, lineSpacing, effect, effectAmount, result);
		}

		internal override Bounds InternalTextBounds(TextSource source, Vector2 position, float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount)
		{
			characterSpacing *= InverseScale;
			lineSpacing *= InverseScale;

			var result = _baseFont.InternalTextBounds(source, position, characterSpacing, lineSpacing, effect, effectAmount);

			result.ApplyScale(new Vector2(Scale, Scale));

			return result;
		}
	}
}

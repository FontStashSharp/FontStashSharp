using FontStashSharp.Interfaces;
using System.Collections.Generic;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
#elif STRIDE
using Stride.Core.Mathematics;
#else
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

		private float InverseScale { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaledSpriteFont"/> class.
		/// </summary>
		/// <param name="baseFont">The base font to scale.</param>
		/// <param name="scale">The scale factor to apply.</param>
		public ScaledSpriteFont(SpriteFontBase baseFont, float scale) : base((int)(baseFont.LineHeight * scale))
		{
			_baseFont = baseFont;
			Scale = scale;
			InverseScale = 1f / scale;
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

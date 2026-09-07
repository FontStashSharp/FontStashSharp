using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FontStashSharp
{
	public class ScaledSpriteFont : SpriteFontBase
	{
		private readonly SpriteFontBase _baseFont;

		public override FontRasterizationMode FontRasterizationMode => _baseFont.FontRasterizationMode;

		public SpriteFontBase BaseFont => _baseFont;

		public float Scale { get; }

		public ScaledSpriteFont(SpriteFontBase baseFont, float scale) : base((int)(baseFont.LineHeight * scale))
		{
			_baseFont = baseFont;
			Scale = scale;
		}

		internal override float GetKerning(FontGlyph glyph, FontGlyph prevGlyph) => _baseFont.GetKerning(glyph, prevGlyph) * Scale;

		internal override float InternalDrawText(IFontStashRenderer renderer, TextColorSource source, Vector2 position, float rotation, Vector2 origin, Vector2 scale, float layerDepth, float characterSpacing, float lineSpacing, TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			return _baseFont.InternalDrawText(renderer, source, position, rotation, origin, scale * Scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		internal override float InternalDrawText2(IFontStashRenderer2 renderer, TextColorSource source, Vector2 position, float rotation, Vector2 origin, Vector2 scale, float layerDepth, float characterSpacing, float lineSpacing, TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			return _baseFont.InternalDrawText2(renderer, source, position, rotation, origin, scale * Scale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		internal override void InternalGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2 scale, float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount, List<Glyph> result)
		{
			_baseFont.InternalGetGlyphs(source, position, origin, scale * Scale, characterSpacing, lineSpacing, effect, effectAmount, result);
		}

		internal override Bounds InternalTextBounds(TextSource source, Vector2 position, float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount)
		{
			var result = _baseFont.InternalTextBounds(source, position, characterSpacing, lineSpacing, effect, effectAmount);

			result.ApplyScale(new Vector2(Scale, Scale));

			return result;
		}
	}
}

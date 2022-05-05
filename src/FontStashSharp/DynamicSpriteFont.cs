using FontStashSharp.Interfaces;
using System;
using System.Text;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
#endif

namespace FontStashSharp
{
	public partial class DynamicSpriteFont : SpriteFontBase
	{
		internal readonly Int32Map<DynamicFontGlyph> Glyphs = new Int32Map<DynamicFontGlyph>();
		private readonly Int32Map<int> Kernings = new Int32Map<int>();
		private FontMetrics[] IndexedMetrics;

		public FontSystem FontSystem { get; private set; }

		internal DynamicSpriteFont(FontSystem system, int size, int lineHeight) : base(size, lineHeight)
		{
			if (system == null)
			{
				throw new ArgumentNullException(nameof(system));
			}

			FontSystem = system;
			RenderFontSizeMultiplicator = FontSystem.FontResolutionFactor;
		}

		private DynamicFontGlyph GetGlyphWithoutBitmap(int codepoint)
		{
			DynamicFontGlyph glyph;
			if (Glyphs.TryGetValue(codepoint, out glyph))
			{
				return glyph;
			}

			int fontSourceIndex;
			var g = FontSystem.GetCodepointIndex(codepoint, out fontSourceIndex);
			if (g == null)
			{
				Glyphs[codepoint] = null;
				return null;
			}

			var fontSize = (int)(FontSize * FontSystem.FontResolutionFactor);

			var font = FontSystem.FontSources[fontSourceIndex];

			int advance, x0, y0, x1, y1;
			font.GetGlyphMetrics(g.Value, fontSize, out advance, out x0, out y0, out x1, out y1);

			var effectPad = Math.Max(FontSystem.BlurAmount, FontSystem.StrokeAmount);
			var gw = x1 - x0 + effectPad * 2;
			var gh = y1 - y0 + effectPad * 2;

			glyph = new DynamicFontGlyph
			{
				Codepoint = codepoint,
				Id = g.Value,
				Size = fontSize,
				FontSourceIndex = fontSourceIndex,
				Bounds = new Rectangle(0, 0, gw, gh),
				XAdvance = advance,
				XOffset = x0,
				YOffset = y0
			};

			Glyphs[codepoint] = glyph;

			return glyph;
		}

#if MONOGAME || FNA || STRIDE
		private DynamicFontGlyph GetGlyphInternal(GraphicsDevice device, int codepoint)
#else
		private DynamicFontGlyph GetGlyphInternal(ITexture2DManager device, int codepoint)
#endif
		{
			var glyph = GetGlyphWithoutBitmap(codepoint);
			if (glyph == null)
			{
				return null;
			}

			if (device == null || glyph.Texture != null)
				return glyph;

			FontSystem.RenderGlyphOnAtlas(device, glyph);

			return glyph;
		}

#if MONOGAME || FNA || STRIDE
		private DynamicFontGlyph GetDynamicGlyph(GraphicsDevice device, int codepoint)
#else
		private DynamicFontGlyph GetDynamicGlyph(ITexture2DManager device, int codepoint)
#endif
		{
			var result = GetGlyphInternal(device, codepoint);
			if (result == null && FontSystem.DefaultCharacter != null)
			{
				result = GetGlyphInternal(device, FontSystem.DefaultCharacter.Value);
			}

			return result;
		}

#if MONOGAME || FNA || STRIDE
		protected internal override FontGlyph GetGlyph(GraphicsDevice device, int codepoint)
#else
		protected internal override FontGlyph GetGlyph(ITexture2DManager device, int codepoint)
#endif
		{
			return GetDynamicGlyph(device, codepoint);
		}

		private void GetMetrics(int fontSourceIndex, out FontMetrics result)
		{
			if (IndexedMetrics == null || IndexedMetrics.Length != FontSystem.FontSources.Count)
			{
				IndexedMetrics = new FontMetrics[FontSystem.FontSources.Count];
				for (var i = 0; i < IndexedMetrics.Length; ++i)
				{
					int ascent, descent, lineHeight;
					FontSystem.FontSources[i].GetMetricsForSize((int)(FontSize * RenderFontSizeMultiplicator), out ascent, out descent, out lineHeight);

					IndexedMetrics[i] = new FontMetrics(ascent, descent, lineHeight);
				}
			}

			result = IndexedMetrics[fontSourceIndex];
		}

		protected override void PreDraw(string str, out int ascent, out int lineHeight)
		{
			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			for (int i = 0; i < str.Length; i += char.IsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = char.ConvertToUtf32(str, i);

				var glyph = GetDynamicGlyph(null, codepoint);
				if (glyph == null)
				{
					continue;
				}

				FontMetrics metrics;
				GetMetrics(glyph.FontSourceIndex, out metrics);
				ascent = metrics.Ascent;
				lineHeight = metrics.LineHeight + FontSystem.LineSpacing;
				break;
			}
		}

		protected override void PreDraw(StringBuilder str, out int ascent, out int lineHeight)
		{
			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			for (int i = 0; i < str.Length; i += StringBuilderIsSurrogatePair(str, i) ? 2 : 1)
			{
				var codepoint = StringBuilderConvertToUtf32(str, i);

				var glyph = GetDynamicGlyph(null, codepoint);
				if (glyph == null)
				{
					continue;
				}

				FontMetrics metrics;
				GetMetrics(glyph.FontSourceIndex, out metrics);
				ascent = metrics.Ascent;
				lineHeight = metrics.LineHeight + FontSystem.LineSpacing;
				break;
			}
		}

		protected override void InternalTextBounds(string str, Vector2 position, ref Bounds bounds)
		{
			if (string.IsNullOrEmpty(str))
				return;

			base.InternalTextBounds(str, position, ref bounds);

			bounds.X2 += FontSystem.StrokeAmount * 2;
		}

		protected override void InternalTextBounds(StringBuilder str, Vector2 position, ref Bounds bounds)
		{
			if (str == null || str.Length == 0)
				return;

			base.InternalTextBounds(str, position, ref bounds);

			bounds.X2 += FontSystem.StrokeAmount * 2;
		}

		private static int GetKerningsKey(int glyph1, int glyph2)
		{
			return ((glyph1 << 16) | (glyph1 >> 16)) ^ glyph2;
		}

		internal override void GetQuad(FontGlyph glyph, FontGlyph prevGlyph, ref float x, float y, ref FontGlyphSquad q)
		{
			if (prevGlyph != null)
			{
				var adv = 0;

				var dynamicGlyph = (DynamicFontGlyph)glyph;
				var dynamicPrevGlyph = (DynamicFontGlyph)prevGlyph;
				if (FontSystem.UseKernings && dynamicGlyph.FontSourceIndex == dynamicPrevGlyph.FontSourceIndex)
				{
					var key = GetKerningsKey(prevGlyph.Id, dynamicGlyph.Id);
					if (!Kernings.TryGetValue(key, out adv))
					{
						var fontSource = FontSystem.FontSources[dynamicGlyph.FontSourceIndex];
						adv = fontSource.GetGlyphKernAdvance(prevGlyph.Id, dynamicGlyph.Id, dynamicGlyph.Size);
						Kernings[key] = adv;
					}
				}

				x += adv + FontSystem.CharacterSpacing;
			}

			base.GetQuad(glyph, prevGlyph, ref x, y, ref q);
		}
	}
}
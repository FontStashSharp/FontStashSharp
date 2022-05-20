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
				FontSize = fontSize,
				FontSourceIndex = fontSourceIndex,
				RenderOffset = new Point(x0, y0),
				Size = new Point(gw, gh),
				XAdvance = advance,
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

		internal override void PreDraw(TextSource source, out int ascent, out int lineHeight)
		{
			// Determine ascent and lineHeight from first character
			ascent = 0;
			lineHeight = 0;
			while (true)
			{
				int codepoint;
				if (!source.GetNextCodepoint(out codepoint))
				{
					break;
				}

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

			source.Reset();
		}

		internal override void InternalTextBounds(TextSource source, Vector2 position, ref Bounds bounds)
		{
			if (source.IsNull)
				return;

			base.InternalTextBounds(source, position, ref bounds);

			bounds.X2 += FontSystem.StrokeAmount * 2;
		}

		private static int GetKerningsKey(int glyph1, int glyph2)
		{
			return ((glyph1 << 16) | (glyph1 >> 16)) ^ glyph2;
		}

		internal override float GetXAdvance(FontGlyph glyph, FontGlyph prevGlyph)
		{
			var result = glyph.XAdvance;
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
						adv = fontSource.GetGlyphKernAdvance(prevGlyph.Id, dynamicGlyph.Id, dynamicGlyph.FontSize);
						Kernings[key] = adv;
					}
				}

				result += adv + FontSystem.CharacterSpacing;
			}

			return result;
		}
	}
}
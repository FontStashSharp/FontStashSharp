using System.Collections.Generic;
using System.Text;
using System;
using FontStashSharp.Interfaces;
using System.Linq;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
using Texture2D = System.Object;
#endif

namespace FontStashSharp
{
	public abstract partial class SpriteFontBase
	{
		private static Texture2D _white;

		/// <summary>
		/// Font Size
		/// </summary>
		public float FontSize { get; private set; }

		/// <summary>
		/// Line Height in pixels
		/// </summary>
		public int LineHeight { get; private set; }

		protected float RenderFontSizeMultiplicator { get; set; } = 1f;

		protected SpriteFontBase(float fontSize, int lineHeight)
		{
			FontSize = fontSize;
			LineHeight = lineHeight;
		}

#if MONOGAME || FNA || XNA || STRIDE
		protected internal abstract FontGlyph GetGlyph(GraphicsDevice device, int codepoint, FontSystemEffect effect, int effectAmount);
#else
		protected internal abstract FontGlyph GetGlyph(ITexture2DManager device, int codepoint, FontSystemEffect effect, int effectAmount);
#endif

		internal abstract void PreDraw(TextSource str, FontSystemEffect effect, int effectAmount, out int ascent, out int lineHeight);

		protected void Prepare(Vector2 position, float rotation, Vector2 origin, ref Vector2 scale, out Matrix transformation)
		{
			scale /= RenderFontSizeMultiplicator;

			Utility.BuildTransform(position, rotation, origin, scale, out transformation);
		}

		internal virtual Bounds InternalTextBounds(TextSource source, Vector2 position,
			float characterSpacing, float lineSpacing,
			FontSystemEffect effect, int effectAmount)
		{
			if (source.IsNull) return Bounds.Empty;

			int ascent, lineHeight;
			PreDraw(source, effect, effectAmount, out ascent, out lineHeight);

			var x = position.X;
			var y = position.Y;
			y += ascent;

			float minx, maxx, miny, maxy;
			minx = maxx = x;
			miny = maxy = y;
			float startx = x;

			FontGlyph prevGlyph = null;

			while (true)
			{
				int codepoint;
				if (!source.GetNextCodepoint(out codepoint))
					break;

				if (codepoint == '\n')
				{
					x = startx;
					y += lineHeight + lineSpacing;
					prevGlyph = null;
					continue;
				}

				var glyph = GetGlyph(null, codepoint, effect, effectAmount);
				if (glyph == null)
				{
					continue;
				}

				if (prevGlyph != null)
				{
					x += characterSpacing;
					x += GetKerning(glyph, prevGlyph);
				}

				var x0 = x + glyph.RenderOffset.X;
				if (x0 < minx)
					minx = x0;
				x += glyph.XAdvance;
				if (x > maxx)
					maxx = x;

				var y0 = y + glyph.RenderOffset.Y;
				var y1 = y0 + glyph.Size.Y;
				if (y0 < miny)
					miny = y0;
				if (y1 > maxy)
					maxy = y1;

				prevGlyph = glyph;
			}

			return new Bounds(minx, miny, maxx, maxy);
		}

		public Bounds TextBounds(string text, Vector2 position, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = InternalTextBounds(new TextSource(text), position, characterSpacing, lineSpacing, effect, effectAmount);

			var realScale = scale ?? Utility.DefaultScale;
			bounds.ApplyScale(realScale / RenderFontSizeMultiplicator);
			return bounds;
		}

		public Bounds TextBounds(StringBuilder text, Vector2 position, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = InternalTextBounds(new TextSource(text), position, characterSpacing, lineSpacing, effect, effectAmount);

			var realScale = scale ?? Utility.DefaultScale;
			bounds.ApplyScale(realScale / RenderFontSizeMultiplicator);
			return bounds;
		}

		internal virtual void InternalGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2? sourceScale,
			float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount,
			List<Glyph> result)
		{
			if (source.IsNull)
			{
				return;
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, 0, origin, ref scale, out transformation);

			int ascent, lineHeight;
			PreDraw(source, effect, effectAmount, out ascent, out lineHeight);

			var pos = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			var i = 0;
			while (true)
			{
				int codepoint;
				if (!source.GetNextCodepoint(out codepoint))
				{
					break;
				}

				var rect = new Rectangle((int)pos.X, (int)pos.Y - LineHeight, 0, LineHeight);
				var xAdvance = 0;
				if (codepoint == '\n')
				{
					pos.X = 0;
					pos.Y += lineHeight + lineSpacing;
					prevGlyph = null;
				}
				else
				{
					var glyph = GetGlyph(null, codepoint, effect, effectAmount);
					if (glyph != null)
					{
						if (prevGlyph != null)
						{
							pos.X += characterSpacing;
							pos.X += GetKerning(glyph, prevGlyph);
						}

						rect = glyph.RenderRectangle;
						rect.Offset((int)pos.X, (int)pos.Y);

						xAdvance = glyph.XAdvance;
						pos.X += xAdvance;
						prevGlyph = glyph;
					}
				}

				// Apply transformation to rect
				var p = new Vector2(rect.X, rect.Y);
				p = p.Transform(ref transformation);
				var s = new Vector2(rect.Width * scale.X, rect.Height * scale.Y);

				var glyphInfo = new Glyph
				{
					Index = i,
					Codepoint = codepoint,
					Bounds = new Rectangle((int)p.X, (int)p.Y, (int)s.X, (int)s.Y),
					XAdvance = (int)(xAdvance * scale.X)
				};

				// Add to the result
				result.Add(glyphInfo);
				++i;
			}
		}

		public List<Glyph> GetGlyphs(string text, Vector2 position,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			List<Glyph> result = new List<Glyph>();

			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

			return result;
		}

		public List<Glyph> GetGlyphs(StringBuilder text, Vector2 position,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			List<Glyph> result = new List<Glyph>();

			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

			return result;
		}

		public void GetGlyphs(string text, Vector2 position, List<Glyph> result,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);

		public void GetGlyphs(StringBuilder text, Vector2 position, List<Glyph> result,
			Vector2 origin = default(Vector2), Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0) =>
			InternalGetGlyphs(new TextSource(text), position, origin, scale, characterSpacing, lineSpacing, effect, effectAmount, result);


		public Vector2 MeasureString(string text, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = TextBounds(text, Utility.Vector2Zero, scale, characterSpacing, lineSpacing, effect, effectAmount);
			return new Vector2(bounds.X2, bounds.Y2);
		}

		public Vector2 MeasureString(StringBuilder text, Vector2? scale = null,
			float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			FontSystemEffect effect = FontSystemEffect.None, int effectAmount = 0)
		{
			var bounds = TextBounds(text, Utility.Vector2Zero, scale, characterSpacing, lineSpacing, effect, effectAmount);
			return new Vector2(bounds.X2, bounds.Y2);
		}

		internal abstract float GetKerning(FontGlyph glyph, FontGlyph prevGlyph);

#if MONOGAME || FNA || XNA || STRIDE
		public static Texture2D GetWhite(GraphicsDevice graphicsDevice)
#else
		public static Texture2D GetWhite(ITexture2DManager textureManager)
#endif
		{
			if (_white != null)
			{
				return _white;
			}

#if MONOGAME || FNA || XNA || STRIDE
			_white = Texture2DManager.CreateTexture(graphicsDevice, 1, 1);
			Texture2DManager.SetTextureData(_white, new Rectangle(0, 0, 1, 1), new byte[] { 255, 255, 255, 255 });
#else
			_white = textureManager.CreateTexture(1, 1);
			textureManager.SetTextureData(_white, new Rectangle(0, 0, 1, 1), new byte[] { 255, 255, 255, 255 });
#endif

			return _white;
		}
	}
}
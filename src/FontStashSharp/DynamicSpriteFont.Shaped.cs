using System;
using System.Collections.Generic;
using FontStashSharp.Interfaces;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp
{
	partial class DynamicSpriteFont
	{
		private struct ShapedTextCacheKey : IEquatable<ShapedTextCacheKey>
		{
			public readonly string Text;
			public readonly float FontSize;

			public ShapedTextCacheKey(string text, float fontSize)
			{
				Text = text;
				FontSize = fontSize;
			}

			public bool Equals(ShapedTextCacheKey other)
			{
				return Text == other.Text && FontSize.Equals(other.FontSize);
			}

			public override bool Equals(object obj)
			{
				return obj is ShapedTextCacheKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((Text?.GetHashCode() ?? 0) * 397) ^ FontSize.GetHashCode();
				}
			}
		}

		private class ShapedTextCache
		{
			private readonly Dictionary<ShapedTextCacheKey, LinkedListNode<(ShapedTextCacheKey Key, ShapedText Value)>> _cache =
				new Dictionary<ShapedTextCacheKey, LinkedListNode<(ShapedTextCacheKey Key, ShapedText Value)>>();
			private readonly LinkedList<(ShapedTextCacheKey Key, ShapedText Value)> _lruList =
				new LinkedList<(ShapedTextCacheKey Key, ShapedText Value)>();
			private readonly int _maxCacheEntries;

			public ShapedTextCache(int maxCacheEntries)
			{
				_maxCacheEntries = maxCacheEntries;
			}

			public bool TryGet(ShapedTextCacheKey key, out ShapedText shapedText)
			{
				if (_cache.TryGetValue(key, out var node))
				{
					// Move to front (most recently used)
					_lruList.Remove(node);
					_lruList.AddFirst(node);
					shapedText = node.Value.Value;
					return true;
				}

				shapedText = null;
				return false;
			}

			public void Add(ShapedTextCacheKey key, ShapedText shapedText)
			{
				// Check if already exists
				if (_cache.ContainsKey(key))
				{
					return;
				}

				// Evict oldest entry if cache is full
				if (_cache.Count >= _maxCacheEntries)
				{
					var oldest = _lruList.Last;
					if (oldest != null)
					{
						_cache.Remove(oldest.Value.Key);
						_lruList.RemoveLast();
					}
				}

				// Add new entry
				var node = _lruList.AddFirst((key, shapedText));
				_cache[key] = node;
			}

			public void Clear()
			{
				_cache.Clear();
				_lruList.Clear();
			}
		}

		private readonly ShapedTextCache _shapedTextCache;

		/// <summary>
		/// Shape text using HarfBuzz with caching
		/// </summary>
		internal ShapedText GetShapedText(string text, float fontSize)
		{
			if (string.IsNullOrEmpty(text))
			{
				return new ShapedText
				{
					Glyphs = new ShapedGlyph[0],
					OriginalText = text ?? string.Empty,
					FontSize = fontSize
				};
			}

			var cacheKey = new ShapedTextCacheKey(text, fontSize);
			if (_shapedTextCache.TryGet(cacheKey, out var cachedResult))
			{
				return cachedResult;
			}

			var shapedText = FontSystem.ShapeText(text, fontSize);
			_shapedTextCache.Add(cacheKey, shapedText);

			return shapedText;
		}

		/// <summary>
		/// Clear the shaped text cache
		/// </summary>
		internal void ClearShapedTextCache()
		{
			_shapedTextCache.Clear();
		}

		/// <summary>
		/// Get a glyph by its glyph ID
		/// </summary>
#if MONOGAME || FNA || XNA || STRIDE
		internal DynamicFontGlyph GetGlyphById(GraphicsDevice device, int glyphId, int fontSourceIndex, FontSystemEffect effect, int effectAmount)
#else
		internal DynamicFontGlyph GetGlyphById(ITexture2DManager device, int glyphId, int fontSourceIndex, FontSystemEffect effect, int effectAmount)
#endif
		{
			if (effect == FontSystemEffect.None)
			{
			}
			else if (effectAmount == 0)
			{
				effect = FontSystemEffect.None;
			}

			var storage = GetGlyphStorage(effect, effectAmount);

			var key = (fontSourceIndex << 24) | glyphId;

			DynamicFontGlyph glyph;
			if (storage.GlyphsByIds.TryGetValue(key, out glyph))
			{
				if (device != null && glyph.Texture == null)
				{
					FontSystem.RenderGlyphOnAtlas(device, glyph);
				}
				return glyph;
			}

			var fontSize = FontSize * FontSystem.FontResolutionFactor;
			var font = FontSystem.FontSources[fontSourceIndex];

			int advance, x0, y0, x1, y1;
			font.GetGlyphMetrics(glyphId, fontSize, out advance, out x0, out y0, out x1, out y1);

			var gw = x1 - x0 + effectAmount * 2;
			var gh = y1 - y0 + effectAmount * 2;

			glyph = new DynamicFontGlyph
			{
				Codepoint = 0, // Not applicable for shaped glyphs
				Id = glyphId,
				FontSize = fontSize,
				FontSourceIndex = fontSourceIndex,
				RenderOffset = new Point(x0, y0),
				Size = new Point(gw, gh),
				XAdvance = advance,
				Effect = effect,
				EffectAmount = effectAmount
			};

			storage.GlyphsByIds[key] = glyph;

			if (device != null && glyph.Texture == null)
			{
				FontSystem.RenderGlyphOnAtlas(device, glyph);
			}

			return glyph;
		}

		internal override float InternalDrawText(IFontStashRenderer renderer, TextColorSource source, Vector2 position, float rotation, Vector2 origin, Vector2? sourceScale, float layerDepth, float characterSpacing, float lineSpacing, TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (FontSystem.UseTextShaping)
			{
				return DrawShapedText(renderer, source, position, rotation, origin, sourceScale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
			}

			return base.InternalDrawText(renderer, source, position, rotation, origin, sourceScale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		internal override float InternalDrawText2(IFontStashRenderer2 renderer, TextColorSource source, Vector2 position, float rotation, Vector2 origin, Vector2? sourceScale, float layerDepth, float characterSpacing, float lineSpacing, TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (FontSystem.UseTextShaping)
			{
				return DrawShapedText2(renderer, source, position, rotation, origin, sourceScale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
			}

			return base.InternalDrawText2(renderer, source, position, rotation, origin, sourceScale, layerDepth, characterSpacing, lineSpacing, textStyle, effect, effectAmount);
		}

		internal override Bounds InternalTextBounds(TextSource source, Vector2 position,
			float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount)
		{
			if (source.IsNull)
				return Bounds.Empty;

			if (FontSystem.UseTextShaping)
			{
				return InternalShapedTextBounds(source, position, characterSpacing, lineSpacing, effect, effectAmount);
			}

			var result = base.InternalTextBounds(source, position, characterSpacing, lineSpacing, effect, effectAmount);
			if (effect != FontSystemEffect.None)
			{
				result.X2 += effectAmount * 2;
			}

			return result;
		}

		internal override void InternalGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2? sourceScale, float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount, List<Glyph> result)
		{
			if (FontSystem.UseTextShaping)
			{
				InternalShapedGetGlyphs(source, position, origin, sourceScale, characterSpacing, lineSpacing, effect, effectAmount, result);
				return;
			}

			base.InternalGetGlyphs(source, position, origin, sourceScale, characterSpacing, lineSpacing, effect, effectAmount, result);
		}

		private float DrawShapedText(IFontStashRenderer renderer, TextColorSource source, Vector2 position,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || XNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			var text = source.TextSource.StringText.String ?? source.TextSource.StringBuilderText?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return 0.0f;
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, rotation, origin, ref scale, out transformation);

			var lines = text.Split('\n');

			int ascent = 0, lineHeight = 0;
			if (FontSystem.FontSources.Count > 0)
			{
				int descent, lh;
				FontSystem.FontSources[0].GetMetricsForSize(FontSize * FontSystem.FontResolutionFactor, out ascent, out descent, out lh);
				lineHeight = lh;
			}

			var pos = new Vector2(0, ascent);
			float maxX = 0;
			Color? firstColor = null;

			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var line = lines[lineIndex];

				if (lineIndex > 0)
				{
					if (textStyle != TextStyle.None && firstColor != null)
					{
						RenderStyle(renderer, textStyle, pos,
							lineHeight, ascent, firstColor.Value, ref transformation,
							rotation, scale, layerDepth);
					}

					pos.X = 0.0f;
					pos.Y += lineHeight + lineSpacing;
					firstColor = null;
				}

				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				var shapedText = GetShapedText(line, FontSize * FontSystem.FontResolutionFactor);

				float lineStartX = pos.X;
				for (int i = 0; i < shapedText.Glyphs.Length; i++)
				{
					var shapedGlyph = shapedText.Glyphs[i];

					if (i > 0 && characterSpacing > 0)
					{
						pos.X += characterSpacing;
					}

#if MONOGAME || FNA || XNA || STRIDE
					var glyph = GetGlyphById(renderer.GraphicsDevice, shapedGlyph.GlyphId, shapedGlyph.FontSourceId, effect, effectAmount);
#else
					var glyph = GetGlyphById(renderer.TextureManager, shapedGlyph.GlyphId, shapedGlyph.FontSourceId, effect, effectAmount);
#endif

					if (glyph != null && !glyph.IsEmpty)
					{
						var color = source.GetNextColor();
						firstColor = color;

						// Apply HarfBuzz positioning
						var glyphPos = pos + new Vector2(
							glyph.RenderOffset.X + shapedGlyph.XOffset,
							glyph.RenderOffset.Y + shapedGlyph.YOffset
						);

						glyphPos = glyphPos.Transform(ref transformation);

						renderer.Draw(glyph.Texture,
							glyphPos,
							glyph.TextureRectangle,
							color,
							rotation,
							scale,
							layerDepth);
					}

					pos.X += shapedGlyph.XAdvance;
					pos.Y += shapedGlyph.YAdvance;
				}

				if (pos.X > maxX)
				{
					maxX = pos.X;
				}
			}

			if (textStyle != TextStyle.None && firstColor != null)
			{
				RenderStyle(renderer, textStyle, pos,
					lineHeight, ascent, firstColor.Value, ref transformation,
					rotation, scale, layerDepth);
			}

			return position.X + maxX;
		}

		private float DrawShapedText2(IFontStashRenderer2 renderer, TextColorSource source, Vector2 position,
			float rotation, Vector2 origin, Vector2? sourceScale,
			float layerDepth, float characterSpacing, float lineSpacing,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || XNA || STRIDE
			if (renderer.GraphicsDevice == null)
			{
				throw new ArgumentNullException("renderer.GraphicsDevice can't be null.");
			}
#else
			if (renderer.TextureManager == null)
			{
				throw new ArgumentNullException("renderer.TextureManager can't be null.");
			}
#endif

			var text = source.TextSource.StringText.String ?? source.TextSource.StringBuilderText?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return 0.0f;
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, rotation, origin, ref scale, out transformation);

			var lines = text.Split('\n');

			int ascent = 0, lineHeight = 0;
			if (FontSystem.FontSources.Count > 0)
			{
				int descent, lh;
				FontSystem.FontSources[0].GetMetricsForSize(FontSize * FontSystem.FontResolutionFactor, out ascent, out descent, out lh);
				lineHeight = lh;
			}

			var pos = new Vector2(0, ascent);
			float maxX = 0;
			Color? firstColor = null;
			var topLeft = new VertexPositionColorTexture();
			var topRight = new VertexPositionColorTexture();
			var bottomLeft = new VertexPositionColorTexture();
			var bottomRight = new VertexPositionColorTexture();

			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var line = lines[lineIndex];

				if (lineIndex > 0)
				{
					if (textStyle != TextStyle.None && firstColor != null)
					{
						RenderStyle(renderer, textStyle, pos,
							lineHeight, ascent, firstColor.Value, ref transformation, layerDepth,
							ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
					}

					pos.X = 0.0f;
					pos.Y += lineHeight + lineSpacing;
					firstColor = null;
				}

				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				var shapedText = GetShapedText(line, FontSize * FontSystem.FontResolutionFactor);

				float lineStartX = pos.X;
				for (int i = 0; i < shapedText.Glyphs.Length; i++)
				{
					var shapedGlyph = shapedText.Glyphs[i];

					if (i > 0 && characterSpacing > 0)
					{
						pos.X += characterSpacing;
					}

#if MONOGAME || FNA || XNA || STRIDE
					var glyph = GetGlyphById(renderer.GraphicsDevice, shapedGlyph.GlyphId, shapedGlyph.FontSourceId, effect, effectAmount);
#else
					var glyph = GetGlyphById(renderer.TextureManager, shapedGlyph.GlyphId, shapedGlyph.FontSourceId, effect, effectAmount);
#endif

					if (glyph != null && !glyph.IsEmpty)
					{
						var color = source.GetNextColor();
						firstColor = color;

						// Apply HarfBuzz positioning
						var glyphPos = pos + new Vector2(
							glyph.RenderOffset.X + shapedGlyph.XOffset,
							glyph.RenderOffset.Y + shapedGlyph.YOffset
						);

						var size = new Vector2(glyph.Size.X, glyph.Size.Y);
						renderer.DrawQuad(glyph.Texture, color, glyphPos, ref transformation,
							layerDepth, size, glyph.TextureRectangle,
							ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
					}

					pos.X += shapedGlyph.XAdvance;
					pos.Y += shapedGlyph.YAdvance;
				}

				if (pos.X > maxX)
				{
					maxX = pos.X;
				}
			}

			if (textStyle != TextStyle.None && firstColor != null)
			{
				RenderStyle(renderer, textStyle, pos,
					lineHeight, ascent, firstColor.Value, ref transformation, layerDepth,
					ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
			}

			return position.X + maxX;
		}

		private Bounds InternalShapedTextBounds(TextSource source, Vector2 position,
			float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount)
		{
			var text = source.StringText.String ?? source.StringBuilderText?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return Bounds.Empty;
			}

			var lines = text.Split('\n');

			int ascent = 0, lineHeight = 0;
			if (FontSystem.FontSources.Count > 0)
			{
				int descent, lh;
				FontSystem.FontSources[0].GetMetricsForSize(FontSize * FontSystem.FontResolutionFactor, out ascent, out descent, out lh);
				lineHeight = lh;
			}

			var x = position.X;
			var y = position.Y + ascent;

			float minx = x, maxx = x, miny = y, maxy = y;

			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var line = lines[lineIndex];

				if (lineIndex > 0)
				{
					x = position.X;
					y += lineHeight + lineSpacing;
				}

				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				var shapedText = GetShapedText(line, FontSize * FontSystem.FontResolutionFactor);

				float lineStartX = x;
				for (int i = 0; i < shapedText.Glyphs.Length; i++)
				{
					var shapedGlyph = shapedText.Glyphs[i];

					if (i > 0 && characterSpacing > 0)
					{
						x += characterSpacing;
					}

					var glyph = GetGlyphById(null, shapedGlyph.GlyphId, shapedGlyph.FontSourceId, effect, effectAmount);
					if (glyph != null && !glyph.IsEmpty)
					{
						var glyphX = x + glyph.RenderOffset.X + shapedGlyph.XOffset;
						var glyphY = y + glyph.RenderOffset.Y + shapedGlyph.YOffset;
						var glyphX2 = glyphX + glyph.Size.X;
						var glyphY2 = glyphY + glyph.Size.Y;

						if (glyphX < minx) minx = glyphX;
						if (glyphY < miny) miny = glyphY;
						if (glyphX2 > maxx) maxx = glyphX2;
						if (glyphY2 > maxy) maxy = glyphY2;
					}

					x += shapedGlyph.XAdvance;
					y += shapedGlyph.YAdvance;
				}

				if (x > maxx) maxx = x;
			}

			if (effect != FontSystemEffect.None)
			{
				maxx += effectAmount * 2;
			}

			return new Bounds(minx, miny, maxx, maxy);
		}

		private void InternalShapedGetGlyphs(TextSource source, Vector2 position, Vector2 origin, Vector2? sourceScale, float characterSpacing, float lineSpacing, FontSystemEffect effect, int effectAmount, List<Glyph> result)
		{
			var text = source.StringText.String ?? source.StringBuilderText?.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, 0, origin, ref scale, out transformation);

			var lines = text.Split('\n');

			int ascent = 0, lineHeight = 0;
			if (FontSystem.FontSources.Count > 0)
			{
				int descent, lh;
				FontSystem.FontSources[0].GetMetricsForSize(FontSize * FontSystem.FontResolutionFactor, out ascent, out descent, out lh);
				lineHeight = lh;
			}

			var pos = new Vector2(0, ascent);
			for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				var line = lines[lineIndex];
				if (lineIndex > 0)
				{
					pos.X = 0.0f;
					pos.Y += lineHeight + lineSpacing;
				}

				if (string.IsNullOrEmpty(line))
				{
					continue;
				}

				var shapedText = GetShapedText(line, FontSize * FontSystem.FontResolutionFactor);

				float lineStartX = pos.X;
				for (int i = 0; i < shapedText.Glyphs.Length; i++)
				{
					var shapedGlyph = shapedText.Glyphs[i];

					if (i > 0 && characterSpacing > 0)
					{
						pos.X += characterSpacing;
					}

					var glyphPos = pos + new Vector2(shapedGlyph.XOffset, shapedGlyph.YOffset);
					var s = Vector2.Zero;
					var glyph = GetGlyphById(null, shapedGlyph.GlyphId, shapedGlyph.FontSourceId, effect, effectAmount);
					if (glyph != null && !glyph.IsEmpty)
					{
						// Apply HarfBuzz positioning
						glyphPos += new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y);

						var rect = glyph.RenderRectangle;
						s = new Vector2(rect.Width * scale.X, rect.Height * scale.Y);
					}

					glyphPos = glyphPos.Transform(ref transformation);
					var glyphInfo = new Glyph
					{
						Index = i,
						Codepoint = shapedGlyph.GlyphId,
						Bounds = new Rectangle((int)glyphPos.X, (int)glyphPos.Y, (int)s.X, (int)s.Y),
						XAdvance = (int)(shapedGlyph.XAdvance * scale.X)
					};

					// Add to the result
					result.Add(glyphInfo);

					pos.X += shapedGlyph.XAdvance;
					pos.Y += shapedGlyph.YAdvance;
				}
			}
		}
	}
}
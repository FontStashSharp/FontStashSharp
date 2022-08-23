using FontStashSharp.Interfaces;
using System.Text;
using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
#endif

namespace FontStashSharp
{
	partial class SpriteFontBase
	{
		private float DrawText(IFontStashRenderer2 renderer, TextColorSource source,
			Vector2 position, Vector2? sourceScale, float rotation, Vector2 origin,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

#if MONOGAME || FNA || STRIDE
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

			if (source.IsNull) return 0.0f;

			Matrix transformation;
			var scale = sourceScale ?? Utility.DefaultScale;
			Prepare(position, ref scale, rotation, origin, out transformation);

			int ascent, lineHeight;
			PreDraw(source.TextSource, out ascent, out lineHeight);

			var pos = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			var topLeft = new VertexPositionColorTexture();
			var topRight = new VertexPositionColorTexture();
			var bottomLeft = new VertexPositionColorTexture();
			var bottomRight = new VertexPositionColorTexture();

			while (true)
			{
				int codepoint;
				Color color;
				if (!source.GetNextCodepoint(out codepoint))
					break;

				if (codepoint == '\n')
				{
					pos.X = 0.0f;
					pos.Y += lineHeight;
					prevGlyph = null;
					continue;
				}

#if MONOGAME || FNA || STRIDE
				var glyph = GetGlyph(renderer.GraphicsDevice, codepoint);
#else
				var glyph = GetGlyph(renderer.TextureManager, codepoint);
#endif
				if (glyph == null)
				{
					continue;
				}

				if (prevGlyph != null)
				{
					pos.X += characterSpacing;
					pos.X += GetKerning(glyph, prevGlyph);
				}

				if (!glyph.IsEmpty)
				{
					 color = source.GetNextColor();
					
#if MONOGAME || FNA || STRIDE
					var textureSize = new Point(glyph.Texture.Width, glyph.Texture.Height);
					var setColor = color;
#else
					var textureSize = renderer.TextureManager.GetTextureSize(glyph.Texture);
					var setColor = (uint)(color.A << 24 | color.B << 16 | color.G << 8 | color.R);
#endif
					var baseOffset = new Vector2(glyph.RenderOffset.X, glyph.RenderOffset.Y) + pos;

					topLeft.Position = baseOffset.TransformToVector3(ref transformation, layerDepth);
					topLeft.TextureCoordinate = new Vector2((float)glyph.TextureRectangle.X / textureSize.X,
															(float)glyph.TextureRectangle.Y / textureSize.Y);
					topLeft.Color = setColor;

					topRight.Position = (baseOffset + new Vector2(glyph.Size.X, 0)).TransformToVector3(ref transformation, layerDepth);
					topRight.TextureCoordinate = new Vector2((float)glyph.TextureRectangle.Right / textureSize.X,
															 (float)glyph.TextureRectangle.Y / textureSize.Y);
					topRight.Color = setColor;

					bottomLeft.Position = (baseOffset + new Vector2(0, glyph.Size.Y)).TransformToVector3(ref transformation, layerDepth);
					bottomLeft.TextureCoordinate = new Vector2((float)glyph.TextureRectangle.Left / textureSize.X,
																 (float)glyph.TextureRectangle.Bottom / textureSize.Y);
					bottomLeft.Color = setColor;

					bottomRight.Position = (baseOffset + new Vector2(glyph.Size.X, glyph.Size.Y)).TransformToVector3(ref transformation, layerDepth);
					bottomRight.TextureCoordinate = new Vector2((float)glyph.TextureRectangle.Right / textureSize.X,
																(float)glyph.TextureRectangle.Bottom / textureSize.Y);
					bottomRight.Color = setColor;

					renderer.DrawQuad(glyph.Texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
				}

				pos.X += glyph.XAdvance;
				prevGlyph = glyph;
			}

			return position.X + position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color color,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, color), position, scale, rotation, origin, layerDepth, characterSpacing, lineSpacing);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color[] colors,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, colors), position, scale, rotation, origin, layerDepth, characterSpacing, lineSpacing);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer2 renderer, StringSegment text, Vector2 position, Color color,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, color), position, scale, rotation, origin, layerDepth, characterSpacing, lineSpacing);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer2 renderer, StringSegment text, Vector2 position, Color[] colors,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, colors), position, scale, rotation, origin, layerDepth, characterSpacing, lineSpacing);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color color,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, color), position, scale, rotation, origin, layerDepth, characterSpacing, lineSpacing);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="colors">Colors of glyphs</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">A character spacing</param>
		/// <param name="lineSpacing">A line spacing</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color[] colors,
			Vector2? scale = null, float rotation = 0, Vector2 origin = default(Vector2),
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, colors), position, scale, rotation, origin, layerDepth, characterSpacing, lineSpacing);
	}
}

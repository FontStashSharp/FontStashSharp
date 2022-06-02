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
		private static void CreateTransform(Vector2 position, Vector2 scale, float rotation, Vector2 origin, out Matrix result)
		{
			result = Matrix.Identity;
			float cos = 0, sin = 0;

			float offsetX, offsetY;
			if (rotation == 0)
			{
				result.M11 = scale.X;
				result.M22 = scale.Y;

				offsetX = position.X - (origin.X * result.M11);
				offsetY = position.Y - (origin.Y * result.M22);
			}
			else
			{
				cos = (float)Math.Cos(rotation);
				sin = (float)Math.Sin(rotation);
				result.M11 = scale.X * cos;
				result.M12 = scale.X * sin;
				result.M21 = scale.Y * -sin;
				result.M22 = scale.Y * cos;

				offsetX = position.X - (origin.X * result.M11) - (origin.Y * result.M21);
				offsetY = position.Y - (origin.X * result.M12) - (origin.Y * result.M22);
			}

#if MONOGAME || FNA || STRIDE
			result.M41 = offsetX;
			result.M42 = offsetY;
#else
			result.M31 = offsetX;
			result.M32 = offsetY;
#endif
		}

		private float DrawText(IFontStashRenderer2 renderer, TextColorSource source, Vector2 position, 
			Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f)
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

			scale /= RenderFontSizeMultiplicator;

			Matrix transformation;
			CreateTransform(position, scale, rotation, origin, out transformation);

			int ascent, lineHeight;
			PreDraw(source.TextSource, out ascent, out lineHeight);

			var offset = new Vector2(0, ascent);

			FontGlyph prevGlyph = null;
			while(true)
			{
				int codepoint;
				Color color;
				if (!source.GetNextCodepoint(out codepoint, out color))
					break;

				if (codepoint == '\n')
				{
					offset.X = 0.0f;
					offset.Y += lineHeight;
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

				if (!glyph.IsEmpty)
				{
					renderer.Texture = glyph.Texture;

					var baseOffset = glyph.RenderOffset.ToVector2() + offset;

					var topLeft = baseOffset.Transform(ref transformation);
					var topRight = (baseOffset + new Vector2(glyph.Size.X, 0)).Transform(ref transformation);
					var topBottom = (baseOffset + new Vector2(0, glyph.Size.Y)).Transform(ref transformation);
					var bottomLeft = (baseOffset + new Vector2(glyph.Size.X, glyph.Size.Y)).Transform(ref transformation);
				}

				offset.X += GetXAdvance(glyph, prevGlyph);
				prevGlyph = glyph;
			}

			return position.X;
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color color,
			Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, color), position, scale, rotation, origin, layerDepth);

			/// <summary>
			/// Draws a text
			/// </summary>
			/// <param name="renderer">A renderer.</param>
			/// <param name="text">The text which will be drawn.</param>
			/// <param name="position">The drawing location on screen.</param>
			/// <param name="color">A color mask.</param>
			/// <param name="scale">A scaling of this text.</param>
			/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, scale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color color, float layerDepth = 0.0f)
		{
			return DrawText(renderer, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);
		}

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color[] colors,
			Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, colors), position, scale, rotation, origin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f) =>
			DrawText(renderer, text, position, colors, scale, 0, DefaultOrigin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, string text, Vector2 position, Color[] colors, float layerDepth = 0.0f) =>
			DrawText(renderer, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color color,
			Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, color), position, scale, rotation, origin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color color, Vector2 scale, float layerDepth = 0.0f) =>
			DrawText(renderer, text, position, color, scale, 0, DefaultOrigin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">A color mask.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color color, float layerDepth = 0.0f) =>
			DrawText(renderer, text, position, color, DefaultScale, 0, DefaultOrigin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="rotation">A rotation of this text in radians.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color[] colors,
			Vector2 scale, float rotation, Vector2 origin, float layerDepth = 0.0f) =>
				DrawText(renderer, new TextColorSource(text, colors), position, scale, rotation, origin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="scale">A scaling of this text.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color[] colors, Vector2 scale, float layerDepth = 0.0f) =>
			DrawText(renderer, text, position, colors, scale, 0, DefaultOrigin, layerDepth);

		/// <summary>
		/// Draws a text
		/// </summary>
		/// <param name="renderer">A renderer.</param>
		/// <param name="text">The text which will be drawn.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="colors">Colors of glyphs.</param>
		/// <param name="layerDepth">A depth of the layer of this string.</param>
		public float DrawText(IFontStashRenderer2 renderer, StringBuilder text, Vector2 position, Color[] colors, float layerDepth = 0.0f) =>
			DrawText(renderer, text, position, colors, DefaultScale, 0, DefaultOrigin, layerDepth);
	}
}
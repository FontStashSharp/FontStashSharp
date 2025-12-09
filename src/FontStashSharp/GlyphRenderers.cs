using System;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Graphics.SpriteFont;

#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
#else

using System.Drawing;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp
{
	public struct GlyphRenderOptions
	{
		public FontSystemEffect Effect;
		public int EffectAmount;
		public Point Size;
		public bool PremultiplyAlpha;
	}

	public delegate void GlyphRenderer(byte[] input, byte[] output, GlyphRenderOptions options);

	public static class GlyphRenderers
	{
		private static void Blur(byte[] dst, int w, int h, int dstStride, int blur)
		{
			int alpha;
			float sigma;
			if (blur < 1)
				return;
			sigma = blur * 0.57735f;
			alpha = (int)((1 << 16) * (1.0f - Math.Exp(-2.3f / (sigma + 1.0f))));
			BlurRows(dst, w, h, dstStride, alpha);
			BlurCols(dst, w, h, dstStride, alpha);
			BlurRows(dst, w, h, dstStride, alpha);
			BlurCols(dst, w, h, dstStride, alpha);
		}

		private static void BlurCols(byte[] dst, int w, int h, int dstStride, int alpha)
		{
			int x;
			int y;

			int index = 0;
			for (y = 0; y < h; y++)
			{
				var z = 0;
				for (x = 1; x < w; x++)
				{
					z += (alpha * ((dst[index + x] << 7) - z)) >> 16;
					dst[index + x] = (byte)(z >> 7);
				}

				dst[index + w - 1] = 0;
				z = 0;
				for (x = w - 2; x >= 0; x--)
				{
					z += (alpha * ((dst[index + x] << 7) - z)) >> 16;
					dst[index + x] = (byte)(z >> 7);
				}

				dst[index] = 0;
				index += dstStride;
			}
		}

		private static void BlurRows(byte[] dst, int w, int h, int dstStride, int alpha)
		{
			int x;
			int y;
			int index = 0;
			for (x = 0; x < w; x++)
			{
				var z = 0;
				for (y = dstStride; y < h * dstStride; y += dstStride)
				{
					z += (alpha * ((dst[index + y] << 7) - z)) >> 16;
					dst[index + y] = (byte)(z >> 7);
				}

				dst[index + (h - 1) * dstStride] = 0;
				z = 0;
				for (y = (h - 2) * dstStride; y >= 0; y -= dstStride)
				{
					z += (alpha * ((dst[index + y] << 7) - z)) >> 16;
					dst[index + y] = (byte)(z >> 7);
				}

				dst[index] = 0;
				++index;
			}
		}

		public static GlyphRenderer Default = (input, output, options) =>
		{
			var bufferSize = options.Size.X * options.Size.Y;
			if (options.Effect == FontSystemEffect.Stroked && options.EffectAmount > 0)
			{
				var width = options.Size.X;
				var top = width * options.EffectAmount;
				var bottom = (options.Size.Y - options.EffectAmount) * options.Size.X;
				var right = options.Size.X - options.EffectAmount;
				var left = options.EffectAmount;

				byte d;
				for (var i = 0; i < bufferSize; ++i)
				{
					var ci = i * 4;
					var col = input[i];
					var black = 0;
					if (col == 255)
					{
						output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = 255;
						continue;
					}

					if (i >= top)
						black = input[i - top];
					if (i < bottom)
					{
						d = input[i + top];
						black = ((255 - d) * black + 255 * d) / 255;
					}
					if (i % width >= left)
					{
						d = input[i - options.EffectAmount];
						black = ((255 - d) * black + 255 * d) / 255;
					}
					if (i % width < right)
					{
						d = input[i + options.EffectAmount];
						black = ((255 - d) * black + 255 * d) / 255;
					}

					if (black == 0)
					{
						if (col == 0)
						{
							output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = 0; //black transparency to suit stroke
							continue;
						}

						if (options.PremultiplyAlpha)
						{
							output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = col;
						}
						else
						{
							output[ci] = output[ci + 1] = output[ci + 2] = 255;
							output[ci + 3] = col;
						}
					}
					else
					{
						if (col == 0)
						{
							output[ci] = output[ci + 1] = output[ci + 2] = 0;
							output[ci + 3] = (byte)black;
							continue;
						}

						if (options.PremultiplyAlpha)
						{
							var alpha = ((255 - col) * black + 255 * col) / 255;
							output[ci] = output[ci + 1] = output[ci + 2] = (byte)((alpha * col) / 255);
							output[ci + 3] = (byte)alpha;
						}
						else
						{
							output[ci] = output[ci + 1] = output[ci + 2] = col;
							output[ci + 3] = (byte)(((255 - col) * black + 255 * col) / 255);
						}
					}
				}
			}
			else
			{
				if (options.Effect == FontSystemEffect.Blurry && options.EffectAmount > 0)
				{
					Blur(input, options.Size.X, options.Size.Y, options.Size.X, options.EffectAmount);
				}
				for (var i = 0; i < bufferSize; ++i)
				{
					var ci = i * 4;
					var c = input[i];
					if (options.PremultiplyAlpha)
					{
						output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = c;
					}
					else
					{
						output[ci] = output[ci + 1] = output[ci + 2] = 255;
						output[ci + 3] = c;
					}
				}
			}
		};
	}
}
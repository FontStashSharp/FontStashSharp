using FontStashSharp.Interfaces;
using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using Texture2D = System.Object;
#endif


namespace FontStashSharp
{
	public class FontAtlas
	{
		byte[] _byteBuffer;
		byte[] _colorBuffer;

		public int Width { get; private set; }

		public int Height { get; private set; }

		public int NodesNumber { get; private set; }

		internal FontAtlasNode[] Nodes { get; private set; }

		public Texture2D Texture { get; set; }

		public FontAtlas(int w, int h, int count, Texture2D texture)
		{
			Width = w;
			Height = h;
			Texture = texture;
			Nodes = new FontAtlasNode[count];
			Nodes[0].X = 0;
			Nodes[0].Y = 0;
			Nodes[0].Width = w;
			NodesNumber++;
		}

		public void InsertNode(int idx, int x, int y, int w)
		{
			if (NodesNumber + 1 > Nodes.Length)
			{
				var oldNodes = Nodes;
				var newLength = Nodes.Length == 0 ? 8 : Nodes.Length * 2;
				Nodes = new FontAtlasNode[newLength];
				for (var i = 0; i < oldNodes.Length; ++i)
				{
					Nodes[i] = oldNodes[i];
				}
			}

			for (var i = NodesNumber; i > idx; i--)
				Nodes[i] = Nodes[i - 1];
			Nodes[idx].X = x;
			Nodes[idx].Y = y;
			Nodes[idx].Width = w;
			NodesNumber++;
		}

		public void RemoveNode(int idx)
		{
			if (NodesNumber == 0)
				return;
			for (var i = idx; i < NodesNumber - 1; i++)
				Nodes[i] = Nodes[i + 1];
			NodesNumber--;
		}

		public void Reset(int w, int h)
		{
			Width = w;
			Height = h;
			NodesNumber = 0;
			Nodes[0].X = 0;
			Nodes[0].Y = 0;
			Nodes[0].Width = w;
			NodesNumber++;
		}

		public bool AddSkylineLevel(int idx, int x, int y, int w, int h)
		{
			InsertNode(idx, x, y + h, w);
			for (var i = idx + 1; i < NodesNumber; i++)
				if (Nodes[i].X < Nodes[i - 1].X + Nodes[i - 1].Width)
				{
					var shrink = Nodes[i - 1].X + Nodes[i - 1].Width - Nodes[i].X;
					Nodes[i].X += shrink;
					Nodes[i].Width -= shrink;
					if (Nodes[i].Width <= 0)
					{
						RemoveNode(i);
						i--;
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}

			for (var i = 0; i < NodesNumber - 1; i++)
				if (Nodes[i].Y == Nodes[i + 1].Y)
				{
					Nodes[i].Width += Nodes[i + 1].Width;
					RemoveNode(i + 1);
					i--;
				}

			return true;
		}

		public int RectFits(int i, int w, int h)
		{
			var x = Nodes[i].X;
			var y = Nodes[i].Y;
			if (x + w > Width)
				return -1;
			var spaceLeft = w;
			while (spaceLeft > 0)
			{
				if (i == NodesNumber)
					return -1;
				y = Math.Max(y, Nodes[i].Y);
				if (y + h > Height)
					return -1;
				spaceLeft -= Nodes[i].Width;
				++i;
			}

			return y;
		}

		public bool AddRect(int rw, int rh, ref int rx, ref int ry)
		{
			var besth = Height;
			var bestw = Width;
			var besti = -1;
			var bestx = -1;
			var besty = -1;
			for (var i = 0; i < NodesNumber; i++)
			{
				var y = RectFits(i, rw, rh);
				if (y != -1)
					if (y + rh < besth || y + rh == besth && Nodes[i].Width < bestw)
					{
						besti = i;
						bestw = Nodes[i].Width;
						besth = y + rh;
						bestx = Nodes[i].X;
						besty = y;
					}
			}

			if (besti == -1)
				return false;
			if (!AddSkylineLevel(besti, bestx, besty, rw, rh))
				return false;

			rx = bestx;
			ry = besty;
			return true;
		}

#if MONOGAME || FNA || STRIDE
		public void RenderGlyph(GraphicsDevice graphicsDevice, DynamicFontGlyph glyph, IFontSource fontSource, int blurAmount, int strokeAmount, bool premultiplyAlpha, int kernelWidth, int kernelHeight)
#else
		public void RenderGlyph(ITexture2DManager textureManager, DynamicFontGlyph glyph, IFontSource fontSource, int blurAmount, int strokeAmount, bool premultiplyAlpha, int kernelWidth, int kernelHeight)
#endif
		{
			if (glyph.IsEmpty)
			{
				return;
			}

			// Render glyph to byte buffer
			var bufferSize = glyph.Size.X * glyph.Size.Y;
			var buffer = _byteBuffer;

			if ((buffer == null) || (buffer.Length < bufferSize))
			{
				buffer = new byte[bufferSize];
				_byteBuffer = buffer;
			}
			Array.Clear(buffer, 0, bufferSize);

			var colorBuffer = _colorBuffer;
			if ((colorBuffer == null) || (colorBuffer.Length < bufferSize * 4))
			{
				colorBuffer = new byte[bufferSize * 4];
				_colorBuffer = colorBuffer;
			}

			var effectPad = Math.Max(blurAmount, strokeAmount);
			fontSource.RasterizeGlyphBitmap(glyph.Id,
				glyph.FontSize,
				buffer,
				effectPad + effectPad * glyph.Size.X,
				glyph.Size.X - effectPad * 2,
				glyph.Size.Y - effectPad * 2,
				glyph.Size.X);

			if (strokeAmount > 0)
			{
				var width = glyph.Size.X;
				var top = width * strokeAmount;
				var bottom = (glyph.Size.Y - strokeAmount) * glyph.Size.X;
				var right = glyph.Size.X - strokeAmount;
				var left = strokeAmount;

				byte d;
				for (var i = 0; i < bufferSize; ++i)
				{
					var ci = i * 4;
					var col = buffer[i];
					var black = 0;
					if (col == 255)
					{
						colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = colorBuffer[ci + 3] = 255;
						continue;
					}

					if (i >= top)
						black = buffer[i - top];
					if (i < bottom)
					{
						d = buffer[i + top];
						black = ((255 - d) * black + 255 * d) / 255;
					}
					if (i % width >= left)
					{
						d = buffer[i - strokeAmount];
						black = ((255 - d) * black + 255 * d) / 255;
					}
					if (i % width < right)
					{
						d = buffer[i + strokeAmount];
						black = ((255 - d) * black + 255 * d) / 255;
					}

					if (black == 0)
					{
						if (col == 0)
						{
							colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = colorBuffer[ci + 3] = 0; //black transparency to suit stroke
							continue;
						}

						if (premultiplyAlpha)
						{
							colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = colorBuffer[ci + 3] = col;
						}
						else
						{
							colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = 255;
							colorBuffer[ci + 3] = col;
						}
					}
					else
					{
						if (col == 0)
						{
							colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = 0;
							colorBuffer[ci + 3] = (byte)black;
							continue;
						}

						if (premultiplyAlpha)
						{
							var alpha = ((255 - col) * black + 255 * col) / 255;
							colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = (byte)((alpha * col) / 255);
							colorBuffer[ci + 3] = (byte)alpha;
						}
						else
						{
							colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = col;
							colorBuffer[ci + 3] = (byte)(((255 - col) * black + 255 * col) / 255);
						}
					}
				}
			}
			else
			{
				if (blurAmount > 0)
				{
					Blur(buffer, glyph.Size.X, glyph.Size.Y, glyph.Size.X, blurAmount);
				}

				for (var i = 0; i < bufferSize; ++i)
				{
					var ci = i * 4;
					var c = buffer[i];

					if (premultiplyAlpha)
					{
						colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = colorBuffer[ci + 3] = c;
					}
					else
					{
						colorBuffer[ci] = colorBuffer[ci + 1] = colorBuffer[ci + 2] = 255;
						colorBuffer[ci + 3] = c;
					}
				}
			}

#if MONOGAME || FNA || STRIDE
			// Write to texture
			if (Texture == null)
			{
				Texture = Texture2DManager.CreateTexture(graphicsDevice, Width, Height);
			}

			Texture2DManager.SetTextureData(Texture, glyph.TextureRectangle, colorBuffer);
#else
			// Write to texture
			if (Texture == null)
			{
				Texture = textureManager.CreateTexture(Width, Height);
			}

			textureManager.SetTextureData(Texture, glyph.TextureRectangle, colorBuffer);
#endif
		}

		void Blur(byte[] dst, int w, int h, int dstStride, int blur)
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

		static void BlurCols(byte[] dst, int w, int h, int dstStride, int alpha)
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

		static void BlurRows(byte[] dst, int w, int h, int dstStride, int alpha)
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
					dst[index +y] = (byte)(z >> 7);
				}

				dst[index +(h - 1) * dstStride] = 0;
				z = 0;
				for (y = (h - 2) * dstStride; y >= 0; y -= dstStride)
				{
					z += (alpha * ((dst[index +y] << 7) - z)) >> 16;
					dst[index +y] = (byte)(z >> 7);
				}

				dst[index] = 0;
				++index;
			}
		}
	}
}

using FontStashSharp.Interfaces;
using System;

#if MONOGAME || FNA || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Graphics;
using Stride.Core.Mathematics;
using Texture2D = Stride.Graphics.Texture;
#else
using Texture2D = System.Object;
using System.Drawing;
#endif


namespace FontStashSharp
{
	/// <summary>
	/// Manages a texture atlas for storing rendered glyphs using a skyline bin-packing algorithm.
	/// </summary>
	public class FontAtlas
	{
		byte[] _byteBuffer;
		byte[] _colorBuffer;

		/// <summary>
		/// Gets the width of the atlas texture in pixels.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the height of the atlas texture in pixels.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Gets the number of active nodes in the skyline.
		/// </summary>
		public int NodesNumber { get; private set; }

		/// <summary>
		/// Gets the array of skyline nodes.
		/// </summary>
		internal FontAtlasNode[] Nodes { get; private set; }

		/// <summary>
		/// Gets or sets the texture for this atlas.
		/// </summary>
		public Texture2D Texture { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FontAtlas"/> class.
		/// </summary>
		/// <param name="w">The width of the atlas in pixels.</param>
		/// <param name="h">The height of the atlas in pixels.</param>
		/// <param name="count">The initial capacity of the skyline node array.</param>
		/// <param name="texture">An optional existing texture to use for this atlas.</param>
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

		/// <summary>
		/// Inserts a node at the specified index in the skyline.
		/// </summary>
		/// <param name="idx">The index where the node will be inserted.</param>
		/// <param name="x">The X coordinate of the node.</param>
		/// <param name="y">The Y coordinate of the node.</param>
		/// <param name="w">The width of the node.</param>
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

		/// <summary>
		/// Removes the node at the specified index from the skyline.
		/// </summary>
		/// <param name="idx">The index of the node to remove.</param>
		public void RemoveNode(int idx)
		{
			if (NodesNumber == 0)
				return;
			for (var i = idx; i < NodesNumber - 1; i++)
				Nodes[i] = Nodes[i + 1];
			NodesNumber--;
		}

		/// <summary>
		/// Resets the atlas to the specified dimensions.
		/// </summary>
		/// <param name="w">The new width in pixels.</param>
		/// <param name="h">The new height in pixels.</param>
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

		/// <summary>
		/// Adds a skyline level to the atlas.
		/// </summary>
		/// <param name="idx">The index where the level will be inserted.</param>
		/// <param name="x">The X coordinate of the level.</param>
		/// <param name="y">The Y coordinate of the level.</param>
		/// <param name="w">The width of the level.</param>
		/// <param name="h">The height of the level.</param>
		/// <returns>True if the level was added successfully.</returns>
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

		/// <summary>
		/// Checks if a rectangle of the specified dimensions fits in the skyline.
		/// </summary>
		/// <param name="i">The starting node index.</param>
		/// <param name="w">The width of the rectangle.</param>
		/// <param name="h">The height of the rectangle.</param>
		/// <returns>The Y coordinate if the rectangle fits, -1 otherwise.</returns>
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

		/// <summary>
		/// Attempts to add a rectangle to the atlas.
		/// </summary>
		/// <param name="rw">The width of the rectangle.</param>
		/// <param name="rh">The height of the rectangle.</param>
		/// <param name="rx">The X coordinate of the placed rectangle (output).</param>
		/// <param name="ry">The Y coordinate of the placed rectangle (output).</param>
		/// <returns>True if the rectangle was placed successfully, false if the atlas is full.</returns>
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

#if MONOGAME || FNA || XNA || STRIDE
		public void RenderGlyph(GraphicsDevice graphicsDevice, DynamicFontGlyph glyph, IFontSource fontSource, GlyphRenderer glyphRenderer, GlyphRenderResult glyphRenderResult, int kernelWidth, int kernelHeight, FontRasterizationMode mode)
#else
		/// <summary>
		/// Renders a glyph and stores it in the atlas texture.
		/// </summary>
		/// <param name="textureManager">The texture manager.</param>
		/// <param name="glyph">The glyph to render.</param>
		/// <param name="fontSource">The font source for rasterization.</param>
		/// <param name="glyphRenderer">The glyph rendering function.</param>
		/// <param name="glyphRenderResult">The glyph render result format.</param>
		/// <param name="kernelWidth">The kernel width for rendering.</param>
		/// <param name="kernelHeight">The kernel height for rendering.</param>
		public void RenderGlyph(ITexture2DManager textureManager, DynamicFontGlyph glyph, IFontSource fontSource, GlyphRenderer glyphRenderer, GlyphRenderResult glyphRenderResult, int kernelWidth, int kernelHeight)
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
			var colorBufferSize = (glyph.Size.X + FontSystem.GlyphPad * 2) * (glyph.Size.Y + FontSystem.GlyphPad * 2) * 4;
			if ((colorBuffer == null) || (colorBuffer.Length < colorBufferSize))
			{
				colorBuffer = new byte[colorBufferSize];
				_colorBuffer = colorBuffer;
			}

			// Create the atlas texture if required
			if (Texture == null)
			{
#if MONOGAME || FNA || XNA || STRIDE
				Texture = Texture2DManager.CreateTexture(graphicsDevice, Width, Height);
#else
				Texture = textureManager.CreateTexture(Width, Height);
#endif
			}

			// Erase an area where we are going to place a glyph
			Array.Clear(colorBuffer, 0, colorBufferSize);
			var eraseArea = glyph.TextureRectangle;
			eraseArea.X = Math.Max(eraseArea.X - FontSystem.GlyphPad, 0);
			eraseArea.Y = Math.Max(eraseArea.Y - FontSystem.GlyphPad, 0);
			eraseArea.Width += FontSystem.GlyphPad * 2;
			if (eraseArea.Right > Width)
			{
				eraseArea.Width = Width - eraseArea.X;
			}
			eraseArea.Height += FontSystem.GlyphPad * 2;
			if (eraseArea.Bottom > Height)
			{
				eraseArea.Height = Height - eraseArea.Y;
			}

#if MONOGAME || FNA || XNA || STRIDE
			Texture2DManager.SetTextureData(Texture, eraseArea, colorBuffer);
#else
			textureManager.SetTextureData(Texture, eraseArea, colorBuffer);
#endif

			fontSource.RasterizeGlyphBitmap(mode,
				glyph.Id,
				glyph.FontSize,
				buffer,
				glyph.EffectAmount + glyph.EffectAmount * glyph.Size.X,
				glyph.Size.X - glyph.EffectAmount * 2,
				glyph.Size.Y - glyph.EffectAmount * 2,
				glyph.Size.X);

			var glyphRenderOptions = new GlyphRenderOptions
			{
				Effect = glyph.Effect,
				EffectAmount = glyph.EffectAmount,
				Size = glyph.Size,
				GlyphRenderResult = glyphRenderResult
			};

			glyphRenderer(buffer, colorBuffer, glyphRenderOptions);

			// Render glyph to texture
#if MONOGAME || FNA || XNA || STRIDE
			Texture2DManager.SetTextureData(Texture, glyph.TextureRectangle, colorBuffer);
#else
			textureManager.SetTextureData(Texture, glyph.TextureRectangle, colorBuffer);
#endif
		}
	}
}
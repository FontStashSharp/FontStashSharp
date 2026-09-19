using FontStashSharp.Interfaces;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Drawing;
using System.Numerics;
using Rectangle = System.Drawing.Rectangle;

namespace FontStashSharp.Platform
{
	internal sealed class VertexBatch : IDisposable
	{
		private const int MAX_SPRITES = 2048;
		private const int MAX_VERTICES = MAX_SPRITES * 4;
		private const int MAX_INDICES = MAX_SPRITES * 6;

		private readonly BufferObject<VertexPositionColorTexture> _vertexBuffer;
		private readonly BufferObject<short> _indexBuffer;
		private readonly VertexArrayObject _vao;
		private readonly VertexPositionColorTexture[] _vertexData = new VertexPositionColorTexture[MAX_VERTICES];
		private static readonly short[] indexData = GenerateIndexArray();
		private object _lastTexture;
		private int _vertexIndex = 0;

		public Silk.NET.Maths.Rectangle<int> Viewport { get; set; } = new Silk.NET.Maths.Rectangle<int>(0, 0, 1200, 800);

		public unsafe VertexBatch()
		{
			_vertexBuffer = new BufferObject<VertexPositionColorTexture>(MAX_VERTICES, BufferTargetARB.ArrayBuffer, true);
			_indexBuffer = new BufferObject<short>(indexData.Length, BufferTargetARB.ElementArrayBuffer, false);
			_indexBuffer.SetData(indexData, 0, indexData.Length);

			// Attribute locations are bound to 0/1/2 in Shader, so a single
			// vertex layout is shared by the plain shader and all SDF variants.
			_vao = new VertexArrayObject(sizeof(VertexPositionColorTexture));
			_vao.Bind();
			_vao.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0);
			_vao.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 12);
			_vao.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 16);
		}

		~VertexBatch() => Dispose(false);

		public void Dispose() => Dispose(true);

		private void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_vao.Dispose();
			_vertexBuffer.Dispose();
			_indexBuffer.Dispose();
		}

		public void Begin()
		{
			Env.Gl.Disable(EnableCap.DepthTest);
			GLUtility.CheckError();
			Env.Gl.Enable(EnableCap.Blend);
			GLUtility.CheckError();

			_vao.Bind();
			_indexBuffer.Bind();
			_vertexBuffer.Bind();
		}

		public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
		{
			if (_lastTexture != texture || _vertexIndex + 4 > MAX_VERTICES)
			{
				FlushBuffer();
			}

			_vertexData[_vertexIndex++] = topLeft;
			_vertexData[_vertexIndex++] = topRight;
			_vertexData[_vertexIndex++] = bottomLeft;
			_vertexData[_vertexIndex++] = bottomRight;

			_lastTexture = texture;
		}

		public void Draw(object texture, Vector2 position, Vector2 size, FSColor color)
		{
			var topLeft = new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0), color, new Vector2(0, 0));
			var topRight = new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y, 0), color, new Vector2(1, 0));
			var bottomLeft = new VertexPositionColorTexture(new Vector3(position.X, position.Y + size.Y, 0), color, new Vector2(0, 1));
			var bottomRight = new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, 0), color, new Vector2(1, 1));

			DrawQuad(texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
		}

		public void Draw(object texture, Vector2 pos, Rectangle? src, FSColor color, float rotation, Vector2 scale, float depth)
		{
			var tex = (Texture)texture;
			var r = src ?? new Rectangle(0, 0, tex.Width, tex.Height);

			float cos = 1.0f, sin = 0.0f;
			if (rotation != 0)
			{
				cos = (float)Math.Cos(rotation);
				sin = (float)Math.Sin(rotation);
			}

			var w = r.Width;
			var h = r.Height;

			var topLeft = CreateVertex(pos, color, r, tex, depth, rotation, scale, cos, sin, 0, 0);
			var topRight = CreateVertex(pos, color, r, tex, depth, rotation, scale, cos, sin, w, 0);
			var bottomLeft = CreateVertex(pos, color, r, tex, depth, rotation, scale, cos, sin, 0, h);
			var bottomRight = CreateVertex(pos, color, r, tex, depth, rotation, scale, cos, sin, w, h);

			DrawQuad(texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
		}

		private static VertexPositionColorTexture CreateVertex(Vector2 pos, FSColor color, Rectangle r, Texture tex, float depth, float rotation, Vector2 scale, float cos, float sin, float dx, float dy)
		{
			// Mirrors MonoGame SpriteBatch.Draw: scale first, then rotate (origin 0,0).
			var sx = dx * scale.X;
			var sy = dy * scale.Y;

			float ox, oy;
			if (rotation == 0)
			{
				ox = sx;
				oy = sy;
			}
			else
			{
				ox = sx * cos - sy * sin;
				oy = sx * sin + sy * cos;
			}

			return new VertexPositionColorTexture(
				new Vector3(pos.X + ox, pos.Y + oy, depth),
				color,
				// UV normalized against atlas size; only alpha carries the SDF distance.
				new Vector2((r.X + dx) / tex.Width, (r.Y + dy) / tex.Height));
		}

		public void End()
		{
			FlushBuffer();
		}

		public unsafe void FlushBuffer()
		{
			if (_vertexIndex == 0 || _lastTexture == null)
			{
				return;
			}

			_vertexBuffer.SetData(_vertexData, 0, _vertexIndex);

			var texture = (Texture)_lastTexture;
			texture.Bind();

			Env.Gl.DrawElements(PrimitiveType.Triangles, (uint)(_vertexIndex * 6 / 4), DrawElementsType.UnsignedShort, null);
			_vertexIndex = 0;
		}

		private static short[] GenerateIndexArray()
		{
			short[] result = new short[MAX_INDICES];
			for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
			{
				result[i] = (short)(j);
				result[i + 1] = (short)(j + 1);
				result[i + 2] = (short)(j + 2);
				result[i + 3] = (short)(j + 3);
				result[i + 4] = (short)(j + 2);
				result[i + 5] = (short)(j + 1);
			}
			return result;
		}
	}
}
using FontStashSharp.Interfaces;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace FontStashSharp.Platform
{
	internal class Renderer : IFontStashRenderer2, IDisposable
	{
		private readonly VertexBatch _vertexBatch = new VertexBatch();
		private readonly Shader _shader;
		private readonly Texture2DManager _textureManager;

		public ITexture2DManager TextureManager => _textureManager;

		public Rectangle<int> Viewport
		{
			get => _vertexBatch.Viewport;
			set => _vertexBatch.Viewport = value;
		}

		public Renderer()
		{
			_textureManager = new Texture2DManager();

			_shader = new Shader(@"Assets/Shaders/shader.vert", @"Assets/Shaders/shader.frag");
		}

		~Renderer() => Dispose(false);
		public void Dispose() => Dispose(true);

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_shader.Dispose();
			_vertexBatch.Dispose();
		}

		public void Begin()
		{
			_vertexBatch.Begin();

			Env.Gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
			GLUtility.CheckError();

			_shader.Use();
			_shader.SetUniform("TextureSampler", 0);

			var transform = Matrix4x4.CreateOrthographicOffCenter(
				Viewport.Origin.X, Viewport.Origin.X + Viewport.Size.X,
				Viewport.Origin.Y + Viewport.Size.Y, Viewport.Origin.Y,
				0, -1);
			_shader.SetUniform("MatrixTransform", transform);
		}

		public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
		{
			_vertexBatch.DrawQuad(texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
		}

		public void Draw(object texture, Vector2 position, Vector2 size, FSColor color)
		{
			_vertexBatch.Draw(texture, position, size, color);
		}

		public void End()
		{
			_vertexBatch.End();
		}
	}
}
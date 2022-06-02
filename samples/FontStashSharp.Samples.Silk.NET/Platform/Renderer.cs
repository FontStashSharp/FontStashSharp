using FontStashSharp.Interfaces;
using Silk.NET.OpenGL;
using Tutorial;
using Shader = Tutorial.Shader;

namespace FontStashSharp.Platform
{
	internal class Renderer : IFontStashRenderer2
	{
		private const int MAX_SPRITES = 2048;
		private const int MAX_VERTICES = MAX_SPRITES * 4;
		private const int MAX_INDICES = MAX_SPRITES * 6;

		private readonly BufferObject<float> Vbo;
		private readonly BufferObject<uint> Ebo;
		private readonly VertexArrayObject<float, uint> Vao;

		//Create a texture object.
		private readonly Shader Shader;

		private readonly Texture2DManager _textureManager;

		public ITexture2DManager TextureManager => _textureManager;

		public object Texture { set => throw new System.NotImplementedException(); }

		public GL Gl => _textureManager.Gl;

		public Renderer(GL gl)
		{
			_textureManager = new Texture2DManager(gl);

			Ebo = new BufferObject<uint>(Gl, MAX_VERTICES, BufferTargetARB.ElementArrayBuffer, true);
			Vbo = new BufferObject<float>(Gl, MAX_INDICES, BufferTargetARB.ArrayBuffer, false);
			Vao = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);

			Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
			Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

			Shader = new Shader(Gl, "shader.vert", "shader.frag");
		}

		public void AddVertex(VertexPositionColorTexture vertex)
		{
			throw new System.NotImplementedException();
		}
	}
}

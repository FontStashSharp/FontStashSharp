using FontStashSharp.Interfaces;
using System.Drawing;
using System.Numerics;
using TrippyGL;

namespace FontStashSharp.Platform
{
	internal class Renderer : IFontStashRenderer
	{
		private readonly SimpleShaderProgram _shaderProgram;
		private readonly TextureBatcher _batch;
		private readonly Texture2DManager _textureManager;

		public ITexture2DManager TextureManager => _textureManager;

		public GraphicsDevice GraphicsDevice => _textureManager.GraphicsDevice;

		public Renderer(GraphicsDevice graphicsDevice)
		{
			_textureManager = new Texture2DManager(graphicsDevice);

			_shaderProgram = SimpleShaderProgram.Create<VertexColorTexture>(graphicsDevice, 0, 0, true);
			_batch = new TextureBatcher(graphicsDevice);
			_batch.SetShaderProgram(_shaderProgram);
			OnViewportChanged();
		}

		public void OnViewportChanged()
		{
			_shaderProgram.Projection = Matrix4x4.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
		}


		public void Begin() => _batch.Begin();

		public void End() => _batch.End();

		public void Draw(object texture, Vector2 pos, Rectangle? src, FSColor color, float rotation, Vector2 scale, float depth)
		{
			var tex = (Texture2D)texture;

			_batch.Draw(tex,
				pos,
				src,
				color.ToTrippy(),
				scale,
				rotation,
				Vector2.Zero,
				depth);
		}
	}
}

using Silk.NET.OpenGL;
using System;
using System.Drawing;

namespace Tutorial
{
	public unsafe class Texture : IDisposable
	{
		private readonly uint _handle;
		private readonly GL _gl;

		public readonly int Width;
		public readonly int Height;

		public Texture(GL gl, int width, int height)
		{
			_gl = gl;
			Width = width;
			Height = height;

			_handle = _gl.GenTexture();
			Bind();

			//Reserve enough memory from the gpu for the whole image
			gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

			SetParameters();
		}

		private void SetParameters()
		{
			//Setting some texture perameters so the texture behaves as expected.
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
			_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

			//Generating mipmaps.
			_gl.GenerateMipmap(TextureTarget.Texture2D);
		}

		public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
		{
			//When we bind a texture we can choose which textureslot we can bind it to.
			_gl.ActiveTexture(textureSlot);
			_gl.BindTexture(TextureTarget.Texture2D, _handle);
		}

		public void Dispose()
		{
			//In order to dispose we need to delete the opengl handle for the texure.
			_gl.DeleteTexture(_handle);
		}

		public void SetData(Rectangle bounds, byte[] data)
		{
			fixed (byte* ptr = data)
			{
				_gl.TexSubImage2D(
					target: TextureTarget.Texture2D,
					level: 0,
					xoffset: bounds.Left,
					yoffset: bounds.Top,
					width: (uint)bounds.Width,
					height: (uint)bounds.Height,
					format: PixelFormat.Rgba,
					type: PixelType.UnsignedByte,
					pixels: ptr
				);
			}
		}
	}
}
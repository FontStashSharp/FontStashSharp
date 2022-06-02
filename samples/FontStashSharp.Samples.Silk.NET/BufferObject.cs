using Silk.NET.OpenGL;
using System;

namespace Tutorial
{
	public class BufferObject<TDataType> : IDisposable
			where TDataType : unmanaged
	{
		private uint _handle;
		private BufferTargetARB _bufferType;
		private GL _gl;

		public unsafe BufferObject(GL gl, int size, BufferTargetARB bufferType, bool isDynamic)
		{
			_gl = gl;
			_bufferType = bufferType;

			_handle = _gl.GenBuffer();
			Bind();
			_gl.BufferData(bufferType, (nuint)(size * sizeof(TDataType)), null, isDynamic ? BufferUsageARB.StreamDraw : BufferUsageARB.StaticDraw);
		}

		public void Bind()
		{
			_gl.BindBuffer(_bufferType, _handle);
		}

		public void Dispose()
		{
			_gl.DeleteBuffer(_handle);
		}
	}
}
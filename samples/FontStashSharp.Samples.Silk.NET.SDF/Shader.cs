using System;
using System.IO;
using System.Numerics;
using System.Text;
using Silk.NET.OpenGL;

namespace FontStashSharp
{
	public class Shader : IDisposable
	{
		private uint _handle;

		public Shader(string vertexPath, string fragmentPath, params string[] defines)
		{
			uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
			uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath, defines);
			_handle = Env.Gl.CreateProgram();
			GLUtility.CheckError();

			Env.Gl.AttachShader(_handle, vertex);
			GLUtility.CheckError();

			Env.Gl.AttachShader(_handle, fragment);
			GLUtility.CheckError();

			// Bind attribute locations explicitly so that all program variants
			// (and the plain sprite shader) share the same vertex layout
			Env.Gl.BindAttribLocation(_handle, 0, "a_position");
			Env.Gl.BindAttribLocation(_handle, 1, "a_color");
			Env.Gl.BindAttribLocation(_handle, 2, "a_texCoords0");

			Env.Gl.LinkProgram(_handle);
			Env.Gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
			if (status == 0)
			{
				throw new Exception($"Program failed to link with error: {Env.Gl.GetProgramInfoLog(_handle)}");
			}

			Env.Gl.DetachShader(_handle, vertex);
			Env.Gl.DetachShader(_handle, fragment);

			Env.Gl.DeleteShader(vertex);
			Env.Gl.DeleteShader(fragment);
		}

		public void Use()
		{
			Env.Gl.UseProgram(_handle);
			GLUtility.CheckError();
		}

		public void SetUniform(string name, int value)
		{
			int location = Env.Gl.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			Env.Gl.Uniform1(location, value);
			GLUtility.CheckError();
		}

		public void SetUniform(string name, float value)
		{
			int location = Env.Gl.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			Env.Gl.Uniform1(location, value);
			GLUtility.CheckError();
		}

		public unsafe void SetUniform(string name, Vector2 value)
		{
			int location = Env.Gl.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			Env.Gl.Uniform2(location, value.X, value.Y);
			GLUtility.CheckError();
		}

		public unsafe void SetUniform(string name, Vector4 value)
		{
			int location = Env.Gl.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}
			Env.Gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
			GLUtility.CheckError();
		}

		public unsafe void SetUniform(string name, Matrix4x4 value)
		{
			int location = Env.Gl.GetUniformLocation(_handle, name);
			if (location == -1)
			{
				throw new Exception($"{name} uniform not found on shader.");
			}

			Env.Gl.UniformMatrix4(location, 1, false, (float*)&value);
			GLUtility.CheckError();
		}

		public void Dispose()
		{
			Env.Gl.DeleteProgram(_handle);
		}

		private uint LoadShader(ShaderType type, string path, params string[] defines)
		{
			string src = File.ReadAllText(path);

			if (defines != null && defines.Length > 0)
			{
				var sb = new StringBuilder();
				foreach (var d in defines)
				{
					sb.Append("#define ");
					sb.Append(d);
					sb.AppendLine();
				}
				sb.AppendLine(src);
				src = sb.ToString();
			}

			uint handle = Env.Gl.CreateShader(type);
			GLUtility.CheckError();

			Env.Gl.ShaderSource(handle, src);
			GLUtility.CheckError();

			Env.Gl.CompileShader(handle);
			string infoLog = Env.Gl.GetShaderInfoLog(handle);
			if (!string.IsNullOrWhiteSpace(infoLog))
			{
				throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
			}

			return handle;
		}

		public int GetAttribLocation(string attribName)
		{
			var result = Env.Gl.GetAttribLocation(_handle, attribName);
			GLUtility.CheckError();
			return result;
		}
	}
}
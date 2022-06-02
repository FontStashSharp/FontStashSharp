using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using FontStashSharp.Platform;

namespace Tutorial
{
	class Program
	{
		private static IWindow window;
		private static GL Gl;
		private static Renderer renderer;

		private static void Main(string[] args)
		{
			var options = WindowOptions.Default;
			options.Size = new Vector2D<int>(800, 600);
			options.Title = "LearnOpenGL with Silk.NET";
			window = Window.Create(options);

			window.Load += OnLoad;
			window.Render += OnRender;
			window.Closing += OnClose;

			window.Run();
		}


		private unsafe static void OnLoad()
		{
			IInputContext input = window.CreateInput();
			for (int i = 0; i < input.Keyboards.Count; i++)
			{
				input.Keyboards[i].KeyDown += KeyDown;
			}

			Gl = GL.GetApi(window);
			renderer = new Renderer(Gl);
		}

		private static unsafe void OnRender(double obj)
		{
			Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

			Vao.Bind();
			Shader.Use();
			//Bind a texture and and set the uTexture0 to use texture0.
			Shader.SetUniform("uTexture0", 0);

			Gl.DrawElements(PrimitiveType.Triangles, (uint)Indices.Length, DrawElementsType.UnsignedInt, null);
		}

		private static void OnClose()
		{
			Vbo.Dispose();
			Ebo.Dispose();
			Vao.Dispose();
			Shader.Dispose();
			//Remember to dispose the texture.
			Texture.Dispose();
		}

		private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
		{
			if (arg2 == Key.Escape)
			{
				window.Close();
			}
		}
	}
}
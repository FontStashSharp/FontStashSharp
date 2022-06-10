using Silk.NET.OpenGL;
using System;

namespace FontStashSharp
{
	internal static class GLUtility
	{
		public static void CheckError()
		{
			var error = (ErrorCode)Env.Gl.GetError();
			if (error != ErrorCode.NoError)
				throw new Exception("GL.GetError() returned " + error.ToString());
		}
	}
}

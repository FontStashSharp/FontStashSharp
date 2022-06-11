using OpenTK.Graphics.OpenGL4;
using System;

namespace FontStashSharp
{
	internal static class GLUtility
	{
		public static void CheckError()
		{
			var error = GL.GetError();
			if (error != ErrorCode.NoError)
				throw new Exception("GL.GetError() returned " + error.ToString());
		}
	}
}

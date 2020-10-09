using Microsoft.Xna.Framework.Input;
using System;

namespace FontStashSharp.Samples.MonoGame
{
	public static class KeyboardUtils
	{
		private static bool _beginCalled = false;
		private static KeyboardState _state, _oldState;

		public static void Begin()
		{
			if (_beginCalled)
			{
				throw new Exception("Begin was already called");
			}

			_state = Keyboard.GetState();
			_beginCalled = true;
		}

		public static bool IsPressed(Keys key)
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			return _state.IsKeyDown(key) && !_oldState.IsKeyDown(key);
		}

		public static void End()
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasnt called");
			}

			_oldState = _state;
			_beginCalled = false;
		}
	}
}

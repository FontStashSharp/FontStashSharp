using System;

#if MONOGAME || FNA
using Microsoft.Xna.Framework.Input;
#elif STRIDE
using Stride.Input;
using System.Collections.Generic;
using Stride.Core.Collections;
#endif

namespace FontStashSharp.Samples
{
	public static class KeyboardUtils
	{
		private static bool _beginCalled = false;

#if MONOGAME || FNA
		private static KeyboardState _state, _oldState;
#elif STRIDE
		private static Stride.Core.Collections.IReadOnlySet<Keys> _state;
		private static readonly HashSet<Keys> _oldState = new HashSet<Keys>();
#endif

		public static void Begin()
		{
			if (_beginCalled)
			{
				throw new Exception("Begin was already called");
			}


#if MONOGAME || FNA
			_state = Keyboard.GetState();
#elif STRIDE
			_state = Game1.Instance.Input.DownKeys;
#endif

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

#if MONOGAME || FNA
			_oldState = _state;
#elif STRIDE
			_oldState.Clear();
			if (_state != null)
			{
				foreach (var key in _state)
				{
					_oldState.Add(key);
				}
			}
#endif
			_beginCalled = false;
		}

#if STRIDE
		private static bool IsKeyDown(this Stride.Core.Collections.IReadOnlySet<Keys> keysDown, Keys key)
		{
			return keysDown.Contains(key);
		}

		private static bool IsKeyDown(this HashSet<Keys> keysDown, Keys key)
		{
			return keysDown.Contains(key);
		}
#endif
	}
}

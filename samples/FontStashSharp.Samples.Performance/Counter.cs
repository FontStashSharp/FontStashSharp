using System.Collections.Generic;
using System.Diagnostics;

namespace FontStashSharp
{
	internal class Counter
	{
		private static Stopwatch _watch = new Stopwatch();
		private readonly List<float> _values = new List<float>();

		public float Last { get; private set; }
		public float Total { get; private set; }

		public void Start()
		{
			_watch.Restart();
		}

		public void Stop()
		{
			_watch.Stop();

			Last = (float)_watch.Elapsed.TotalMilliseconds;
			Total += Last;
			_values.Add(Last);

			// Remove values from the start until there are 200 values
			while (_values.Count > 200)
			{
				Total -= _values[0];

				_values.RemoveAt(0);
			}
		}
	}
}

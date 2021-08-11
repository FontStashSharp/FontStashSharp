using FontStashSharp.Interfaces;
using System;

namespace FontStashSharp
{
	internal sealed class FontMetrics
	{
		public int Ascent { get; private set; }
		public int Descent { get; private set; }
		public int LineHeight { get; private set; }

		public FontMetrics(int ascent, int descent, int lineHeight)
		{
			Ascent = ascent;
			Descent = descent;
			LineHeight = lineHeight;
		}
	}

	internal sealed class FontSourceWrapper : IDisposable
	{
		private IFontSource _fontSource;
		private readonly Int32Map<FontMetrics> _metricsCache = new Int32Map<FontMetrics>();

		public IFontSource Source => _fontSource;

		public FontSourceWrapper(IFontSource fontSource)
		{
			_fontSource = fontSource ?? throw new ArgumentNullException(nameof(fontSource));
		}

		~FontSourceWrapper()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (disposing && _fontSource != null)
			{
				_fontSource.Dispose();
				_fontSource = null;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public FontMetrics GetMetrics(int fontSize)
		{
			FontMetrics metrics;
			if (_metricsCache.TryGetValue(fontSize, out metrics))
			{
				return metrics;
			}

			int ascent, descent, lineHeight;
			_fontSource.GetMetricsForSize(fontSize, out ascent, out descent, out lineHeight);
			metrics = new FontMetrics(ascent, descent, lineHeight);
			_metricsCache[fontSize] = metrics;

			return metrics;
		}
	}
}
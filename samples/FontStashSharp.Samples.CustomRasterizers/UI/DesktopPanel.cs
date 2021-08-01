using FontStashSharp.Interfaces;
using Myra;
using System;
using System.IO;

namespace FontStashSharp.Samples.UI
{
	public partial class DesktopPanel
	{
		private FontSystem _fontSystem;
		private Func<Stream> _streamOpener = () => DefaultAssets.OpenDefaultFontDataStream();
		private IFontLoader _fontLoader;
		private float _fontResolutionFactor = 1.0f;
		public int _kernelWidth, _kernelHeight;

		public Func<Stream> StreamOpener
		{
			get => _streamOpener;
			set
			{
				if (value == _streamOpener)
				{
					return;
				}

				_streamOpener = value;
				UpdateFontSystem();
			}
		}

		public IFontLoader FontLoader
		{
			get => _fontLoader;
			set
			{
				if (value == _fontLoader)
				{
					return;
				}

				_fontLoader = value;
				UpdateFontSystem();
			}
		}

		public float FontResolutionFactor
		{
			get => _fontResolutionFactor;
			set
			{
				if (value == _fontResolutionFactor)
				{
					return;
				}

				_fontResolutionFactor = value;
				UpdateFontSystem();
			}
		}

		public int KernelWidth
		{
			get => _kernelWidth;
			set
			{
				if (value == _kernelWidth)
				{
					return;
				}

				_kernelWidth = value;
				UpdateFontSystem();
			}
		}

		public int KernelHeight
		{
			get => _kernelHeight;
			set
			{
				if (value == _kernelHeight)
				{
					return;
				}

				_kernelHeight = value;
				UpdateFontSystem();
			}
		}

		public int FontSize
		{
			get => _labelText.Font.FontSize;
			set => _labelText.Font = _fontSystem.GetFont(value);
		}

		public string Text
		{
			get => _labelText.Text;
			set => _labelText.Text = value;
		}

		public DesktopPanel()
		{
			BuildUI();
			UpdateFontSystem();
		}

		private FontSystem CreateFontSystem()
		{
			var settings = new FontSystemSettings
			{
				FontResolutionFactor = _fontResolutionFactor,
				KernelWidth = _kernelWidth,
				KernelHeight = _kernelHeight
			};

			var result = _fontLoader != null ? new FontSystem(_fontLoader, settings) : new FontSystem(settings);

			byte[] data;

			using (var stream = _streamOpener())
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				data = ms.ToArray();
			}

			result.AddFont(data);

			return result;
		}

		private void UpdateFontSystem()
		{
			var fontSize = FontSize;
			_fontSystem = CreateFontSystem();
			_labelHeader.Font = _fontSystem.GetFont(20);
			_labelText.Font = _fontSystem.GetFont(fontSize);
		}
	}
}
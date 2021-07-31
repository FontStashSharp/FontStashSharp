using FontStashSharp.Samples.UI;
using FontStashSharp.Interfaces;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace FontStashSharp.Samples
{
	public class TopWidget: HorizontalStackPanel
	{
		private readonly List<DesktopPanel> _panels = new List<DesktopPanel>();

		public void AddFontSystem(string title, Func<Stream> streamOpener, IFontLoader fontLoader)
		{
			if (streamOpener == null)
			{
				throw new ArgumentNullException(nameof(streamOpener));
			}

			var panel = new DesktopPanel
			{
				StreamOpener = streamOpener,
				FontLoader = fontLoader
			};

			panel._labelHeader.Text = title;

			if (Widgets.Count > 0)
			{
				Proportions.Add(new Proportion(ProportionType.Auto));
				Widgets.Add(new VerticalSeparator());
			}

			Proportions.Add(new Proportion(ProportionType.Auto));
			Widgets.Add(panel);
			_panels.Add(panel);
		}

		public void SetStreamOpener(Func<Stream> opener)
		{
			foreach (var panel in _panels)
			{
				panel.StreamOpener = opener;
			}
		}

		public void SetFontResolutionFactor(float value)
		{
			foreach (var panel in _panels)
			{
				panel.FontResolutionFactor = value;
			}
		}

		public void SetKernelWidth(int value)
		{
			foreach (var panel in _panels)
			{
				panel.KernelWidth = value;
			}
		}

		public void SetKernelHeight(int value)
		{
			foreach (var panel in _panels)
			{
				panel.KernelHeight = value;
			}
		}

		public void SetText(string text)
		{
			foreach (var panel in _panels)
			{
				panel.Text = text;
			}
		}

		public void SetFontSize(int size)
		{
			foreach (var panel in _panels)
			{
				panel.FontSize = size;
			}
		}
	}
}
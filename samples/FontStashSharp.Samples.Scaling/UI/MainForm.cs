/* Generated by MyraPad at 6/26/2021 3:37:37 PM */
using Microsoft.Xna.Framework.Graphics;

namespace FontStashSharp.Samples.UI
{
	public partial class MainForm
	{
		private TextWidget _top, _bottom;

		public MainForm(GraphicsDevice device)
		{
			BuildUI();

			_top = new TextWidget(device);
			_panelTopContainer.Widgets.Add(_top);

			_bottom = new TextWidget(device);
			_panelBottomContainer.Widgets.Add(_bottom);

			_spinButtonFontResolutionFactor.ValueChanged += (s, a) => Update();
			_spinButtonKernelWidth.ValueChanged += (s, a) => Update();
			_spinButtonKernelHeight.ValueChanged += (s, a) => Update();
			_spinButtonScale.ValueChanged += (s, a) => Update();

			Update();
		}

		private void Update()
		{
			_top.FontResolutionFactor = _spinButtonFontResolutionFactor.Value.Value;
			_top.KernelWidth = (int)_spinButtonKernelWidth.Value.Value;
			_top.KernelHeight = (int)_spinButtonKernelHeight.Value.Value;

			_top.Scale = _spinButtonScale.Value.Value;
			_bottom.Scale = _spinButtonScale.Value.Value;
		}
	}
}
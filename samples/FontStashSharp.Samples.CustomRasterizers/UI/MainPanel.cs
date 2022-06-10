using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using System;
using System.IO;

namespace FontStashSharp.Samples.UI
{
	public partial class MainPanel
	{
		public MainPanel()
		{
			BuildUI();

			_checkBoxSmoothText.PressedChanged += (s, a) =>
			{
				MyraEnvironment.SmoothText = _checkBoxSmoothText.IsChecked;
			};

			_textBoxText.TextChanged += (s, a) => TextRenderingGame.TopWidget.SetText(_textBoxText.Text);
			_sliderScale.ValueChanged += (s, a) =>
			{
				UpdateLabelScale();
				var scale = _sliderScale.Value;
				TextRenderingGame.TopDesktop.Scale = new Vector2(scale, scale);
			};
			_spinButtonFontSize.ValueChanged += (s, a) => TextRenderingGame.TopWidget.SetFontSize((int)_spinButtonFontSize.Value.Value);
			_spinButtonResolutionFactor.ValueChanged += (s, a) => TextRenderingGame.TopWidget.SetFontResolutionFactor((int)_spinButtonResolutionFactor.Value.Value);
			_spinButtonKernelWidth.ValueChanged += (s, a) => TextRenderingGame.TopWidget.SetKernelWidth((int)_spinButtonKernelWidth.Value.Value);
			_spinButtonKernelHeight.ValueChanged += (s, a) => TextRenderingGame.TopWidget.SetKernelHeight((int)_spinButtonKernelHeight.Value.Value);

			_buttonReset.Click += _buttonReset_Click;
			_buttonBrowseFont.Click += _buttonBrowseFont_Click;

			UpdateLabelScale();
		}

		private void UpdateLabelScale()
		{
			var scale = _sliderScale.Value;
			_labelScaleValue.Text = scale.ToString("0.00");
		}

		private void _buttonReset_Click(object sender, EventArgs e)
		{
			_sliderScale.Value = 1.0f;
			_spinButtonFontSize.Value = 32;
			_spinButtonResolutionFactor.Value = 1.0f;
			_spinButtonKernelWidth.Value = 0;
			_spinButtonKernelHeight.Value = 0;
			TextRenderingGame.TopWidget.SetStreamOpener(DefaultAssets.OpenDefaultFontDataStream);
			_textBoxFont.Text = "(default)";
		}

		private void _buttonBrowseFont_Click(object sender, EventArgs e)
		{
			var dialog = new FileDialog(FileDialogMode.OpenFile)
			{
				Filter = "*.ttf|*.otf|*.ttc"
			};

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					return;
				}

				try
				{
					var fontSystem = new FontSystem();
					fontSystem.AddFont(File.ReadAllBytes(dialog.FilePath));
					_textBoxFont.Text = dialog.FilePath;
					TextRenderingGame.TopWidget.SetStreamOpener(() => File.OpenRead(dialog.FilePath));
				}
				catch(Exception ex)
				{
					var messageBox = Dialog.CreateMessageBox("Error", ex.Message);
					messageBox.ShowModal(Desktop);
				}
			};

			dialog.ShowModal(Desktop);
		}
	}
}
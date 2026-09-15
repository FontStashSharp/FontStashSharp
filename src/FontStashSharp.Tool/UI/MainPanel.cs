using AssetManagementBase.Utility;
using FontStashSharp.RichText;
using FontStashSharp.Samples;
using Myra.Events;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using System;
using System.Globalization;
using System.IO;

namespace FontStashSharp.Tool.UI;

public partial class MainPanel
{
	private readonly TextRenderingWidget _widget;

	public MainPanel()
	{
		BuildUI();

		_widget = new TextRenderingWidget();
		_panelTextContainer.Widgets.Add(_widget);

		_text.TextChanged += (s, a) => Update();
		_sliderScale.ValueChanged += (s, a) => Update();
		_spinButtonFontSize.ValueChanged += (s, a) => Reload();

		_buttonResetFont.Click += _buttonResetFont_Click;
		_buttonBrowseFont.Click += _buttonBrowseFont_Click;

		_propertyGridTextSettings.PropertyChanged += (s, a) => Reload();
		_propertyGridTextSettings.Object = Settings.Instance;

		SetSplitterPosition(0, 0.75f);
		_panelTop.SetSplitterPosition(0, 0.75F);

		_buttonResetFont.DoClick();
	}

	private void _buttonResetFont_Click(object sender, MyraEventArgs e)
	{
		_textFontFile.Text = "(default)";
		_sliderScale.Value = 1.0f;
		_spinButtonFontSize.Value = 32;
		Reload();
	}

	private void _buttonBrowseFont_Click(object sender, MyraEventArgs e)
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
				var testFontSystem = new FontSystem();
				testFontSystem.AddFont(File.ReadAllBytes(dialog.FilePath));

				_textFontFile.Text = dialog.FilePath;

				Reload();
			}
			catch (Exception ex)
			{
				var messageBox = Dialog.CreateMessageBox("Error", ex.Message);
				messageBox.ShowModal(Desktop);
			}
		};


		dialog.ShowModal(Desktop);
	}

	private void Reload()
	{
		byte[] data;

		if (string.IsNullOrEmpty(_textFontFile.Text) || _textFontFile.Text == "(default)")
		{
			data = typeof(MainPanel).Assembly.ReadResourceAsBytes("FontStashSharp.Tool.Resources.Inter-Regular.ttf");
		}
		else
		{
			data = File.ReadAllBytes(_textFontFile.Text);
		}

		var fontSystemSettings = new FontSystemSettings();

		var settings = Settings.Instance;
		if (!settings.UseSDF)
		{
			fontSystemSettings.FontRasterizationMode = FontRasterizationMode.Standard;
			fontSystemSettings.FontResolutionFactor = settings.FontResolutionFactor;
			fontSystemSettings.KernelWidth = settings.KernelWidth;
			fontSystemSettings.KernelHeight = settings.KernelHeight;
		}
		else
		{
			fontSystemSettings.FontRasterizationMode = FontRasterizationMode.SDF;
		}

		RichTextDefaults.SDFShadowColor = settings.ShadowColor;
		RichTextDefaults.SDFShadowOffset = settings.ShadowOffset;

		var fontSystem = new FontSystem(fontSystemSettings);
		fontSystem.AddFont(data);

		_widget.Font = fontSystem.GetFont(_spinButtonFontSize.Value.Value);
		_widget.Background = new SolidBrush(settings.BackgroundColor);
	}

	private void Update()
	{
		_labelScale.Text = _sliderScale.Value.ToString("0.##", CultureInfo.InvariantCulture);
		_widget.TextScale = _sliderScale.Value;
		_widget.Text = _text.Text;
	}
}
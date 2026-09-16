using AssetManagementBase.Utility;
using FontStashSharp.RichText;
using FontStashSharp.Samples;
using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using System;
using System.Globalization;
using System.IO;

namespace FontStashSharp.Tool.UI;

public partial class MainPanel
{
	private readonly TextRenderingWidget _widget;
	private readonly Image _imageTexture;
	private readonly HorizontalSeparator _separatorImageTexture = new HorizontalSeparator();
	private bool _imageTextureDirty = true;

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

		_checkShowTexture.IsCheckedChanged += _checkShowTexture_IsCheckedChanged;

		_sliderScale.Value = 5.0f;

		_imageTexture = new Image();
		StackPanel.SetProportionType(_imageTexture, ProportionType.Part);
		StackPanel.SetProportionValue(_imageTexture, 1.0f);
	}

	private void _checkShowTexture_IsCheckedChanged(object sender, MyraEventArgs e)
	{
		if (_checkShowTexture.IsChecked && !_panelRight.Widgets.Contains(_imageTexture))
		{
			_panelRight.Widgets.Add(_separatorImageTexture);
			_panelRight.Widgets.Add(_imageTexture);
		}
		else if (!_checkShowTexture.IsChecked && _panelRight.Widgets.Contains(_imageTexture))
		{
			_panelRight.Widgets.Remove(_separatorImageTexture);
			_panelRight.Widgets.Remove(_imageTexture);
		}

		InvalidateImageTexture();
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
			fontSystemSettings.FixedSDFFontSize = settings.FixedFontSize;
		}

		RichTextDefaults.SDFShadowColor = settings.ShadowColor;
		RichTextDefaults.SDFShadowOffset = settings.ShadowOffset;
		RichTextDefaults.SDFStrokeColor = settings.StrokeColor;
		RichTextDefaults.SDFStrokeThickness = settings.StrokeThickness;

		var fontSystem = new FontSystem(fontSystemSettings);
		fontSystem.AddFont(data);

		_widget.Font = fontSystem.GetFont(_spinButtonFontSize.Value.Value);
		_widget.Background = new SolidBrush(settings.BackgroundColor);
		InvalidateImageTexture();
	}

	private void InvalidateImageTexture()
	{
		_imageTextureDirty = true;
	}

	private void UpdateImageTexture()
	{
		if (!_imageTextureDirty || _imageTexture == null)
		{
			return;
		}

		if (_checkShowTexture.IsChecked)
		{
			if (_widget.Font != null && _widget.Font.FontSystem.Atlases.Count > 0)
			{
				_imageTexture.Renderable = new TextureRegion(_widget.Font.FontSystem.Atlases[0].Texture);
				_imageTextureDirty = false;
			}
		}
		else
		{
			_imageTexture.Renderable = null;
			_imageTextureDirty = false;
		}
	}

	private void Update()
	{
		_labelScale.Text = _sliderScale.Value.ToString("0.##", CultureInfo.InvariantCulture);
		_widget.TextScale = _sliderScale.Value;
		_widget.Text = _text.Text;
	}

	public override void InternalRender(RenderContext context)
	{
		base.InternalRender(context);

		UpdateImageTexture();
	}
}
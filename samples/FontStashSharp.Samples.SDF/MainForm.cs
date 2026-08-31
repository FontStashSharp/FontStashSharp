namespace FontStashSharp.Samples;

public partial class MainForm
{
	private readonly TextRenderingWidget _textRendering;

	public MainForm()
	{
		BuildUI();

		_textRendering = new TextRenderingWidget();
		_panelTextContainer.Widgets.Add(_textRendering);

		_textFontSize.ValueChangedByUser += (s, e) => UpdateParameters();
		_sliderScale.ValueChangedByUser += (s, e) => UpdateParameters();
		_text.TextChangedByUser += (s, e) => UpdateParameters();

		_splitPaneTop.SetSplitterPosition(0, 0.75f);

		_propertyGridTextSettings.Object = SDFTextSettings.Default;
		_propertyGridTextSettings.PropertyChanged += (s, a) => _textRendering.SDFTextSettings = (SDFTextSettings)_propertyGridTextSettings.Object;

		UpdateParameters();
	}

	private void UpdateParameters()
	{
		_labelScale.Text = _sliderScale.Value.ToString();
		_textRendering.FontSize = _textFontSize.Value.Value;
		_textRendering.TextScale = _sliderScale.Value;
		_textRendering.Text = _text.Text;
	}
}
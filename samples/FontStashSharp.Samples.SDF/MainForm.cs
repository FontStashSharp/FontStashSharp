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
using Microsoft.Xna.Framework;

namespace FontStashSharp.Samples;

/// <summary>
/// Specifies the signed distance field (SDF) effects to apply when rendering text
/// </summary>
public enum SDFFontEffect
{
	/// <summary>
	/// Text is rendered without any special effect
	/// </summary>
	None,

	/// <summary>
	/// Text is rendered with a shadow
	/// </summary>
	Shadow,

	/// <summary>
	/// Text is rendered with an outline
	/// </summary>
	Stroked
}

/// <summary>
/// Settings that control how text is rendered with signed distance field (SDF) effects
/// </summary>
public struct SDFTextSettings
{
	/// <summary>
	/// Gets or sets whether supersampling is enabled for the SDF effect
	/// </summary>
	public bool EnableSuperSampling;

	/// <summary>
	/// Gets or sets the SDF effect to apply
	/// </summary>
	public SDFFontEffect Effect;

	/// <summary>
	/// Gets or sets the color of the shadow
	/// </summary>
	public Color ShadowColor;

	/// <summary>
	/// Gets or sets the offset of the shadow in pixels.
	/// </summary>
	public Point ShadowOffset;

	/// <summary>
	/// Gets or sets the color of the stroke
	/// </summary>
	public Color StrokeColor;

	/// <summary>
	/// Gets or sets the thickness of the stroke, expressed in normalized SDF-space units.
	/// </summary>
	public float StrokeThickness;

	/// <summary>
	/// Gets or sets the smoothness of the stroke edges, expressed in normalized SDF-space units.
	/// </summary>
	public float StrokeSmoothness;

	/// <summary>
	/// Initializes a new instance of the <see cref="SDFTextSettings"/> struct
	/// </summary>
	/// <param name="enableSuperSampling">Whether supersampling is enabled</param>
	/// <param name="effect">The SDF effect to apply</param>
	/// <param name="shadowColor">The color of the shadow</param>
	/// <param name="shadowOffset">The offset of the shadow in pixels</param>
	/// <param name="strokeColor">The color of the stroke</param>
	/// <param name="strokeThickness">The thickness of the stroke, expressed in normalized SDF-space units</param>
	/// <param name="strokeSmoothness">The smoothness of the stroke edges, expressed in normalized SDF-space units</param>
	public SDFTextSettings(bool enableSuperSampling, SDFFontEffect effect, Color shadowColor, Point shadowOffset, Color strokeColor, float strokeThickness, float strokeSmoothness)
	{
		EnableSuperSampling = enableSuperSampling;
		Effect = effect;
		ShadowColor = shadowColor;
		ShadowOffset = shadowOffset;
		StrokeColor = strokeColor;
		StrokeThickness = strokeThickness;
		StrokeSmoothness = strokeSmoothness;
	}

	/// <summary>
	/// The default <see cref="SDFTextSettings"/>: no supersampling and no effect
	/// </summary>
	public static readonly SDFTextSettings Default = new SDFTextSettings(false, SDFFontEffect.None, Color.Black, new Point(1, 1), Color.Black, 0.525f, 0.05f);
}

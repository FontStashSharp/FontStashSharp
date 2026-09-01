#if MONOGAME || FNA

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp
{
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

	/// <summary>
	/// A text batch renderer that draws text using a signed distance field (SDF) font effect
	/// </summary>
	public class SDFTextBatch : IDisposable
	{
		private class Renderer : IFontStashRenderer, IDisposable
		{
			private readonly SpriteBatch _spriteBatch;
			private SDFTextSettings _settings;
			private Texture2D _lastTexture;
			private Effect _effect;

			public GraphicsDevice GraphicsDevice => _spriteBatch.GraphicsDevice;
			public SDFTextSettings Settings => _settings;

			public Renderer(GraphicsDevice device)
			{
				_spriteBatch = new SpriteBatch(device);
			}

			public void Dispose()
			{
				_spriteBatch.Dispose();
				GC.SuppressFinalize(this);
			}

			public void Begin(SDFTextSettings settings)
			{
				_settings = settings;
				_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, settings.EnableSuperSampling, settings.Effect == SDFFontEffect.Shadow, settings.Effect == SDFFontEffect.Stroked);

				switch (settings.Effect)
				{
					case SDFFontEffect.Shadow:
						_effect.Parameters["cShadowColor"].SetValue(settings.ShadowColor.ToVector4());
						break;

					case SDFFontEffect.Stroked:
						_effect.Parameters["cStrokeColor"].SetValue(settings.StrokeColor.ToVector4());
						_effect.Parameters["cStrokeThickness"].SetValue(settings.StrokeThickness);
						_effect.Parameters["cStrokeSmoothness"].SetValue(settings.StrokeSmoothness);
						break;
				}

				_spriteBatch.Begin(SpriteSortMode.Deferred,
					BlendState.NonPremultiplied,
					SamplerState.LinearClamp,
					DepthStencilState.None,
					RasterizerState.CullCounterClockwise,
					_effect);
			}

			public void End()
			{
				_spriteBatch.End();

				_lastTexture = null;
				_effect = null;
			}

			public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				if (src != null)
				{
					var rect = src.Value;

					if (_settings.Effect == SDFFontEffect.Shadow)
					{
						if (texture != _lastTexture)
						{
							var shadowOffset = new Vector2((float)_settings.ShadowOffset.X / texture.Width, (float)_settings.ShadowOffset.Y / texture.Height);
							_effect.Parameters["cShadowOffset"].SetValue(shadowOffset);
							_lastTexture = texture;
						}

						rect.Width += _settings.ShadowOffset.X;
						rect.Height += _settings.ShadowOffset.Y;
					}

					_spriteBatch.Draw(texture,
						pos,
						rect,
						color,
						rotation,
						Vector2.Zero,
						scale,
						SpriteEffects.None,
						depth);
				}
				else
				{
					_spriteBatch.Draw(texture,
						pos,
						src,
						color,
						rotation,
						Vector2.Zero,
						scale,
						SpriteEffects.None,
						depth);
				}
			}
		}

		private readonly Renderer _renderer;


		/// <summary>
		/// Initializes a new instance of the <see cref="SDFTextBatch"/> class
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		public SDFTextBatch(GraphicsDevice graphicsDevice)
		{
			_renderer = new Renderer(graphicsDevice);
		}

		/// <summary>
		/// Releases all resources used by the <see cref="SDFTextBatch"/>
		/// </summary>
		public void Dispose()
		{
			_renderer.Dispose();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Begins a text batch using the specified signed distance field effects
		/// </summary>
		/// <param name="settings">The signed distance field settings to use</param>
		public void Begin(SDFTextSettings settings) => _renderer.Begin(settings);

		/// <summary>
		/// Begins a text batch using the default signed distance field effects
		/// </summary>
		public void Begin() => _renderer.Begin(SDFTextSettings.Default);

		/// <summary>
		/// Flushes the text batch and restores the previous state
		/// </summary>
		public void End() => _renderer.End();

		/// <summary>
		/// Draws a string using the specified font and settings
		/// </summary>
		/// <param name="font">The font to use</param>
		/// <param name="text">The text to draw</param>
		/// <param name="position">The position to draw the text at</param>
		/// <param name="color">The color of the text</param>
		/// <param name="rotation">The rotation of the text in radians</param>
		/// <param name="origin">The origin of the text</param>
		/// <param name="scale">The scale of the text</param>
		/// <param name="layerDepth">The layer depth of the text</param>
		/// <param name="characterSpacing">The additional character spacing</param>
		/// <param name="lineSpacing">The additional line spacing</param>
		/// <param name="textStyle">The text style to apply</param>
		public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None)
		{
			var fontEffect = FontSystemEffect.None;
			var fontEffectAmount = 0;

			var settings = _renderer.Settings;
			if (settings.Effect == SDFFontEffect.Stroked)
			{
				// This will force the font atlas to add 1 pixel padding around the glyphs to accommodate the stroke effect
				fontEffect = FontSystemEffect.Stroked;
				fontEffectAmount = 1;
			}

			font.DrawText(_renderer, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, fontEffect, fontEffectAmount);
		}
	}
}

#endif
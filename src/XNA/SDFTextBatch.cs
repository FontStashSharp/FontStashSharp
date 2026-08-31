#if MONOGAME || FNA

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Design;

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
		/// Gets or sets the offset of the shadow in pixels
		/// </summary>
		public Point ShadowOffset;

		// <summary>
		/// Gets or sets the color of the stroke
		/// </summary>
		public Color StrokeColor;

		public int StrokeSize;

		public SDFTextSettings(bool enableSuperSampling, SDFFontEffect effect, Color shadowColor, Point shadowOffset, Color strokeColor, int strokeSize)
		{
			EnableSuperSampling = enableSuperSampling;
			Effect = effect;
			ShadowColor = shadowColor;
			ShadowOffset = shadowOffset;
			StrokeColor = strokeColor;
			StrokeSize = strokeSize;
		}

		/// <summary>
		/// The default <see cref="SDFTextSettings"/>: no supersampling and no effect
		/// </summary>
		public static readonly SDFTextSettings Default = new SDFTextSettings(false, SDFFontEffect.None, Color.Black, new Point(1, 1), Color.Black, 1);
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
					} else if (_settings.Effect == SDFFontEffect.Stroked)
					{
						pos.X -= _settings.StrokeSize;
						pos.Y -= _settings.StrokeSize;

						var scaleFixX = (float)(rect.Width + _settings.StrokeSize * 2) / rect.Width;
						var scaleFixY = (float)(rect.Height + _settings.StrokeSize * 2) / rect.Height;
						
						scale.X *= scaleFixX;
						scale.Y *= scaleFixY;
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
		/// Draws a text
		/// </summary>
		/// <param name="font">The font to use for drawing</param>
		/// <param name="text">The text which will be drawn</param>
		/// <param name="position">The drawing location on screen</param>
		/// <param name="color">A color mask</param>
		/// <param name="rotation">A rotation of this text in radians</param>
		/// <param name="origin">Center of the rotation</param>
		/// <param name="scale">A scaling of this text. Null means the scaling is (1, 1)</param>
		/// <param name="layerDepth">A depth of the layer of this string</param>
		/// <param name="characterSpacing">Additional spacing between characters</param>
		/// <param name="lineSpacing">Additional spacing between lines</param>
		/// <param name="textStyle">The text style to apply</param>
		public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None)
		{
			font.DrawText(_renderer, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
		}
	}
}

#endif
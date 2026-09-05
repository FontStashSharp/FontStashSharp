#if MONOGAME || FNA

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp
{
	/// <summary>
	/// A text batch renderer that draws text using a signed distance field (SDF) font effect
	/// </summary>
	public class SDFTextBatch : ISDFTextRenderer, IDisposable
	{
		private enum RenderMode
		{
			Standard,
			Shadow,
			Stroked
		}

		private class Renderer : IFontStashRenderer
		{
			private RenderMode _mode;
			private readonly SpriteBatch _spriteBatch;
			private Texture2D _lastTexture;
			private Effect _effect;
			private bool _beginCalled, _spriteBatchBeginCalled;
			private Color _effectColor;
			private Vector2 _effectParameters;
			private bool _effectParametersDirty = true, _spriteModeSet = false;
			private bool _supersampling;

			public GraphicsDevice GraphicsDevice => _spriteBatch.GraphicsDevice;

			public bool Supersampling
			{
				get => _supersampling;

				set
				{
					if (value == _supersampling)
					{
						return;
					}

					_supersampling = value;
					InvalidateEffect();
				}
			}

			private RenderMode Mode
			{
				get => _mode;

				set
				{
					if (value == _mode)
					{
						return;
					}

					_mode = value;
					InvalidateEffect();
				}
			}

			private Color EffectColor
			{
				get => _effectColor;

				set
				{
					if (value == _effectColor)
					{
						return;
					}

					_effectColor = value;
					InvalidateEffectParameters();
				}
			}

			private Vector2 EffectParameters
			{
				get => _effectParameters;

				set
				{
					if (value == _effectParameters)
					{
						return;
					}

					_effectParameters = value;
					InvalidateEffectParameters();
				}
			}

			public Renderer(GraphicsDevice graphicsDevice)
			{
				_spriteBatch = new SpriteBatch(graphicsDevice);
			}

			public void Dispose()
			{
				_spriteBatch.Dispose();
				GC.SuppressFinalize(this);
			}

			public void Begin()
			{
				if (_beginCalled)
				{
					throw new Exception("Begin was called already.");
				}

				_beginCalled = true;
				ResetEffect();
			}

			public void End()
			{
				if (!_beginCalled)
				{
					throw new Exception("Begin wasn't called.");
				}

				EnsureSpriteBatchEnd();

				_beginCalled = false;
				InvalidateEffect();
				_lastTexture = null;
			}

			private void InvalidateEffectParameters()
			{
				_effectParametersDirty = true;
			}

			private void InvalidateEffect()
			{
				_effect = null;
				InvalidateEffectParameters();
			}

			public void ResetEffect()
			{
				Mode = RenderMode.Standard;
			}

			public void SetShadowEffect(Color color, Vector2 shadowOffset)
			{
				Mode = RenderMode.Shadow;
				EffectColor = color;
				EffectParameters = shadowOffset;
			}

			public void SetShadowEffect(Color color) => SetShadowEffect(color, Vector2.One);

			public void SetStrokeEffect(Color color, float thickness = 0.5f, float smoothness = 0.025f)
			{
				Mode = RenderMode.Stroked;
				EffectColor = color;
				EffectParameters = new Vector2(thickness, smoothness);
			}

			private void EnsureSpriteBatchEnd()
			{
				if (!_spriteBatchBeginCalled)
				{
					return;
				}

				_spriteBatch.End();
				_spriteBatchBeginCalled = false;
			}

			private void ApplyEffectMode()
			{
				if (_effect == null)
				{
					_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, _mode == RenderMode.Shadow, _mode == RenderMode.Stroked);

					EnsureSpriteBatchEnd();

					_spriteBatch.Begin(SpriteSortMode.Deferred,
						BlendState.NonPremultiplied,
						SamplerState.LinearClamp,
						DepthStencilState.None,
						RasterizerState.CullCounterClockwise,
						_effect);
					_spriteBatchBeginCalled = true;
				}

				if (_effectParametersDirty)
				{
					switch (Mode)
					{
						case RenderMode.Shadow:
							_effect.Parameters["cShadowColor"].SetValue(_effectColor.ToVector4());
							break;

						case RenderMode.Stroked:
							_effect.Parameters["cStrokeColor"].SetValue(_effectColor.ToVector4());
							_effect.Parameters["cStrokeThickness"].SetValue(_effectParameters.X);
							_effect.Parameters["cStrokeSmoothness"].SetValue(_effectParameters.Y);
							break;
					}

					_effectParametersDirty = false;
				}

				_spriteModeSet = false;
			}

			private void ApplySpriteMode()
			{
				if (_spriteModeSet)
				{
					return;
				}

				EnsureSpriteBatchEnd();
				_spriteBatch.Begin();
				_spriteBatchBeginCalled = true;

				_spriteModeSet = true;
				InvalidateEffect();
			}

			public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle)
			{
				ApplyEffectMode();

				var fontEffect = FontSystemEffect.None;
				var fontEffectAmount = 0;

				if (Mode == RenderMode.Stroked)
				{
					// This will force the font atlas to add 1 pixel padding around the glyphs to accommodate the stroke effect
					fontEffect = FontSystemEffect.Stroked;
					fontEffectAmount = 1;
				}

				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, fontEffect, fontEffectAmount);
			}

			public void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				ApplySpriteMode();
				_spriteBatch.Draw(texture, pos, src, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
			}

			void IFontStashRenderer.Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				if (src != null)
				{
					var rect = src.Value;

					if (Mode == RenderMode.Shadow)
					{
						if (texture != _lastTexture)
						{
							var shadowOffset = new Vector2((float)_effectParameters.X / texture.Width, (float)_effectParameters.Y / texture.Height);
							_effect.Parameters["cShadowOffset"].SetValue(shadowOffset);
							_lastTexture = texture;
						}

						rect.Width += (int)_effectParameters.X;
						rect.Height += (int)_effectParameters.Y;
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
		/// Gets or sets whether supersampling is enabled for the SDF font effect.
		/// Supersampling improves the quality of the signing distance field at the cost of performance.
		/// </summary>
		public bool Supersampling
		{
			get => _renderer.Supersampling;
			set => _renderer.Supersampling = value;
		}

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
		/// Begins a batch of text drawing operations.
		/// </summary>
		public void Begin() => _renderer.Begin();

		/// <summary>
		/// Ends a batch of text drawing operations and flushes any pending sprite batches.
		/// </summary>
		public void End() => _renderer.End();

		/// <inheritdoc/>
		public void ResetEffect() => _renderer.ResetEffect();

		/// <inheritdoc/>
		public void SetShadowEffect(Color color, Vector2 shadowOffset) => _renderer.SetShadowEffect(color, shadowOffset);

		/// <summary>
		/// Configures the renderer to draw a shadow behind the text using the specified color and a default offset of one pixel.
		/// </summary>
		/// <param name="color">The color of the shadow.</param>
		public void SetShadowEffect(Color color) => SetShadowEffect(color, Vector2.One);

		/// <inheritdoc/>
		public void SetStrokeEffect(Color color, float thickness = 0.5f, float smoothness = 0.025f) => _renderer.SetStrokeEffect(color, thickness, smoothness);

		/// <inheritdoc/>
		public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None) => _renderer.DrawString(font, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);

		/// <inheritdoc/>
		public void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth) =>
			_renderer.DrawSprite(texture, pos, src, color, rotation, scale, depth);
	}
}

#endif
#if MONOGAME || FNA

using FontStashSharp.Interfaces;
using FontStashSharp.RichText;
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
			Stroke
		}

		private class Renderer : IFontStashRenderer, IDisposable
		{
			private RenderMode? _mode;
			private readonly SpriteBatch _spriteBatchEffect;
			private SpriteBatch _spriteBatchSprite;
			private Texture2D _lastTexture;
			private bool _beginCalled, _spriteBatchEffectBeginCalled, _spriteBatchSpriteBeginCalled;
			private Effect _effect;
			private Color? _effectColor;
			private Vector2? _effectParameters;
			private bool _supersampling;

			public GraphicsDevice GraphicsDevice => _spriteBatchEffect.GraphicsDevice;

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
					_mode = null;
				}
			}

			private RenderMode? Mode
			{
				get => _mode;

				set
				{
					if (value == _mode)
					{
						return;
					}

					_mode = value;
					_effectColor = null;
					_effectParameters = null;
					_lastTexture = null;
				}
			}

			public Renderer(GraphicsDevice graphicsDevice)
			{
				_spriteBatchEffect = new SpriteBatch(graphicsDevice);
			}

			public void Dispose()
			{
				_spriteBatchEffect.Dispose();
				_spriteBatchSprite?.Dispose();

				GC.SuppressFinalize(this);
			}

			public void Begin()
			{
				if (_beginCalled)
				{
					throw new Exception("Begin was called already.");
				}

				_beginCalled = true;
			}

			public void End()
			{
				if (!_beginCalled)
				{
					throw new Exception("Begin wasn't called.");
				}

				EnsureSpriteBatchEffectEnd();
				if (_spriteBatchSpriteBeginCalled)
				{
					_spriteBatchSprite.End();
					_spriteBatchSpriteBeginCalled = false;
				}

				_beginCalled = false;
				Mode = null;
			}

			private void EnsureSpriteBatchEffectEnd()
			{
				if (!_spriteBatchEffectBeginCalled)
				{
					return;
				}

				_spriteBatchEffect.End();
				_spriteBatchEffectBeginCalled = false;
			}

			private void RestartSpriteBatchEffect(Effect effect)
			{
				EnsureSpriteBatchEffectEnd();

				_spriteBatchEffect.Begin(SpriteSortMode.Deferred,
					BlendState.NonPremultiplied,
					SamplerState.LinearClamp,
					DepthStencilState.None,
					RasterizerState.CullCounterClockwise,
					effect);
				_spriteBatchEffectBeginCalled = true;
				_effect = effect;
			}

			public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle)
			{
				if (Mode != RenderMode.Standard)
				{
					var effect = Resources.GetEffect(_spriteBatchEffect.GraphicsDevice, Supersampling, false, false);
					RestartSpriteBatchEffect(effect);
					Mode = RenderMode.Standard;
				}

				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawShadowString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale,
				float layerDepth, float characterSpacing, float lineSpacing,
				TextStyle textStyle, Color shadowColor, float shadowOffsetX, float shadowOffsetY)
			{
				if (Mode != RenderMode.Shadow)
				{
					var effect = Resources.GetEffect(_spriteBatchEffect.GraphicsDevice, Supersampling, true, false);
					RestartSpriteBatchEffect(effect);
					Mode = RenderMode.Shadow;
				}

				if (_effectColor != shadowColor)
				{
					_effect.Parameters["cShadowColor"].SetValue(shadowColor.ToVector4());
					_effectColor = shadowColor;
				}

				var newParameters = new Vector2(shadowOffsetX, shadowOffsetY);
				if (_effectParameters != new Vector2(shadowOffsetX, shadowOffsetY))
				{
					_lastTexture = null;
					_effectParameters = newParameters;
				}

				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale,
				float layerDepth, float characterSpacing, float lineSpacing,
				TextStyle textStyle, Color strokeColor, float strokeThickness, float strokeSmoothness)
			{
				if (Mode != RenderMode.Stroke)
				{
					var effect = Resources.GetEffect(_spriteBatchEffect.GraphicsDevice, Supersampling, false, true);
					RestartSpriteBatchEffect(effect);
					Mode = RenderMode.Stroke;
				}

				if (_effectColor != strokeColor)
				{
					_effect.Parameters["cStrokeColor"].SetValue(strokeColor.ToVector4());
				}

				var newParameters = new Vector2(strokeThickness, strokeSmoothness);
				if (_effectParameters != newParameters)
				{
					_effect.Parameters["cStrokeThickness"].SetValue(strokeThickness);
					_effect.Parameters["cStrokeSmoothness"].SetValue(strokeSmoothness);
					_effectParameters = newParameters;
				}

				// This will force the font atlas to add 1 pixel padding around the glyphs to accommodate the stroke effect
				var fontEffect = FontSystemEffect.Stroked;
				var fontEffectAmount = 1;

				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, fontEffect, fontEffectAmount);
			}

			public void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				if (!_spriteBatchSpriteBeginCalled)
				{
					if (_spriteBatchSprite == null)
					{
						_spriteBatchSprite = new SpriteBatch(GraphicsDevice);
					}

					_spriteBatchSprite.Begin();
					_spriteBatchSpriteBeginCalled = true;
				}

				_spriteBatchSprite.Draw(texture, pos, src, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
			}

			void IFontStashRenderer.Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				if (src != null)
				{
					var rect = src.Value;

					if (Mode == RenderMode.Shadow)
					{
						var ep = _effectParameters.Value;
						if (texture != _lastTexture)
						{
							var shadowOffset = new Vector2(ep.X / texture.Width, ep.Y / texture.Height);
							_effect.Parameters["cShadowOffset"].SetValue(shadowOffset);
							_lastTexture = texture;
						}

						rect.Width += (int)ep.X;
						rect.Height += (int)ep.Y;
					}

					_spriteBatchEffect.Draw(texture,
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
					_spriteBatchEffect.Draw(texture,
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
		public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None) =>
			_renderer.DrawString(font, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);

		/// <inheritdoc/>
		public void DrawShadowString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, Color? shadowColor = null, float shadowOffsetX = 1, float shadowOffsetY = 1) =>
			_renderer.DrawShadowString(font, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, shadowColor ?? Color.Black, shadowOffsetX, shadowOffsetY);

		/// <inheritdoc/>
		public void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, Color? strokeColor = null, float strokeThickness = 0.5f, float strokeSmoothness = 0.05f) =>
			_renderer.DrawStrokeString(font, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, strokeColor ?? Color.Black, strokeThickness, strokeSmoothness);

		/// <inheritdoc/>
		public void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth) =>
			_renderer.DrawSprite(texture, pos, src, color, rotation, scale, depth);
	}
}

#endif
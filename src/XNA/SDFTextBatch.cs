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
			Stroke,
			Sprite
		}

		private class Renderer : IFontStashRenderer, IDisposable
		{
			private RenderMode? _mode;
			private readonly SpriteBatch _spriteBatch;
			private bool _beginCalled, _spriteBatchBeginCalled, _spriteBatchRestartRequired;
			private Color? _effectColor;
			private Vector2? _effectParameters;
			private bool _supersampling;
			private Texture2D _lastTexture;
			private Effect _effect;

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
					Mode = null;
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
					EffectColor = null;
					EffectParameters = null;
					_lastTexture = null;
					InvalidateSpriteBatch();
				}
			}

			private Color? EffectColor
			{
				get => _effectColor;

				set
				{
					if (value == _effectColor)
					{
						return;
					}

					_effectColor = value;
					InvalidateSpriteBatch();
				}
			}

			private Vector2? EffectParameters
			{
				get => _effectParameters;

				set
				{
					if (value == _effectParameters)
					{
						return;
					}

					_effectParameters = value;
					_lastTexture = null;

					InvalidateSpriteBatch();
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

			private void InvalidateSpriteBatch()
			{
				_spriteBatchRestartRequired = true;
			}

			private void UpdateSpriteBatch()
			{
				if (!_spriteBatchRestartRequired)
				{
					return;
				}

				if (_spriteBatchBeginCalled)
				{
					_spriteBatch.End();
					_spriteBatchBeginCalled = false;
					_effect = null;
				}

				if (_mode == null)
				{
					_spriteBatchRestartRequired = false;
					return;
				}

				var v = _mode.Value;
				switch (v)
				{
					case RenderMode.Standard:
						_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, false, false);
						break;
					case RenderMode.Shadow:
						_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, true, false);
						_effect.Parameters["cShadowColor"].SetValue(EffectColor.Value.ToVector4());
						break;
					case RenderMode.Stroke:
						_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, false, true);
						_effect.Parameters["cStrokeColor"].SetValue(EffectColor.Value.ToVector4());

						var v2 = EffectParameters.Value;
						_effect.Parameters["cStrokeThickness"].SetValue(v2.X);
						_effect.Parameters["cStrokeSmoothness"].SetValue(v2.Y);
						break;
				}

				if (_mode != RenderMode.Sprite)
				{
					_spriteBatch.Begin(SpriteSortMode.Deferred,
						BlendState.NonPremultiplied,
						SamplerState.LinearClamp,
						DepthStencilState.None,
						RasterizerState.CullCounterClockwise,
						_effect);
				}
				else
				{
					_spriteBatch.Begin(SpriteSortMode.Deferred,
						BlendState.AlphaBlend,
						SamplerState.LinearClamp,
						DepthStencilState.None,
						RasterizerState.CullCounterClockwise);
				}

				_spriteBatchBeginCalled = true;
				_spriteBatchRestartRequired = false;
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

				_beginCalled = false;
				Mode = null;
				UpdateSpriteBatch();
			}

			public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle)
			{
				Mode = RenderMode.Standard;

				UpdateSpriteBatch();
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawShadowString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle,
				Color shadowColor, float shadowOffsetX, float shadowOffsetY)
			{
				Mode = RenderMode.Shadow;
				EffectColor = shadowColor;
				EffectParameters = new Vector2(shadowOffsetX, shadowOffsetY);

				UpdateSpriteBatch();
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle,
				Color strokeColor, float strokeThickness, float strokeSmoothness)
			{
				Mode = RenderMode.Stroke;
				EffectColor = strokeColor;
				EffectParameters = new Vector2(strokeThickness, strokeSmoothness);

				UpdateSpriteBatch();
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				Mode = RenderMode.Sprite;

				UpdateSpriteBatch();
				_spriteBatch.Draw(texture, pos, src, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
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
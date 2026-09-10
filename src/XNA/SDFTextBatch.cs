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
			private bool _beginCalled;
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
					SetState(null, null, null);
				}
			}

			public RasterizerState RasterizerState { get; set; } = RasterizerState.CullCounterClockwise;

			public Renderer(GraphicsDevice graphicsDevice)
			{
				_spriteBatch = new SpriteBatch(graphicsDevice);
			}

			public void Dispose()
			{
				_spriteBatch.Dispose();

				GC.SuppressFinalize(this);
			}

			private void SetState(RenderMode? newMode, Color? newColor, Vector2? newParameters)
			{
				if (newMode == RenderMode.Shadow || newMode == RenderMode.Stroke)
				{
					// Some validation
					if (newColor == null || newParameters == null)
					{
						throw new ArgumentNullException("newColor/newParameters can'be null for Shadow/Stroke modes.");
					}
				}

				if (newMode == _mode && newColor == _effectColor && newParameters == _effectParameters)
				{
					// Nothing changed
					return;
				}

				// End current SpriteBatch
				if (_mode != null)
				{
					_spriteBatch.End();
				}

				// Set the new state
				_mode = newMode;
				_effectColor = newColor;
				_effectParameters = newParameters;

				if (_mode != null)
				{
					// Apply the new state
					switch (_mode)
					{
						case RenderMode.Standard:
							_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, false, false);
							break;
						case RenderMode.Shadow:
							_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, true, false);
							_effect.Parameters["cShadowColor"].SetValue(_effectColor.Value.ToVector4());
							break;
						case RenderMode.Stroke:
							_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersampling, false, true);
							_effect.Parameters["cStrokeColor"].SetValue(_effectColor.Value.ToVector4());

							var v2 = _effectParameters.Value;
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
							RasterizerState,
							_effect);
					}
					else
					{
						_spriteBatch.Begin(SpriteSortMode.Deferred,
							BlendState.AlphaBlend,
							SamplerState.LinearClamp,
							DepthStencilState.None,
							RasterizerState);
					}
				}

				_lastTexture = null;
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
				SetState(null, null, null);
			}

			public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle)
			{
				SetState(RenderMode.Standard, null, null);
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawShadowString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle,
				Color shadowColor, float shadowOffsetX, float shadowOffsetY)
			{
				SetState(RenderMode.Shadow, shadowColor, new Vector2(shadowOffsetX, shadowOffsetY));
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, Color color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle,
				Color strokeColor, float strokeThickness, float strokeSmoothness)
			{
				SetState(RenderMode.Stroke, strokeColor, new Vector2(strokeThickness, strokeSmoothness));
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				SetState(RenderMode.Sprite, null, null);
				_spriteBatch.Draw(texture, pos, src, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
			}

			void IFontStashRenderer.Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
			{
				if (src != null)
				{
					var rect = src.Value;

					if (_mode == RenderMode.Shadow)
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
		/// Gets or sets the rasterizer state used when drawing SDF text.
		/// </summary>
		public RasterizerState RasterizerState
		{
			get => _renderer.RasterizerState;
			set => _renderer.RasterizerState = value;
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
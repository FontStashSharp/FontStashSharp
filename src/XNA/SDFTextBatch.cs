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
	public class SDFTextBatch : ISDFTextRenderer, IFontStashRenderer, IDisposable
	{
		private enum RenderMode
		{
			None,
			Shadow,
			Stroked,
			Sprite
		}

		private RenderMode _mode;
		private readonly SpriteBatch _spriteBatch;
		private Texture2D _lastTexture;
		private Effect _effect;
		private bool _beginCalled, _spriteBatchBeginCalled;
		private Color _effectColor;
		private Vector2 _effectParameters;
		private bool _effectParametersDirty = true;

		public GraphicsDevice GraphicsDevice => _spriteBatch.GraphicsDevice;

		public bool Supersamling { get; set; }

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

		/// <summary>
		/// Initializes a new instance of the <see cref="SDFTextBatch"/> class
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		public SDFTextBatch(GraphicsDevice graphicsDevice)
		{
			_spriteBatch = new SpriteBatch(graphicsDevice);
		}

		/// <summary>
		/// Releases all resources used by the <see cref="SDFTextBatch"/>
		/// </summary>
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
		}

		public void End()
		{
			if (!_beginCalled)
			{
				throw new Exception("Begin wasn't called");
			}

			EnsureSpriteBatchEnd();

			_beginCalled = false;
			_effect = null;
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
			Mode = RenderMode.None;
		}

		public void SetShadowEffect(Color color, Vector2 shadowOffset)
		{
			Mode = RenderMode.Shadow;
			EffectColor = color;
			EffectParameters = shadowOffset;
		}

		public void SetStrokeEffect(Color color, float thickness, float smoothness)
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

		private void ApplyEffectSettings()
		{
			var restartSpriteBatch = false;

			if (_effect == null)
			{
				_effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, Supersamling, _mode == RenderMode.Shadow, _mode == RenderMode.Stroked);
				restartSpriteBatch = true;
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
				restartSpriteBatch = true;
			}

			if (restartSpriteBatch)
			{
				EnsureSpriteBatchEnd();

				_spriteBatch.Begin(SpriteSortMode.Deferred,
					BlendState.NonPremultiplied,
					SamplerState.LinearClamp,
					DepthStencilState.None,
					RasterizerState.CullCounterClockwise,
					_effect);
				_spriteBatchBeginCalled = true;
			}
		}

		public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None)
		{
			ApplyEffectSettings();

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
			if (_mode != RenderMode.Sprite)
			{
				_effect = null;
				_mode = RenderMode.Sprite;
				EnsureSpriteBatchEnd();
				_spriteBatch.Begin();
			}

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
}

#endif
#if MONOGAME || FNA

using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace FontStashSharp
{
	internal struct SDFTextSettings
	{
		public bool EnableSuperSampling;
		public bool EnableShadow;
		public Vector2 ShadowOffset;
		public Color ShadowColor;
		public bool EnableStroke;
		public Color StrokeColor;

		public static readonly SDFTextSettings Default = new SDFTextSettings();
	}

	/// <summary>
	/// A text batch renderer that draws text using a signed distance field (SDF) font effect
	/// </summary>
	public class SDFTextBatch : IDisposable
	{
		private class Renderer: IFontStashRenderer, IDisposable
		{
			private readonly SpriteBatch _spriteBatch;

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
				var effect = Resources.GetEffect(_spriteBatch.GraphicsDevice, settings.EnableSuperSampling, settings.EnableShadow, settings.EnableStroke);

				if (settings.EnableShadow)
				{
					effect.Parameters["cShadowOffset"].SetValue(settings.ShadowOffset);
					effect.Parameters["cShadowColor"].SetValue(settings.ShadowColor.ToVector4());
				}

				if (settings.EnableStroke)
				{
					effect.Parameters["cStrokeColor"].SetValue(settings.StrokeColor.ToVector4());
				}

				_spriteBatch.Begin(effect: effect, blendState: BlendState.NonPremultiplied);
			}

			public void End()
			{
				_spriteBatch.End();
			}

			public void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth)
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
using FontStashSharp.Interfaces;
using System;

#if MONOGAME || FNA || KNI || XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#elif STRIDE
using Stride.Core.Mathematics;
using Stride.Graphics;
using Texture2D = Stride.Graphics.Texture;
#else
using System.Drawing;
using System.Numerics;
using Matrix = System.Numerics.Matrix3x2;
using Texture2D = System.Object;
using Color = FontStashSharp.FSColor;
#endif

namespace FontStashSharp.RichText
{
	internal class FSRenderContext : IFSRenderContext
	{
		private IFontStashRenderer _renderer;
		private IFontStashRenderer2 _renderer2;
		private ISDFTextRenderer _renderer3;
		private Matrix _transformation;
		private Vector2 _scale;
		private float _rotation;
		private float _layerDepth;

		public void SetRenderer(IFontStashRenderer renderer)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

			_renderer = renderer;
			_renderer2 = null;
			_renderer3 = null;
		}

		public void SetRenderer(IFontStashRenderer2 renderer)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}
			_renderer = null;
			_renderer2 = renderer;
			_renderer3 = null;
		}

		public void SetRenderer(ISDFTextRenderer renderer)
		{
			if (renderer == null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}
			_renderer = null;
			_renderer2 = null;
			_renderer3 = renderer;
		}

		internal void Prepare(Vector2 position, float rotation, Vector2 origin, Vector2 scale, float layerDepth)
		{
			_scale = scale;
			_rotation = rotation;
			_layerDepth = layerDepth;
			Utility.BuildTransform(position, _rotation, origin, _scale, out _transformation);
		}

		public void DrawText(string text, SpriteFontBase font, Vector2 pos, Color color,
			TextStyle textStyle, FontSystemEffect effect, int effectAmount)
		{
			if (_renderer == null && _renderer2 == null)
			{
				throw new Exception("Can't draw ordinary text through SDF renderer.");
			}

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			pos = pos.Transform(ref _transformation);
			if (_renderer != null)
			{
				font.DrawText(_renderer, text, pos, color, _rotation, default(Vector2), _scale, _layerDepth,
					textStyle: textStyle, effect: effect, effectAmount: effectAmount);
			}
			else if (_renderer2 != null)
			{
				font.DrawText(_renderer2, text, pos, color, _rotation, default(Vector2), _scale, _layerDepth,
					textStyle: textStyle, effect: effect, effectAmount: effectAmount);
			}
		}

		/// <summary>
		/// Draws an image using the current rendering context.
		/// </summary>
		/// <param name="texture">The texture to draw</param>
		/// <param name="sourceRegion">The region of the texture to draw</param>
		/// <param name="position">The position to draw at</param>
		/// <param name="scale">The scale factors to apply</param>
		/// <param name="color">The color to render the image with</param>
		public void DrawImage(Texture2D texture, Rectangle sourceRegion, Vector2 position, Vector2 scale, Color color)
		{
			if (_renderer != null)
			{
				position = position.Transform(ref _transformation);
				_renderer.Draw(texture, position, sourceRegion, color, _rotation, _scale, _layerDepth);
			}
			else if (_renderer2 != null)
			{
				var topLeft = new VertexPositionColorTexture();
				var topRight = new VertexPositionColorTexture();
				var bottomLeft = new VertexPositionColorTexture();
				var bottomRight = new VertexPositionColorTexture();

				var size = new Vector2(sourceRegion.Width, sourceRegion.Height) * _scale * scale;
				_renderer2.DrawQuad(texture, color, position, ref _transformation,
					_layerDepth, size, sourceRegion,
					ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
			}
			else
			{
				position = position.Transform(ref _transformation);
				_renderer3.DrawSprite(texture, position, sourceRegion, color, _rotation, _scale, _layerDepth);
			}
		}

		private void EnsureSDFRenderer()
		{
			if (_renderer3 == null)
			{
				throw new Exception("SDF Renderer is null.");
			}
		}

		public void DrawSDFText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle)
		{
			EnsureSDFRenderer();

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			pos = pos.Transform(ref _transformation);
			_renderer3.DrawString(font, text, pos, color, _rotation, default(Vector2), _scale, _layerDepth, 0, 0, textStyle);
		}

		public void DrawSDFShadowText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle, Color shadowColor, Vector2 shadowOffset)
		{
			EnsureSDFRenderer();

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			pos = pos.Transform(ref _transformation); 
			_renderer3.DrawShadowString(font, text, pos, color, _rotation, default(Vector2), _scale, _layerDepth, 0, 0, textStyle, shadowColor, shadowOffset.X, shadowOffset.Y);
		}

		public void DrawSDFStrokeText(string text, SpriteFontBase font, Vector2 pos, Color color, TextStyle textStyle, Color strokeColor, float strokeThickness, float strokeSmoothness)
		{
			EnsureSDFRenderer();

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			pos = pos.Transform(ref _transformation); 
			_renderer3.DrawStrokeString(font, text, pos, color, _rotation, default(Vector2), _scale, _layerDepth, 0, 0, textStyle, strokeColor, strokeThickness, strokeSmoothness);
		}
	}
}

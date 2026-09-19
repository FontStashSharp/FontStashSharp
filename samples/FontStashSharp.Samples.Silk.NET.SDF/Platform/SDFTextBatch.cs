using FontStashSharp.Interfaces;
using FontStashSharp.RichText;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
using Color = FontStashSharp.FSColor;
using Rectangle = System.Drawing.Rectangle;

namespace FontStashSharp.Platform
{
	/// <summary>
	/// A text batch renderer that draws text using a signed distance field (SDF) font effect.
	/// </summary>
	/// <remarks>
	/// Rendering is state-driven: each draw call picks a <see cref="InternalRenderer.Mode"/>,
	/// which selects a shader variant (standard / shadow / stroke, optionally supersampled) and a
	/// matching blend state. Switching modes flushes geometry queued in the <c>VertexBatch</c>.
	/// </remarks>
	public class SDFTextBatch : ISDFTextRenderer, IDisposable
	{
		// Implements IFontStashRenderer so SpriteFontBase.DrawText can push glyphs to us.
		private class InternalRenderer : IFontStashRenderer, IDisposable
		{
			// Owns the vertex/index buffers, VAO and projection viewport.
			private readonly VertexBatch _vertexBatch = new VertexBatch();

			// The effect applied to subsequently drawn glyphs.
			public enum Mode
			{
				Standard,
				Shadow,
				Stroke,
				Sprite
			}

			// Current pipeline state; _mode == null means no batch in progress.
			private Mode? _mode;
			private bool _supersampling = true;
			private FSColor? _effectColor;
			private Vector2? _effectParameters;
			// Shadow offset in pixels; expands glyph rects so the offset shadow isn't clipped.
			private Vector2 _shadowOffsetPixels;

			// Shader for Sprite mode and ordinary rendering.
			private readonly Shader _plainShader;
			// Lazily compiled SDF variants, indexed by (supersampling?1:0)|(shadow?2:0)|(stroke?4:0).
			private readonly Shader[] _shaders = new Shader[8];

			private readonly Texture2DManager _textureManager;

			public ITexture2DManager TextureManager => _textureManager;

			public Silk.NET.Maths.Rectangle<int> Viewport
			{
				get => _vertexBatch.Viewport;
				set => _vertexBatch.Viewport = value;
			}

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
					// Flush and switch to the matching shader variant.
					SetState(null, null, null);
				}
			}

			public InternalRenderer()
			{
				_textureManager = new Texture2DManager();

				_plainShader = new Shader(@"Assets/Shaders/shader.vert", @"Assets/Shaders/shader.frag");
			}

			~InternalRenderer() => Dispose(false);
			public void Dispose() => Dispose(true);

			private void Dispose(bool disposing)
			{
				if (!disposing)
				{
					return;
				}

				for (var i = 0; i < _shaders.Length; ++i)
				{
					_shaders[i]?.Dispose();
				}

				_plainShader.Dispose();
				_vertexBatch.Dispose();
			}

			public void Begin() => _vertexBatch.Begin();

			// Flushes pending geometry and returns to the no-batch idle state.
			public void End()
			{
				SetState(null, null, null);
			}

			// Requests a (mode, color, parameters) combination for subsequent draws.
			private void SetState(Mode? newMode, FSColor? newColor, Vector2? newParameters)
			{
				if (newMode == Mode.Shadow || newMode == Mode.Stroke)
				{
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

				// Flush geometry under the previous shader before switching state.
				if (_mode != null)
				{
					_vertexBatch.FlushBuffer();
				}

				_mode = newMode;
				_effectColor = newColor;
				_effectParameters = newParameters;

				if (_mode != null)
				{
					ApplyShaderAndBlend();
				}
			}

			private void ApplyShaderAndBlend()
			{
				var shader = GetCurrentShader();

				// Sprite mode uses premultiplied blending; SDF modes are straight alpha.
				if (_mode == Mode.Sprite)
				{
					Env.Gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
				}
				else
				{
					Env.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				}
				GLUtility.CheckError();

				shader.Use();

				var transform = Matrix4x4.CreateOrthographicOffCenter(
					Viewport.Origin.X, Viewport.Origin.X + Viewport.Size.X,
					Viewport.Origin.Y + Viewport.Size.Y, Viewport.Origin.Y,
					0, -1);
				shader.SetUniform("MatrixTransform", transform);
				shader.SetUniform("TextureSampler", 0);

				// Shadow/stroke uniforms match the #ifdef'd uniforms in sdf.frag.
				if (_mode == Mode.Shadow)
				{
					shader.SetUniform("cShadowColor", _effectColor.Value.ToVector4());
					shader.SetUniform("cShadowOffset", _effectParameters.Value);
				}
				else if (_mode == Mode.Stroke)
				{
					shader.SetUniform("cStrokeColor", _effectColor.Value.ToVector4());
					shader.SetUniform("cStrokeThickness", _effectParameters.Value.X);
				}
			}

			private Shader GetCurrentShader()
			{
				switch (_mode)
				{
					case Mode.Standard:
						return GetShader(_supersampling, false, false);
					case Mode.Shadow:
						return GetShader(_supersampling, true, false);
					case Mode.Stroke:
						return GetShader(_supersampling, false, true);
					default:
						return _plainShader;
				}
			}

			private Shader GetShader(bool superSampling, bool shadow, bool stroke)
			{
				// Key the cache on the 3 boolean flags.
				var key = 0;
				if (superSampling)
				{
					key |= 1;
				}

				if (shadow)
				{
					key |= 2;
				}

				if (stroke)
				{
					key |= 4;
				}

				if (_shaders[key] != null)
				{
					return _shaders[key];
				}

				// Compile the matching variant by prepending the #defines to sdf.frag.
				var defines = new List<string>();
				if (superSampling)
				{
					defines.Add("SUPERSAMPLING");
				}

				if (shadow)
				{
					defines.Add("EFFECTSHADOW");
				}

				if (stroke)
				{
					defines.Add("EFFECTSTROKE");
				}

				_shaders[key] = new Shader(@"Assets/Shaders/shader.vert", @"Assets/Shaders/sdf.frag", defines.ToArray());

				return _shaders[key];
			}

			// Each public draw entry sets the effect mode, then pushes glyphs through
			// SpriteFontBase.DrawText which calls our IFontStashRenderer.Draw per quad.
			public void DrawString(SpriteFontBase font, string text, Vector2 position, FSColor color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle)
			{
				SetState(Mode.Standard, null, null);
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawShadowString(SpriteFontBase font, string text, Vector2 position, FSColor color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle,
				FSColor shadowColor, float shadowOffsetX, float shadowOffsetY)
			{
				// Pixel-space offset for glyph rect expansion; normalized offset for the shader.
				_shadowOffsetPixels = new Vector2(shadowOffsetX, shadowOffsetY);
				var normalizedShadowOffset = new Vector2(shadowOffsetX / font.TextureSize.X, shadowOffsetY / font.TextureSize.Y);
				SetState(Mode.Shadow, shadowColor, normalizedShadowOffset);
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, FSColor color,
				float rotation, Vector2 origin, Vector2? scale, float layerDepth,
				float characterSpacing, float lineSpacing, TextStyle textStyle,
				FSColor strokeColor, float strokeThickness)
			{
				SetState(Mode.Stroke, strokeColor, new Vector2(strokeThickness, 0));
				font.DrawText(this, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle);
			}

			public void DrawSprite(object texture, Vector2 pos, Rectangle? src, FSColor color, float rotation, Vector2 scale, float depth)
			{
				SetState(Mode.Sprite, null, null);
				Draw(texture, pos, src, color, rotation, scale, depth);
			}

			public void Draw(object texture, Vector2 pos, Rectangle? src, FSColor color, float rotation, Vector2 scale, float depth)
			{
				// In shadow mode, grow the source rect so the offset shadow stays inside the atlas.
				if (src != null && _mode == Mode.Shadow)
				{
					var rect = src.Value;
					rect.Width += (int)_shadowOffsetPixels.X;
					rect.Height += (int)_shadowOffsetPixels.Y;
					src = rect;
				}

				_vertexBatch.Draw(texture, pos, src, color, rotation, scale, depth);
			}
		}


		private readonly InternalRenderer _renderer;

		/// <summary>
		/// Gets or sets whether supersampling is enabled for the SDF font effect.
		/// When enabled, the SDF shader applies supersampled edges for smoother outlines.
		/// The default value is <c>true</c>.
		/// Disabling it may improve rendering performance, but can lead to rendering artefacts, such as "holes" appearing in the glyphs.
		/// </summary>
		public bool Supersampling
		{
			get => _renderer.Supersampling;
			set => _renderer.Supersampling = value;
		}

		/// <summary>
		/// Gets or sets the viewport used to build the projection matrix.
		/// </summary>
		public Silk.NET.Maths.Rectangle<int> Viewport
		{
			get => _renderer.Viewport;
			set => _renderer.Viewport = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SDFTextBatch"/> class
		/// </summary>
		public SDFTextBatch()
		{
			_renderer = new InternalRenderer();
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
			TextStyle textStyle = TextStyle.None, Color? shadowColor = null, float shadowOffsetX = 2, float shadowOffsetY = 2) =>
			_renderer.DrawShadowString(font, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, shadowColor ?? FSColor.Black, shadowOffsetX, shadowOffsetY);

		/// <inheritdoc/>
		public void DrawStrokeString(SpriteFontBase font, string text, Vector2 position, Color color,
			float rotation = 0, Vector2 origin = default, Vector2? scale = null,
			float layerDepth = 0.0f, float characterSpacing = 0.0f, float lineSpacing = 0.0f,
			TextStyle textStyle = TextStyle.None, Color? strokeColor = null, float strokeThickness = 0.2f) =>
			_renderer.DrawStrokeString(font, text, position, color, rotation, origin, scale, layerDepth, characterSpacing, lineSpacing, textStyle, strokeColor ?? FSColor.Black, strokeThickness);

		/// <inheritdoc/>
		public void DrawSprite(object texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth) =>
			_renderer.DrawSprite(texture, pos, src, color, rotation, scale, depth);
	}
}
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
using Color = FontStashSharp.FSColor;
using Texture2D = System.Object;
#endif

namespace FontStashSharp.Interfaces
{
	/// <summary>
	/// Provides rendering capabilities for drawing text with a graphics device or texture manager.
	/// </summary>
	public interface IFontStashRenderer
	{
#if MONOGAME || FNA || KNI || XNA || STRIDE
		/// <summary>
		/// Gets the graphics device used for rendering.
		/// </summary>
		GraphicsDevice GraphicsDevice { get; }
#else
		/// <summary>
		/// Gets the texture manager for managing 2D textures in a platform-agnostic manner.
		/// </summary>
		ITexture2DManager TextureManager { get; }
#endif

		/// <summary>
		/// Draws a textured quad at the specified position with optional rotation and scaling.
		/// </summary>
		/// <param name="texture">The texture to draw.</param>
		/// <param name="pos">The position to draw at.</param>
		/// <param name="src">The source rectangle within the texture, or null to use the entire texture.</param>
		/// <param name="color">The color to apply to the rendered texture.</param>
		/// <param name="rotation">The rotation angle in radians.</param>
		/// <param name="scale">The scale factors for X and Y axes.</param>
		/// <param name="depth">The depth value for layering.</param>
		void Draw(Texture2D texture, Vector2 pos, Rectangle? src, Color color, float rotation, Vector2 scale, float depth);
	}
}

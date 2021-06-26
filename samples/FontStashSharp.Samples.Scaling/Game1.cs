using FontStashSharp.Samples.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;

namespace FontStashSharp.Samples
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private MainForm _mainForm;
		private Desktop _desktop;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};
			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			MyraEnvironment.Game = this;

			_mainForm = new MainForm(GraphicsDevice);

			// Add it to the desktop
			_desktop = new Desktop
			{
				Root = _mainForm
			};
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_desktop.Render();
		}
	}
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace FontStashSharp.Samples;

/// <summary>
/// This is the main type for your game.
/// </summary>
public class Game1 : Game
{
	private readonly GraphicsDeviceManager _graphics;
	private Desktop _desktop;

	public static Game1 Instance { get; private set; }

	public Game1()
	{
		Instance = this;

		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = 1400,
			PreferredBackBufferHeight = 900,
			GraphicsProfile = GraphicsProfile.HiDef
		};

		Window.AllowUserResizing = true;

		IsMouseVisible = true;
	}

	/// <summary>
	/// LoadContent will be called once per game and is the place to load
	/// all of your content.
	/// </summary>
	protected override void LoadContent()
	{
		MyraEnvironment.Game = this;

		var mainForm = new MainForm();
		_desktop = new Desktop
		{
			Root = mainForm
		};
	}

	/// <summary>
	/// This is called when the game should draw itself.
	/// </summary>
	/// <param name="gameTime">Provides a snapshot of timing values.</param>
	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		_desktop.Render();
	}
}
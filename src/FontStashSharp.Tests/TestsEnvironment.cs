using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System.Reflection;

namespace FontStashSharp.Tests
{
	[SetUpFixture]
	public class TestsEnvironment
	{
		private static TestGame _game;

		public static Assembly Assembly => typeof(TestsEnvironment).Assembly;

		public static GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

		[OneTimeSetUp]
		public void SetUp()
		{
			_game = new TestGame();
		}
	}
}

using System;

namespace FontStashSharp
{
	class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				var game = new Game();
				game.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
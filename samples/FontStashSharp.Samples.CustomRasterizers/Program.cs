namespace FontStashSharp.Samples
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var game = new TextRenderingGame())
				game.Run();
		}
	}
}

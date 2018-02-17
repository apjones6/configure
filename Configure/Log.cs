using System;

namespace Configure
{
    static class Log
	{
		public static void Break()
		{
			Console.WriteLine();
		}

		public static void Error(Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.ResetColor();
		}

		public static void Error(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void Info(string message)
		{
			Console.WriteLine(message);
		}
    }
}

using System;

namespace Configure
{
    static class Log
	{
		public static bool Errored { get; private set; }

		public static void Break()
		{
			Console.WriteLine();
		}

		public static void Error(Exception ex)
		{
			Errored = true;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
			Console.ResetColor();
		}

		public static void Error(string message)
		{
			Errored = true;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
		}

		public static void Info(string message, ConsoleColor? color = null)
		{
			if (color != null) Console.ForegroundColor = color.Value;
			Console.WriteLine(message);
			if (color != null) Console.ResetColor();
		}
    }
}

using System;

namespace Configure
{
	class Program
    {
        static void Main(string[] args)
        {
			if (Configuration.TryLoad(out Configuration configuration))
			{
				var i = 0;
				foreach (var node in configuration.Nodes)
				{
					var name = node.Name ?? $"node {i}";
					Console.WriteLine($"Processing {name}...");
					
					foreach (var path in node.ListFiles())
					{
						Console.WriteLine(path);
					}

					Console.WriteLine();
					++i;
				}

				Console.WriteLine("Configure complete...");
				Console.ReadKey();
			}
        }
	}
}
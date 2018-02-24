using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Configure
{
	class Program
	{
		static void Main(string[] args)
		{
			var configuration = Configuration.Load();
			if (configuration == null)
			{
				return;
			}

			for (var i = 0; i < configuration.Nodes.Length; i++)
			{
				var clock = new Stopwatch();
				clock.Start();

				var node = configuration.Nodes[i];
				Log.Info($"NODE[{node.Name ?? (i + 1).ToString()}]");
				
				var tasks = new List<Task>();
				foreach (var file in node.EnumerateFiles().Distinct())
				{
					tasks.Add(Task.Run(() => Execute(file, node)));
				}

				Task.WaitAll(tasks.ToArray());

				clock.Stop();
				Log.Info($"Elapsed {clock.ElapsedMilliseconds}ms");
				Log.Break();
			}

			if (args.Contains("--pause"))
			{
				System.Console.ReadKey();
			}
		}
		
		static void Execute(string file, ConfigureNode node)
		{
			Log.Info($"  {file}");
			var document = node.LoadDocument(file);
			if (document != null)
			{
				var changed = node.ApplyActions(document);
				if (changed)
				{
					node.SaveDocument(document, file);
				}
			}
		}
	}
}
using System;
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
				// TODO: When we have options configuration use pause: false|error|true
				Console.ReadKey();
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
				Console.ReadKey();
			}
		}
		
		static void Execute(string file, ConfigureNode node)
		{
			var document = node.LoadDocument(file);
			if (document != null)
			{
				var changed = node.ApplyActions(document);
				if (changed)
				{
					try
					{
						document.Save(file);
						Log.Info($"  {file}", ConsoleColor.Green);
					}
					catch (Exception ex)
					{
						Log.Error(ex);
					}
				}
				else
				{
					Log.Info($"  {file}");
				}
			}
		}
	}
}
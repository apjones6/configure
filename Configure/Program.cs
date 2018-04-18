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
				// Always pause if we can't read configuration
				Console.ReadKey();
				return;
			}

			var i = 0;
			foreach (var node in configuration.Nodes)
			{
				var clock = new Stopwatch();
				clock.Start();
				
				Log.Info($"NODE[{node.Name ?? (i + 1).ToString()}]");
				
				var tasks = new List<Task>();
				foreach (var file in node.EnumerateFiles().Distinct())
				{
					tasks.Add(Task.Run(() => Execute(file, node, configuration)));
				}

				Task.WaitAll(tasks.ToArray());

				clock.Stop();
				Log.Info($"Elapsed {clock.ElapsedMilliseconds}ms");
				Log.Break();
				++i;
			}

			if (configuration.Pause == PauseMode.True || (configuration.Pause == PauseMode.Error && Log.Errored))
			{
				Console.ReadKey();
			}
		}
		
		static void Execute(string file, ConfigureNode node, Configuration configuration)
		{
			var document = node.LoadDocument(file);
			if (document != null)
			{
				var changed = node.ApplyActions(document);
				if (changed)
				{
					try
					{
						// Don't save file in dry-run, but do *everything* else
						if (!configuration.DryRun)
						{
							document.Save(file);
						}

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
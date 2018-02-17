using System.Linq;

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
				var node = configuration.Nodes[i];
				Log.Info($"Node {node.Name ?? (i + 1).ToString()}...");

				var files = node.ListFiles();
				Log.Info($"Matched {files.Length} files");

				files.AsParallel().ForAll(x => Execute(x, node));
			}
		}

		static void Execute(string path, ConfigureNode node)
		{
			var document = node.LoadDocument(path);
			if (document == null)
			{
				return;
			}

			var changed = node.ApplyActions(document);
			if (changed)
			{
				node.SaveDocument(document, path);
			}
		}
	}
}
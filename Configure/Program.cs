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

				foreach (var path in node.ListFiles())
				{
					var document = node.LoadDocument(path);
					if (document == null)
					{
						continue;
					}

					var changed = node.ApplyActions(document);
					if (changed)
					{
						node.SaveDocument(document, path);
					}
				}
			}
		}
	}
}
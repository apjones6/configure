using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Configure
{
	class ConfigureNode
	{
		public ConfigureAction[] Actions { get; set; }
		public string[] Match { get; set; }
		public string Name { get; set; }

		public IEnumerable<string> EnumerateFiles()
		{
			// Find common base paths of match strings, so we don't enumerate the same root multiple times,
			// and we don't enumerate a child path unnecessarily
			var matchers = Match.Select(x => new PathMatcher(x)).ToList();
			for (var i = 0; i < matchers.Count; i++)
			{
				var match = matchers[i];
				for (var j = i + 1; j < matchers.Count; j++)
				{
					// If [j] was merged into [i], remove [j] and fix index as list items shift
					if (match.Merge(matchers[j]))
					{
						matchers.RemoveAt(j);
						j--;
					}
				}
			}

			// Log invalid paths
			foreach (var matcher in matchers.Where(x => x.IsInvalid))
			{
				Log.Error($"Path \"{matcher.Path}\" not found.");
			}

			// Create an async iterator for each matcher enumeration, then process the first
			// iterator to get its next value each time to interleves results
			var iterators = matchers.Select(x => new AsyncIterator<string>(x.EnumerateFiles())).ToArray();
			var tasks = iterators.Select(x => x.NextAsync()).ToList();
			while (tasks.Any())
			{
				var index = Task.WaitAny(tasks.ToArray());
				var iterator = tasks[index].Result;
				if (iterator.HasCurrent)
				{
					yield return iterator.Current;
					tasks[index] = iterator.NextAsync();
				}
				else
				{
					tasks.RemoveAt(index);
				}
			}
		}

		public XmlDocument LoadDocument(string path)
		{
			try
			{
				using (var stream = File.OpenText(path))
				{
					var document = new XmlDocument { PreserveWhitespace = true };
					document.Load(stream);
					return document;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}
		
		public bool ApplyActions(XmlDocument document)
		{
			var navigator = document.CreateNavigator();
			var changed = false;
			foreach (var action in Actions)
			{
				var a = action.GetAction();
				if (a != null)
				{
					a.Initialize(action, navigator);
					changed |= a.Execute();
				}
			}

			return changed;
		}
	}
}

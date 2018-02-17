﻿using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Configure
{
	public class ConfigureNode
	{
		public ConfigureAction[] Actions { get; set; }
		public string[] Match { get; set; }
		public string Name { get; set; }

		public string[] ListFiles()
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
			foreach (var matcher in matchers.Where(x => !x.IsFile && !x.IsFolder))
			{
				Log.Error($"Path \"{matcher.Path}\" not found.");
			}

			// Enumerate the folders, applying the regexes to filter results, and join
			// with any explicit file paths
			return matchers
				.Where(x => x.IsFolder)
				.AsParallel()
				.SelectMany(x => Directory
					.EnumerateFiles(x.Path, "*.*", SearchOption.AllDirectories)
					.Select(p => p.Replace('\\', '/'))
					.Where(x.IsMatch))
				.ToArray()
				.Union(matchers
					.Where(x => x.IsFile)
					.Select(x => x.Path))
				.OrderBy(x => x)
				.Distinct()
				.ToArray();
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

		public void SaveDocument(XmlDocument document, string path)
		{
			try
			{
				document.Save(path);
				Log.Info($"Saved {path}");
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		public bool ApplyActions(XmlDocument document)
		{
			var navigator = document.CreateNavigator();
			var changed = false;
			foreach (var action in Actions)
			{
				foreach (XPathNavigator node in navigator.Select(action.XPath))
				{
					switch (action.Action)
					{
						case ConfigureActionType.Update:
							if (node.Value != action.Value)
							{
								node.SetValue(action.Value);
								changed = true;
							}

							break;

						case ConfigureActionType.Remove:
							node.DeleteSelf();
							changed = true;
							break;

						default:
							Log.Info($"Action \"{action.Action}\" is not supported.");
							break;
					}
				}
			}

			return changed;
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace Configure
{
	public class ConfigureNode
	{
		public ConfigureAction[] Actions { get; set; }
		public string[] Match { get; set; }
		public string Name { get; set; }

		public IEnumerable<string> ListFiles()
		{
			// TODO: Find common base paths of match strings, so we don't enumerate the same root multiple times,
			// and we don't enumerate a child path unnecessarily

			var paths = new List<string>();
			foreach (var pattern in Match)
			{
				// If the pattern has no asterisks, it's either a single file, or a folder to enumerate all files at any depth
				// Otherwise find the simple base path then filter paths by RegEx
				var index = pattern.IndexOf('*');
				if (index == -1)
				{
					if (Directory.Exists(pattern))
					{
						paths.AddRange(Directory.EnumerateFiles(pattern, "*.*", SearchOption.AllDirectories).Select(x => x.Replace('\\', '/')));
					}
					else if (File.Exists(pattern))
					{
						paths.Add(pattern.Replace('\\', '/'));
					}

					// TODO: Else error
				}
				else
				{
					var pathLength = pattern.LastIndexOf('/', index);
					var path = pattern.Substring(0, pathLength);
					if (Directory.Exists(path))
					{
						var regex = PatternToRegex(pattern);
						paths.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Select(x => x.Replace('\\', '/')).Where(x => regex.IsMatch(x)));
					}

					// TODO: Else error
				}
			}

			return paths
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
		
		// Courtesy of Stack overflow https://stackoverflow.com/a/19655824/527243
		// and modified to support /**/ and /*/
		private static Regex PatternToRegex(string pattern)
		{
			var mask = "^" + Regex.Escape(pattern).Replace("\\*\\*/", "([^/]+/)*").Replace("\\*", "[^/]+").Replace("\\?", ".") + "$";
			return new Regex(mask, RegexOptions.IgnoreCase);
		}
	}
}

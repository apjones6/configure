using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace Configure.Actions
{
	class CreateAction : UpdateAction
	{
		private static readonly Regex EQUALITY_PREDICATE = new Regex("^\\[@(?<key>.*)=(\"|')(?<value>.*)(\"|')\\]$");

		public override bool Execute()
		{
			var changed = false;
			XPathNavigator node;

			// Break down the path to walk up the document until we find a node which exists
			var breakdown = BreakdownPath().ToArray();
			var i = -1;
			do
			{
				++i;
				node = Navigator.Select(breakdown[i]).Cast<XPathNavigator>().FirstOrDefault();
			}
			while (node == null && i < breakdown.Length);

			if (node == null)
			{
				Log.Error($"Path \"{Action.Path}\" has no matched roots.");
				return false;
			}

			// From the point which didn't exist (if any) walk back down the path creating nodes and
			// attributes (i starts on the first found node)
			while (--i >= 0)
			{
				var diff = breakdown[i].Substring(breakdown[i + 1].Length);
				if (diff.StartsWith("/@"))
				{
					node.CreateAttribute(null, diff.Substring(2), null, null);
				}
				else if (diff.StartsWith("/"))
				{
					var attributes = new Dictionary<string, string>();
					string name;

					var openBracket = diff.IndexOf('[');
					if (openBracket != -1)
					{
						name = diff.Substring(1, diff.IndexOf('[') - 1);
						do
						{
							var closeBracket = diff.IndexOf(']', openBracket);
							var match = EQUALITY_PREDICATE.Match(diff.Substring(openBracket, closeBracket + 1 - openBracket));
							if (match.Success)
							{
								attributes[match.Groups["key"].Value] = match.Groups["value"].Value;
							}
							else
							{
								Log.Error($"Path \"{Action.Path}\" cannot be used to create XML.");
								return false;
							}

							openBracket = diff.IndexOf('[', closeBracket);
						}
						while (openBracket != -1);
					}
					else
					{
						name = diff.Substring(1);
					}
					
					using (var writer = node.AppendChild())
					{
						writer.WriteStartElement(name);
						foreach (var attribute in attributes)
						{
							writer.WriteAttributeString(attribute.Key, attribute.Value);
						}

						writer.WriteEndElement();
						changed = true;
					}
				}
				else
				{
					Log.Error($"Path \"{Action.Path}\" cannot be parsed.");
					return false;
				}

				// Select the new node/attribute
				node = node.Select(breakdown[i]).Cast<XPathNavigator>().Single();
			}

			// Finally set the value of the last node or attribute
			changed |= base.Execute();

			return changed;
		}

		private IEnumerable<string> BreakdownPath()
		{
			var path = Action.Path;
			yield return path;

			do
			{
				//var last = path.LastIndexOfAny(new[] { '/', '[' });
				var last = path.LastIndexOf('/');
				if (last > 0)
				{
					path = path.Substring(0, last);
					yield return path;
				}
				else
				{
					break;
				}
			}
			while (true);
		}
	}
}

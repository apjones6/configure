using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Configure
{
	class Program
    {
        static void Main(string[] args)
        {
			// Load configuration
			Configuration configuration;
			try
			{
				configuration = Configuration.Load();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex);
				Console.ResetColor();
				return;
			}

			// Write default
			if (configuration == null)
			{
				var path = Configuration.WriteDefault();
				Console.WriteLine($"Created {path}");
				return;
			}

			var i = 0;
			foreach (var node in configuration.Nodes)
			{
				var name = node.Name ?? $"node {i}";
				Console.WriteLine($"Processing {name}...");

				var paths = new List<string>();
				foreach (var pattern in node.Match)
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

				foreach (var path in paths.OrderBy(x => x).Distinct())
				{
					Console.WriteLine(path);
				}

				Console.WriteLine();
				++i;
			}

			Console.WriteLine("Configure complete...");
			Console.ReadKey();
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
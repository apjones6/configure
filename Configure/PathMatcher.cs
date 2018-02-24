using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Configure
{
	class PathMatcher
    {
		private string[] extensionPatterns = new[] { "*.*" };

		public PathMatcher(string pattern)
		{
			var path = pattern.Replace('\\', '/').TrimEnd('/');

			var index = path.IndexOf('*');
			if (index == -1)
			{
				Path = path;
				if (Directory.Exists(path))
				{
					IsFolder = true;
				}
				else if (File.Exists(path))
				{
					IsFile = true;
				}
			}
			else
			{
				Path = path.Substring(0, path.LastIndexOf('/', index));
				if (Directory.Exists(Path))
				{
					var last = path.Substring(path.LastIndexOf('/') + 1);
					if (last.Contains('.'))
					{
						extensionPatterns = new[] { last };
					}

					IsFolder = true;
					Regexes = new[] { PatternToRegex(path) };
				}
			}
		}

		public bool IsFile { get; }

		public bool IsFolder { get; }

		public string Path { get; private set; }

		public Regex[] Regexes { get; private set; }

		private Regex[] RegexesInternal
		{
			get
			{
				if (Regexes == null)
				{
					Regexes = new[] { new Regex($"^{Regex.Escape(Path)}/", RegexOptions.IgnoreCase) };
				}

				return Regexes;
			}
		}

		public IEnumerable<string> EnumerateFiles()
		{
			if (IsFile)
			{
				yield return Path;
			}
			else if (!IsFolder)
			{
				yield break;
			}
			else if (extensionPatterns.Contains("*.*"))
			{
				foreach (var path in Directory.EnumerateFiles(Path, "*.*", SearchOption.AllDirectories))
				{
					yield return path;
				}
			}
			else
			{
				var iterators = extensionPatterns.Select(x => new AsyncIterator<string>(Directory.EnumerateFiles(Path, x, SearchOption.AllDirectories))).ToArray();
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
		}

		public bool IsMatch(string path)
		{
			return Regexes == null || Regexes.Any(x => x.IsMatch(path));
		}

		public bool Merge(PathMatcher matcher)
		{
			var merge = false;
			if (IsFolder && matcher.IsFolder)
			{
				if (Path.StartsWith(matcher.Path))
				{
					Path = matcher.Path;
					merge = true;
				}
				else if (matcher.Path.StartsWith(Path))
				{
					merge = true;
				}

				if (merge)
				{
					extensionPatterns = extensionPatterns.Union(matcher.extensionPatterns).Distinct().ToArray();
					Regexes = matcher.RegexesInternal.Union(RegexesInternal).ToArray();
				}
			}

			return merge;
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

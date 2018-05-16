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
		private readonly bool isFile;
		private bool isFolder;
		private Regex[] regexes;

		public PathMatcher(string pattern)
		{
			// If the path isn't rooted, make it relative to the current directory
			if (!Path.IsPathRooted(pattern)) pattern = Path.Combine(Directory.GetCurrentDirectory(), pattern);
			
			var path = pattern.Replace('\\', '/').TrimEnd('/');

			var index = path.IndexOf('*');
			if (index == -1)
			{
				BasePath = path;
				if (Directory.Exists(path))
				{
					isFolder = true;
				}
				else if (File.Exists(path))
				{
					isFile = true;
				}
			}
			else
			{
				BasePath = path.Substring(0, path.LastIndexOf('/', index));
				if (Directory.Exists(BasePath))
				{
					var last = path.Substring(path.LastIndexOf('/') + 1);
					if (last.Contains('.'))
					{
						extensionPatterns = new[] { last };
					}

					isFolder = true;
					regexes = new[] { PatternToRegex(path) };
				}
			}
		}
		
		public bool IsInvalid
		{
			get { return !isFile && !isFolder; }
		}

		public string BasePath { get; private set; }
		
		private Regex[] RegexesInternal
		{
			get
			{
				if (regexes == null)
				{
					regexes = new[] { new Regex($"^{Regex.Escape(BasePath)}/", RegexOptions.IgnoreCase) };
				}

				return regexes;
			}
		}

		public IEnumerable<string> EnumerateFiles()
		{
			if (isFile)
			{
				yield return BasePath;
			}
			else if (!isFolder)
			{
				yield break;
			}
			else if (extensionPatterns.Contains("*.*"))
			{
				foreach (var path in Directory.EnumerateFiles(BasePath, "*.*", SearchOption.AllDirectories))
				{
					yield return path;
				}
			}
			else
			{
				// Create an async iterator for each directory enumeration, then process the first
				// iterator to get its next value each time to interleves results
				var iterators = extensionPatterns.Select(x => new AsyncIterator<string>(Directory.EnumerateFiles(BasePath, x, SearchOption.AllDirectories), p => p.Replace('\\', '/'), p => IsMatch(p))).ToArray();
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

		private bool IsMatch(string path)
		{
			return regexes == null || regexes.Any(x => x.IsMatch(path));
		}

		public bool Merge(PathMatcher matcher)
		{
			var merge = false;
			if (isFolder && matcher.isFolder)
			{
				if (BasePath.StartsWith(matcher.BasePath))
				{
					BasePath = matcher.BasePath;
					merge = true;
				}
				else if (matcher.BasePath.StartsWith(BasePath))
				{
					merge = true;
				}

				if (merge)
				{
					extensionPatterns = extensionPatterns.Union(matcher.extensionPatterns).Distinct().ToArray();
					regexes = matcher.RegexesInternal.Union(RegexesInternal).ToArray();
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

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Configure
{
	class PathMatcher
    {
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

		public bool IsMatch(string path)
		{
			return Regexes == null || Regexes.Any(x => x.IsMatch(path));
		}

		public bool Merge(PathMatcher matcher)
		{
			if (IsFolder && matcher.IsFolder)
			{
				if (Path.StartsWith(matcher.Path))
				{
					Path = matcher.Path;
					Regexes = matcher.RegexesInternal.Union(RegexesInternal).ToArray();
					return true;
				}
				else if (matcher.Path.StartsWith(Path))
				{
					Regexes = matcher.RegexesInternal.Union(RegexesInternal).ToArray();
					return true;
				}
			}

			return false;
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

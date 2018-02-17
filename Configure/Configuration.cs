using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Configure
{
	public class Configuration
	{
		private const string CONFIGURATION_FILE = "configure.yaml";
		private const string DEFAULT_YAML = @"---
match:
  - C:/wip/websites/**/web.config
  - C:/wip/nuget/**/app.config
  - C:/wip/nuget/**/*.exe.config
  - C:/wip/nuget/**/web.config
actions:
  - path: //appSettings/add[@key='MyApplication.Homepage']/@value
    value: http://www.aj.co.uk
    action: create
  - path: //appSettings/add[@key='MyApplication.SqlDatabase']
    action: remove
  - path: //appSettings/add[@key='MyApplication.MongoDB']/@value
    value: http://localhost:27017
  - appSetting: MyApplication.AdminUsername
    value: RichTea
...
";

		private Configuration(IEnumerable<ConfigureNode> nodes)
		{
			Nodes = nodes.ToArray();
		}

		public ConfigureNode[] Nodes { get; }

		public static bool TryLoad(out Configuration configuration)
		{
			if (!File.Exists(CONFIGURATION_FILE))
			{
				File.WriteAllText(CONFIGURATION_FILE, DEFAULT_YAML);
				var path = Path.GetFullPath(CONFIGURATION_FILE);
				Console.WriteLine($"Created {path}");

				configuration = null;
				return false;
			}
			
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(new CamelCaseNamingConvention())
				.IgnoreUnmatchedProperties()
				.Build();

			try
			{
				using (var stream = File.OpenText(CONFIGURATION_FILE))
				{
					var nodes = new List<ConfigureNode>();
					var parser = new Parser(stream);
					parser.Expect<StreamStart>();
					while (parser.Accept<DocumentStart>())
					{
						nodes.Add(deserializer.Deserialize<ConfigureNode>(parser));
					}

					configuration = new Configuration(nodes);
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex);
				Console.ResetColor();

				configuration = null;
				return false;
			}
		}
	}
}

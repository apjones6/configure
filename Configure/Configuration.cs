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

		public static Configuration Load()
		{
			if (!File.Exists(CONFIGURATION_FILE))
			{
				return null;
			}
			
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(new CamelCaseNamingConvention())
				.IgnoreUnmatchedProperties()
				.Build();
			
			using (var stream = File.OpenText(CONFIGURATION_FILE))
			{
				var nodes = new List<ConfigureNode>();
				var parser = new Parser(stream);
				parser.Expect<StreamStart>();
				while (parser.Accept<DocumentStart>())
				{
					nodes.Add(deserializer.Deserialize<ConfigureNode>(parser));
				}

				return new Configuration(nodes);
			}
		}

		public static string WriteDefault()
		{
			File.WriteAllText(CONFIGURATION_FILE, DEFAULT_YAML);
			return Path.GetFullPath(CONFIGURATION_FILE);
		}
	}

	public class ConfigureNode
	{
		public ConfigureAction[] Actions { get; set; }
		public string[] Match { get; set; }
		public string Name { get; set; }
	}

	public class ConfigureAction
	{
		public ConfigureActionType Action { get; set; }
		public string AppSetting { get; set; }
		public string Path { get; set; }
		public string Value { get; set; }
	}

	public enum ConfigureActionType
	{
		Update,
		Create,
		Remove
	}
}

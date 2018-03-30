using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Configure
{
	class Configuration
	{
		private static readonly Deserializer DESERIALIZER = new DeserializerBuilder()
			.WithNamingConvention(new CamelCaseNamingConvention())
			.IgnoreUnmatchedProperties()
			.Build();

		private static readonly Serializer SERIALIZER = new SerializerBuilder()
			.JsonCompatible()
			.Build();
		
		private const string CONFIGURATION_FILE = "configure.yaml";

		public Configuration()
		{
			Nodes = new List<ConfigureNode>();
		}

		public bool DryRun { get; private set; }

		public List<ConfigureNode> Nodes { get; }

		public PauseMode Pause { get; private set; }

		public static Configuration Load()
		{
			if (!File.Exists(CONFIGURATION_FILE))
			{
				var assembly = Assembly.GetExecutingAssembly();
				using (var inStream = assembly.GetManifestResourceStream("Configure.Data.configure.yaml"))
				{
					using (var outStream = File.OpenWrite(CONFIGURATION_FILE))
					{
						inStream.CopyTo(outStream);
					}
				}

				var path = Path.GetFullPath(CONFIGURATION_FILE);
				Log.Info($"Created {path}");
				return null;
			}
			
			try
			{
				using (var stream = File.OpenText(CONFIGURATION_FILE))
				{
					var configuration = new Configuration();
					var parser = new Parser(stream);

					parser.Expect<StreamStart>();
					var first = true;
					while (parser.Accept<DocumentStart>())
					{
						// Deserialize to JSON
						var document = DESERIALIZER.Deserialize(parser);
						var json = JObject.Parse(SERIALIZER.Serialize(document));

						// If the first document doesn't have a "match" property it instead contains
						// configuration options, so process it as a special case
						if (first && !json.ContainsKey("match"))
						{
							configuration.DryRun = json["dry-run"] != null ? json.Value<bool>("dry-run") : false;
							configuration.Pause = json["pause"] != null && Enum.TryParse(json["pause"].ToString(), true, out PauseMode pause) ? pause : PauseMode.False;
						}
						else
						{
							configuration.Nodes.Add(json.ToObject<ConfigureNode>());
						}

						first = false;
					}
					
					return configuration;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}
	}
}

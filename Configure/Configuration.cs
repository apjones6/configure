using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Configure
{
	class Configuration
	{
		private const string CONFIGURATION_FILE = "configure.yaml";
		private static readonly Deserializer DESERIALIZER = new DeserializerBuilder()
			.WithNamingConvention(new CamelCaseNamingConvention())
			// TODO: Uncomment and remove Aliases property when https://github.com/aaubry/YamlDotNet/issues/295 resolved
			//.IgnoreUnmatchedProperties()
			.Build();

		public Configuration()
		{
			Nodes = new ConfigureNode[0];
		}
		
		public YamlMappingNode Aliases { get; set; }
		[YamlMember(Alias = "dry-run", ApplyNamingConventions = false)]
		public bool DryRun { get; set; }
		public ConfigureNode[] Nodes { get; set; }
		public PauseMode Pause { get; set; }

		public static Configuration Load()
		{
			try
			{
				using (var stream = File.OpenText(CONFIGURATION_FILE))
				{
					var yaml = new YamlStream();
					yaml.Load(stream);

					if (yaml.Documents.Any())
					{
						var parser = new EventStreamParserAdapter(ConvertToEventStream(yaml.Documents[0].RootNode));
						return DESERIALIZER.Deserialize<Configuration>(parser);
					}

					return new Configuration();
				}
			}
			catch (FileNotFoundException)
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
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}

			return null;
		}

		#region Custom Deserialization

		private class EventStreamParserAdapter : IParser
		{
			private readonly IEnumerator<ParsingEvent> enumerator;

			public EventStreamParserAdapter(IEnumerable<ParsingEvent> events)
			{
				enumerator = events.GetEnumerator();
			}

			public ParsingEvent Current => enumerator.Current;

			public bool MoveNext()
			{
				return enumerator.MoveNext();
			}
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlNode node)
		{
			if (node is YamlScalarNode scalar)
			{
				return ConvertToEventStream(scalar);
			}

			if (node is YamlSequenceNode sequence)
			{
				return ConvertToEventStream(sequence);
			}

			if (node is YamlMappingNode mapping)
			{
				return ConvertToEventStream(mapping);
			}

			throw new NotSupportedException($"Unsupported node type: {node.GetType().Name}");
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlScalarNode scalar)
		{
			yield return new Scalar(scalar.Anchor, scalar.Tag, scalar.Value, scalar.Style, false, false);
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlSequenceNode sequence)
		{
			yield return new SequenceStart(sequence.Anchor, sequence.Tag, false, sequence.Style);
			foreach (var node in sequence.Children)
			{
				foreach (var evt in ConvertToEventStream(node))
				{
					yield return evt;
				}
			}

			yield return new SequenceEnd();
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlMappingNode mapping)
		{
			yield return new MappingStart(mapping.Anchor, mapping.Tag, false, mapping.Style);
			foreach (var pair in mapping.Children)
			{
				foreach (var evt in ConvertToEventStream(pair.Key))
				{
					yield return evt;
				}

				foreach (var evt in ConvertToEventStream(pair.Value))
				{
					yield return evt;
				}
			}

			yield return new MappingEnd();
		}

		#endregion
	}
}

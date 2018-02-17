using System.Xml.XPath;

namespace Configure
{
	public class ConfigureAction
	{
		public ConfigureActionType Action { get; set; }
		public string AppSetting { get; set; }
		public string Path { get; set; }
		public string Value { get; set; }
		public XPathExpression XPath
		{
			get
			{
				var path = Path ?? (AppSetting != null ? $"//appSettings/add[@key='{AppSetting}']/@value" : null);
				if (path != null)
				{
					return XPathExpression.Compile(path);
				}

				return null;
			}
		}
	}

	public enum ConfigureActionType
	{
		Update,
		Create,
		Remove
	}
}

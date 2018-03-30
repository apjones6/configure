using Configure.Actions;
using System;
using System.Xml.XPath;

namespace Configure
{
	class ConfigureAction
	{
		private string path;

		public ConfigureActionType Action { get; set; }
		public string AppSetting { get; set; }
		public string Path
		{
			get
			{
				if (path == null && AppSetting != null)
				{
					// Update or create the value, but remove the entire element
					if (Action != ConfigureActionType.Remove)
					{
						return $"/configuration/appSettings/add[@key='{AppSetting}']/@value";
					}
					else
					{
						return $"/configuration/appSettings/add[@key='{AppSetting}']";
					}
				}

				return path;
			}
			set { path = value; }
		}

		public string Value { get; set; }
		public XPathExpression XPath
		{
			get
			{
				var path = Path;
				return path != null ? XPathExpression.Compile(path) : null;
			}
		}

		public IAction GetAction()
		{
			try
			{
				var type = Type.GetType($"Configure.Actions.{Action}Action, Configure");
				return (IAction)Activator.CreateInstance(type);
			}
			catch (Exception ex)
			{
				Log.Error($"Action \"{Action}\" is not supported.");
				Log.Error(ex.Message);
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

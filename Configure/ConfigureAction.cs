﻿using Configure.Actions;
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
					return $"/configuration/appSettings/add[@key='{AppSetting}']/@value";
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

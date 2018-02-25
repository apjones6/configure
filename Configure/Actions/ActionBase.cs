using System.Xml.XPath;

namespace Configure.Actions
{
    abstract class ActionBase : IAction
	{
		protected ConfigureAction Action { get; private set; }

		protected XPathNavigator Navigator { get; private set; }

		public virtual void Initialize(ConfigureAction action, XPathNavigator navigator)
		{
			Action = action;
			Navigator = navigator;
		}

		public abstract bool Execute();
	}
}

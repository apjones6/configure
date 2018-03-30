using System.Xml.XPath;

namespace Configure.Actions
{
    interface IAction
    {
		void Initialize(ConfigureAction action, XPathNavigator navigator);
		bool Execute();
	}
}

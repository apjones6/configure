using System.Xml.XPath;

namespace Configure.Actions
{
    class RemoveAction : ActionBase
	{
		public override bool Execute()
		{
			var changed = false;
			foreach (XPathNavigator node in Navigator.Select(Action.XPath))
			{
				node.DeleteSelf();
				changed = true;
			}

			return changed;
		}
    }
}

using System.Linq;
using System.Xml.XPath;

namespace Configure.Actions
{
    class RemoveAction : ActionBase
	{
		public override bool Execute()
		{
			var changed = false;
			var nodes = Navigator.Select(Action.XPath).Cast<XPathNavigator>().ToArray();
			foreach (var node in nodes)
			{
				node.DeleteSelf();
				changed = true;
			}

			return changed;
		}
    }
}

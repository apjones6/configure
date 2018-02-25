using System.Xml.XPath;

namespace Configure.Actions
{
    class UpdateAction : ActionBase
	{
		public override bool Execute()
		{
			var changed = false;
			foreach (XPathNavigator node in Navigator.Select(Action.XPath))
			{
				if (node.Value != Action.Value)
				{
					node.SetValue(Action.Value);
					changed = true;
				}
			}

			return changed;
		}
    }
}

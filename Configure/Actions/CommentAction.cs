using System.Linq;
using System.Xml.XPath;

namespace Configure.Actions
{
    class CommentAction : ActionBase
	{
		public override bool Execute()
		{
			var changed = false;
			var nodes = Navigator.Select(Action.XPath).Cast<XPathNavigator>().Where(x => x.NodeType == XPathNodeType.Element).ToArray();
			foreach (var node in nodes)
			{
				// Defend against XML which already contains comments (or other problematic content)
				var xml = node.OuterXml.Replace("--", "- -");
				node.ReplaceSelf($"<!--{xml}-->");
				changed = true;
			}

			return changed;
		}
    }
}

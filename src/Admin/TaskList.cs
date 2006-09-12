using System.IO;
using System.Windows.Forms;
using System.Xml;
using WeSay.UI;

namespace Admin
{
	public partial class TaskList : UserControl
	{

		public TaskList()
		{
			InitializeComponent();

			string path = Path.Combine(BasilProject.Project.ApplicationCommonDirectory, "taskInventory.xml");
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			foreach (XmlNode node in doc.SelectNodes("tasks/task"))
			{
				_taskList.Items.Add(new TaskInfo(node));
			}
		}
	}

	public class TaskInfo
	{
		XmlNode _node;
		public TaskInfo(XmlNode node)
		{
			_node = node;
		}

		public override string ToString()
		{
			try
			{
				return this._node.SelectSingleNode("tool/config/label").InnerText;

			}
			catch (System.Exception)
			{
				return GetOptionalAttributeString(_node, "id", "task");
			}
		}

		private string GetOptionalAttributeString(XmlNode xmlNode, string name, string defaultValue)
		{
			XmlAttribute attr = xmlNode.Attributes[name];
			if (attr == null)
				return defaultValue;
			return attr.Value;
		}

	}
}

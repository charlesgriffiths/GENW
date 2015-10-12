using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

class DialogResponse
{
	public string text = "", name = "", jump = "";

	public DialogResponse(XmlNode xnode)
	{
		text = MyXml.GetString(xnode, "text");
		name = MyXml.GetString(xnode, "name");
		jump = MyXml.GetString(xnode, "jump");
	}
}

class DialogNode
{
	public string text, name;
	public Collection<DialogResponse> responses = new Collection<DialogResponse>();

	public DialogNode(XmlNode node)
	{
		text = MyXml.GetString(node, "text");
		name = MyXml.GetString(node, "name");

		XmlNode xnode = node.FirstChild;
		while (xnode != null)
		{
			DialogResponse temp = new DialogResponse(xnode);
			responses.Add(temp);
			xnode = xnode.NextSibling;
		}
	}
}

class Dialog : NamedObject
{
	public Dictionary<string, DialogNode> nodes = new Dictionary<string, DialogNode>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");

		XmlNode node = xnode.FirstChild;
		while (node != null)
		{
			DialogNode temp = new DialogNode(node);
			nodes.Add(temp.name, temp);
			node = node.NextSibling;
		}
	}
/*
	public static void LoadBase()
	{
		// повторяющийся код с тайлами, очевидно. Потом исправить!
		Log.Write("loading the dialog database... ");
		XmlNode xnode = MyXml.SecondChild("dialogs.xml");

		while (xnode != null)
		{
			Dialog temp = new Dialog();	// вот эти 2 строчки превратить в одну, сделать специальный конструктор просто.
			temp.Load((XmlElement)xnode); // аналогично для тайлов.
			BigBase.Instance.dialogBase.Add(temp);
			xnode = xnode.NextSibling;
		}

		BigBase.Instance.dialogBase.loaded = true;
		Log.WriteLine("OK");
	}
*/
}

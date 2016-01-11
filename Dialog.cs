using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

public class DialogResponse
{
	public string text, action, jump, condition;

	public DialogResponse(XmlNode xnode)
	{
		text = MyXml.GetString(xnode, "text");
		action = MyXml.GetString(xnode, "action");
		jump = MyXml.GetString(xnode, "jump");
		condition = MyXml.GetString(xnode, "condition");
	}
}

public class DialogNode
{
	public string text, name, description;
	public Collection<DialogResponse> responses = new Collection<DialogResponse>();

	public DialogNode(XmlNode node)
	{
		text = MyXml.GetString(node, "text");
		name = MyXml.GetString(node, "name");
		description = MyXml.GetString(node, "description");

		XmlNode xnode = node.FirstChild;
		while (xnode != null)
		{
			DialogResponse temp = new DialogResponse(xnode);
			responses.Add(temp);
			xnode = xnode.NextSibling;
		}
	}
}

public class Dialog : NamedObject
{
	public Dictionary<string, DialogNode> nodes = new Dictionary<string, DialogNode>();

	public bool isUnique;
	public bool happened = false;

	private string picture;
	public Texture2D texture;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		isUnique = MyXml.GetBool(xnode, "unique");
		picture = MyXml.GetString(xnode, "picture");		

		XmlNode node = xnode.FirstChild;
		while (node != null)
		{
			DialogNode temp = new DialogNode(node);
			nodes.Add(temp.name, temp);
			node = node.NextSibling;
		}
	}

	public static void LoadTextures()
	{
		foreach (Dialog d in BigBase.Instance.dialogs.data)
			d.texture = MainScreen.Instance.game.Content.Load<Texture2D>(d.picture);
	}
}

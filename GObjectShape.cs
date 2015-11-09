using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class GObjectShape : NamedObject
{
	private string textureName;
	public Texture2D texture;
	public float speed;
	public bool isActive;
	public Dialog dialog;

	public Dictionary<string, int> partyShape = new Dictionary<string, int>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		textureName = MyXml.GetString(xnode, "icon");
		if (textureName == "") textureName = name;
		isActive = MyXml.GetBool(xnode, "active");

		if (isActive) speed = MyXml.GetFloat(xnode, "speed");
		else speed = 1.0f;

		string dialogName = MyXml.GetString(xnode, "dialog");
		if (dialogName != "") dialog = BigBase.Instance.dialogs.Get(dialogName);

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
			partyShape.Add(MyXml.GetString(xnode, "name"), MyXml.GetInt(xnode, "quantity"));
	}

	public static void LoadTextures()
	{
		foreach (GObjectShape s in BigBase.Instance.gShapes.data)
			s.texture = MainScreen.Instance.game.Content.Load<Texture2D>("global/" + s.textureName);
	}
}

partial class GObject
{
	public GObject(GObjectShape shapei)
	{
		shape = shapei;
		dialog = shapei.dialog;

		foreach (KeyValuePair<string, int> pair in shape.partyShape)
		{
			for (int i = 0; i < pair.Value; i++)
			{
				Creep item = new Creep(pair.Key);
				party.Add(item);
			}
		}

		initiative = -0.1f;
	}
}
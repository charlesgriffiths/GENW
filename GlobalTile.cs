using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class GlobalTileType : NamedObject
{
	public bool isWalkable, isFlat;
	public float travelTime;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");
		travelTime = MyXml.GetFloat(xnode, "time");
	}
}

public class GlobalTile : NamedObject
{
	public Texture2D texture, topTexture;
	public string picture;
	public GlobalTileType type;
	public bool hasTop;

	public List<CraftingComponent> components = new List<CraftingComponent>();
	public List<ItemShape> items = new List<ItemShape>();
	
	public bool IsWalkable { get { return type.isWalkable; } }
	public bool IsFlat { get { return type.isFlat; } }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		type = BigBase.Instance.globalTileTypes.Get(MyXml.GetString(xnode, "type"));
		picture = MyXml.GetString(xnode, "picture");
		hasTop = MyXml.GetBool(xnode, "top");

		for (XmlNode secondNode = xnode.FirstChild; secondNode != null; secondNode = secondNode.NextSibling)
		{
			if (secondNode.Name == "components")
				for (XmlNode thirdNode = secondNode.FirstChild; thirdNode != null; thirdNode = thirdNode.NextSibling)
					components.Add(CraftingComponent.Get(MyXml.GetString(thirdNode, "name")));
			else if (secondNode.Name == "items")
				for (XmlNode thirdNode = secondNode.FirstChild; thirdNode != null; thirdNode = thirdNode.NextSibling)
				{
					int amount = MyXml.GetInt(thirdNode, "amount", 1);
					string name = MyXml.GetString(thirdNode, "name");
					for (int i = 0; i < amount; i++) items.Add(ItemShape.Get(name));
				}
		}
	}

	public static void LoadTextures()
	{
		foreach (GlobalTile t in BigBase.Instance.globalTiles.data)
		{
			t.texture = MainScreen.Instance.game.Content.Load<Texture2D>("terrain/" + t.picture);
			if (t.hasTop) t.topTexture = MainScreen.Instance.game.Content.Load<Texture2D>("terrain/" + t.picture + " Top");
		}
	}
}
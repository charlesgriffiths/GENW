using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class LocalTileType : NamedObject
{
	public bool isWalkable, isFlat;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");
	}
}

public class LocalTile : NamedObject
{
	public Texture texture;
	public LocalTileType type;
	public Dictionary<LocalShape, double> spawns = new Dictionary<LocalShape, double>();
	public int variations;

	public bool IsWalkable { get { return type.isWalkable; } }
	public bool IsFlat { get { return type.isFlat; } }

	public override void Load(XmlNode xnode)
	{
		texture = new Texture();

		name = MyXml.GetString(xnode, "name");
		type = BB.localTileTypes.Get(MyXml.GetString(xnode, "type"));
		variations = MyXml.GetInt(xnode, "variations", 1);

		for (XmlNode secondNode = xnode.FirstChild; secondNode != null; secondNode = secondNode.NextSibling)
			spawns.Add(LocalShape.Get(MyXml.GetString(secondNode, "name")), MyXml.GetFloat(secondNode, "p"));
	}

	public static void LoadTextures()
	{
		foreach (LocalTile t in BigBase.Instance.localTiles.data)
		{
			//lTile.texture = MainScreen.Instance.game.Content.Load<Texture2D>("tiles/" + lTile.name);
			t.texture.LoadImages("tiles/" + t.name, t.variations);
		}
	}

	public static LocalTile Get(string name) { return BB.localTiles.Get(name); }
}
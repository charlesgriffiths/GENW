using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class GTileType : NamedObject
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

class GTile : NamedObject
{
	public Texture2D texture, topTexture;
	public string picture;
	public GTileType type;
	public bool hasTop;
	
	public bool IsWalkable { get { return type.isWalkable; } }
	public bool IsFlat { get { return type.isFlat; } }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		type = BigBase.Instance.gTileTypes.Get(MyXml.GetString(xnode, "type"));
		picture = MyXml.GetString(xnode, "picture");
		hasTop = MyXml.GetBool(xnode, "top");
	}

	public static void LoadTextures()
	{
		foreach (GTile t in BigBase.Instance.gTiles.data)
		{
			t.texture = MainScreen.Instance.game.Content.Load<Texture2D>("terrain/" + t.picture);
			if (t.hasTop) t.topTexture = MainScreen.Instance.game.Content.Load<Texture2D>("terrain/" + t.picture + " Top");
		}
	}
}

class LTileType : NamedObject
{
	public bool isWalkable, isFlat;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");
	}
}

class LTile : NamedObject
{
	public Texture2D texture;
	private LTileType type;

	public bool IsWalkable { get { return type.isWalkable; } }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		type = BigBase.Instance.lTileTypes.Get(MyXml.GetString(xnode, "type"));
	}

	public static void LoadTextures()
	{
		foreach (LTile lTile in BigBase.Instance.lTiles.data)
		{
			lTile.texture = MainScreen.Instance.game.Content.Load<Texture2D>("tiles/" + lTile.name);
		}
	}
}
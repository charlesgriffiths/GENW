using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class GTileType : NamedObject
{
	public bool isWalkable, isFlat;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");
	}
}

class GTile : NamedObject
{
	public Texture2D texture;
	public string picture;// type;
	public GTileType type;
	
	public bool IsWalkable
	{
		get
		{
			return type.isWalkable;
			//if (type == "ground" || type == "forest" || type == "hills" || type == "death" || type == "mountainPass") return true;
			//else return false;
		}
	}

	public bool IsFlat
	{
		get
		{
			return type.isFlat;
			//if (type == "forest" || type == "hills" || type == "mountains" || type == "mountainPass") return false;
			//else return true;
		}
	}

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		//type = MyXml.GetString(xnode, "type");
		type = BigBase.Instance.gTileTypes.Get(MyXml.GetString(xnode, "type"));
		picture = MyXml.GetString(xnode, "picture");
	}

	public static void LoadTextures()
	{
		foreach (GTile gTile in BigBase.Instance.gTiles.data)
		{
			gTile.texture = MainScreen.Instance.game.Content.Load<Texture2D>("terrain/" + gTile.picture);
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
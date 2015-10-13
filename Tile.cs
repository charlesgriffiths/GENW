//Если функциональность этих классов сильно не изменится, то их можно унаследовать из abstract Tile, используя обобщения.
//Аналогично Map и Battlefield. Короче, ту часть кода можно сделать существенно более красивой, но сейчас это не стоит того.
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GTile : NamedObject
{
	public Texture2D texture;
	public string picture, type;
	
	public bool IsWalkable
	{
		get
		{
			if (type == "ground" || type == "forest" || type == "hills") return true;
			else return false;
		}
	}

	public bool IsFlat
	{
		get
		{
			if (type == "forest" || type == "hills" || type == "mountains") return false;
			else return true;
		}
	}

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		type = MyXml.GetString(xnode, "type");
		picture = MyXml.GetString(xnode, "picture");
	}

	public static void LoadTextures(Game game)
	{
		foreach (GTile gTile in BigBase.Instance.gTiles.data)
		{
			gTile.texture = game.Content.Load<Texture2D>("t" + gTile.picture);
		}
	}
}

class LTile : NamedObject
{
	public Texture2D texture;
	private string picture;
	public string type;

	public bool IsWalkable
	{
		get
		{
			if (type == "ground" || type == "shallowWater") return true;
			else return false;
		}
	}

	public bool IsFlat
	{
		get
		{
			if (type == "tree" || type == "wall") return false;
			else return true;
		}
	}

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		type = MyXml.GetString(xnode, "type");
		picture = MyXml.GetString(xnode, "picture");
	}

	public static void LoadTextures(Game game)
	{
		foreach (LTile lTile in BigBase.Instance.lTiles.data)
		{
			lTile.texture = game.Content.Load<Texture2D>("_" + lTile.picture);
		}
	}
}
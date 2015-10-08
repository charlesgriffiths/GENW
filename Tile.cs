using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GlobalTile : NamedObject
{
	public Texture2D texture;
	private string picture;
	public string type;
	
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

	public void Load(XmlElement xl)
	{
		name = xl.GetAttribute("name");
		type = xl.GetAttribute("type");
		picture = xl.GetAttribute("picture");
    }

	public static void LoadBase()
	{
		Log.Write("loading tile base... ");
		XmlNode xnode = MyXml.SecondChild("globalTiles.xml");

		while (xnode != null)
		{
			GlobalTile temp = new GlobalTile();
			temp.Load((XmlElement)xnode);
			BigBase.Instance.globalTileBase.Add(temp);
			xnode = xnode.NextSibling;
		}

		BigBase.Instance.globalTileBase.loaded = true;
		Log.WriteLine("OK");
	}

	public static void LoadTextures(Game game)
	{
		foreach (GlobalTile globalTile in BigBase.Instance.globalTileBase.data)
		{
			globalTile.texture = game.Content.Load<Texture2D>(globalTile.picture);
		}
	}
}

﻿using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GTile : NamedObject
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

	public override void Load(XmlNode xnode)
	{
		//		name = xl.GetAttribute("name");
		//		type = xl.GetAttribute("type");
		//		picture = xl.GetAttribute("picture");
		name = MyXml.GetString(xnode, "name");
		type = MyXml.GetString(xnode, "type");
		picture = MyXml.GetString(xnode, "picture");
	}
/*
	public static void LoadBase()
	{
		Log.Write("loading the tile database... ");
		XmlNode xnode = MyXml.SecondChild("gTiles.xml");

		while (xnode != null)
		{
			GTile temp = new GTile();
			temp.Load((XmlElement)xnode);
			BigBase.Instance.gTileBase.Add(temp);
			xnode = xnode.NextSibling;
		}

		BigBase.Instance.gTileBase.loaded = true;
		Log.WriteLine("OK");
	}
*/
	public static void LoadTextures(Game game)
	{
		foreach (GTile gTile in BigBase.Instance.gTileBase.data)
		{
			gTile.texture = game.Content.Load<Texture2D>("t" + gTile.picture);
		}
	}
}

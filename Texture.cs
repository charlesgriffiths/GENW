using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Texture : NamedObject
{
	public List<Texture2D> data = new List<Texture2D>();
	public int numberOfVariations;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		numberOfVariations = MyXml.GetInt(xnode, "variations");
		if (numberOfVariations == 0) numberOfVariations = 1;
	}

	public Texture2D Single() { return data.Single();	}
	public Texture2D Random() { return data[World.Instance.random.Next(numberOfVariations)]; }

	public static void LoadTextures()
	{
		foreach (Texture t in BigBase.Instance.textures.data)
		{
			for (int i = 0; i < t.numberOfVariations; i++)
			{
				string suffix = t.numberOfVariations == 1 ? "" : " " + (i + 1).ToString();
                t.data.Add(MainScreen.Instance.game.Content.Load<Texture2D>("local/" + t.name + suffix));
			}
		}
	}
}
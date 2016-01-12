using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Texture
{
	public List<Texture2D> data = new List<Texture2D>();

	public Texture2D Single() { return data.Single();	}
	public Texture2D Random() { return data.Random(); }//{ return data[World.Instance.random.Next(data.Count)]; }
	public Texture2D this[int k] { get { return data[k]; } }

	public void LoadImages(string path, int variations)
	{
		for (int i = 0; i < variations; i++)
		{
			string suffix = variations == 1 ? "" : " " + (i + 1).ToString();
			data.Add(MainScreen.Instance.game.Content.Load<Texture2D>(path + suffix));
		}
	}
}

public class NamedTexture : NamedObject
{
	public Texture2D data;

	public override void Load(XmlNode xnode) { name = MyXml.GetString(xnode, "name"); }
	public static Texture2D Get(string name) { return BB.namedTextures.Get(name).data; }

	public static void LoadTextures()
	{
		foreach (NamedTexture t in BigBase.Instance.namedTextures.data)
			t.data = M.game.Content.Load<Texture2D>(t.name);
	}
}
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Texture : NamedObject
{
	public List<Texture2D> data = new List<Texture2D>();
	public int numberOfVariations;

	public static Texture2D Get(string name) { return BigBase.Instance.textures.Get(name).data[0]; }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		numberOfVariations = MyXml.GetInt(xnode, "variations", 1);
	}

	public Texture2D Single() { return data.Single();	}
	public Texture2D Random() { return data[World.Instance.random.Next(numberOfVariations)]; }
	public Texture2D this[int k] { get {
			Log.Assert(k >= 0 && k < numberOfVariations, "Texture index out of boundaries");
			return data[k]; } }

	public static void LoadTextures() {
		foreach (Texture t in BigBase.Instance.textures.data)
			for (int i = 0; i < t.numberOfVariations; i++)
				t.LoadImages("local/" + t.name); }

	public void LoadImages(string path)
	{
		for (int i = 0; i < numberOfVariations; i++)
		{
			string suffix = numberOfVariations == 1 ? "" : " " + (i + 1).ToString();
			data.Add(MainScreen.Instance.game.Content.Load<Texture2D>(path + suffix));
		}
	}
}
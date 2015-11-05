using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class Texture : NamedObject
{
	public Texture2D data;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
	}

	public static void LoadTextures()
	{
		foreach (Texture t in BigBase.Instance.textures.data)
			t.data = MainScreen.Instance.game.Content.Load<Texture2D>("local/" + t.name);
	}
}
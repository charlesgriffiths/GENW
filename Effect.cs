using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class Effect : NamedObject
{
	public Texture2D texture;
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
	}

	public static void LoadTextures()
	{
		foreach (Effect e in BigBase.Instance.effects.data)
			e.texture = MainScreen.Instance.game.Content.Load<Texture2D>("effects/" + e.name);
	}
}

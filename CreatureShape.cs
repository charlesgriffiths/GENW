using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class CreatureShape : NamedObject
{
	public Texture2D texture;
	public int maxHP, damage;
	public float movementSpeed, attackSpeed;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		maxHP = MyXml.GetInt(xnode, "maxHP");
		damage = MyXml.GetInt(xnode, "damage");
		movementSpeed = MyXml.GetFloat(xnode, "movementSpeed");
		attackSpeed = MyXml.GetFloat(xnode, "attackSpeed");
	}

	public static void LoadTextures()
	{
		foreach (CreatureShape s in BigBase.Instance.creatureShapes.data)
			s.texture = MainScreen.Instance.game.Content.Load<Texture2D>("l" + s.name);
	}
}
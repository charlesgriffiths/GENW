using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class LocalShape : NamedObject
{
	public Texture2D texture;
	public CreatureType creatureType;
	public int maxHP, damage, attack, defence, armor;
	public float movementTime, attackTime;
	public List<CAbility> abilities = new List<CAbility>();
	public bool isWalkable, isFlat;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		maxHP = MyXml.GetInt(xnode, "maxHP");
		damage = MyXml.GetInt(xnode, "damage");
		attack = MyXml.GetInt(xnode, "attack");
		defence = MyXml.GetInt(xnode, "defence");
		armor = MyXml.GetInt(xnode, "armor");
		movementTime = MyXml.GetFloat(xnode, "movementTime");
		attackTime = MyXml.GetFloat(xnode, "attackTime");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");

		string ctn = MyXml.GetString(xnode, "type");
		if (ctn != "") creatureType = BigBase.Instance.creatureTypes.Get(ctn);

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
			abilities.Add(BigBase.Instance.abilities.Get(MyXml.GetString(xnode, "name")));
	}

	public static void LoadTextures()
	{
		foreach (LocalShape s in BigBase.Instance.shapes.data)
			s.texture = M.game.Content.Load<Texture2D>("objects/" + s.name);
	}

	public static LocalShape Get(string name) { return BB.shapes.Get(name); }
}

public class CreatureType : NamedObject
{
	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
	}
}
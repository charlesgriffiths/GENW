using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class ShapeComponent : LocalComponent
{
	public LocalShape data;
	public int variation;

	public ShapeComponent(LocalShape shape, LocalObject o) : base(o)
	{
		data = shape;
		variation = R.Next(data.texture.numberOfVariations);
	}

	public Texture2D GetTexture { get { return data.texture[variation]; } }
}

public class LocalShape : NamedObject
{
	public Texture texture;
	public LocalType type;
	public CreatureType creatureType;
	public int maxHP, damage, attack, defence, armor;
	public float movementTime, attackTime;
	public List<CAbility> abilities = new List<CAbility>();
	public bool isWalkable, isFlat;

	public override void Load(XmlNode xnode)
	{
		type = LocalType.Get(xnode.Name);

		texture = new Texture();
		texture.numberOfVariations = MyXml.GetInt(xnode, "variations", 1);

		name = MyXml.GetString(xnode, "name");
		maxHP = MyXml.GetInt(xnode, "hp");
		damage = MyXml.GetInt(xnode, "damage");
		attack = MyXml.GetInt(xnode, "attack");
		defence = MyXml.GetInt(xnode, "defence");
		armor = MyXml.GetInt(xnode, "armor");
		movementTime = MyXml.GetFloat(xnode, "movementTime");
		attackTime = MyXml.GetFloat(xnode, "attackTime");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");

		string ctn = MyXml.GetString(xnode, "type");
		if (ctn != "") creatureType = CreatureType.Get(ctn);

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
			abilities.Add(BigBase.Instance.abilities.Get(MyXml.GetString(xnode, "name")));
	}

	public static void LoadTextures()
	{
		foreach (LocalShape s in BigBase.Instance.shapes.data) s.texture.LoadImages("objects/" + s.name);
			//s.texture = M.game.Content.Load<Texture2D>("objects/" + s.name);
	}

	public static LocalShape Get(string name) { return BB.shapes.Get(name); }
}

public class LocalType : NamedObject{
	public override void Load(XmlNode xnode) { name = MyXml.GetString(xnode, "name"); }
	public static LocalType Get(string name) { return BB.types.Get(name); }}

public class CreatureType : NamedObject {
	public override void Load(XmlNode xnode) { name = MyXml.GetString(xnode, "name"); }
	public static CreatureType Get(string name) { return BB.creatureTypes.Get(name); }}
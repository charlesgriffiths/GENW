using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Skill : NamedObject
{
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
	}

	public static Skill Get (string name) {	return BigBase.Instance.skills.Get(name); }
}

public class Skills : LocalComponent
{
	public Skills(LocalObject o) : base(o) { }

	public int this[Skill skill]
	{
		get
		{
			return (int)(t.race.bonus.skills[skill] + t.origin.bonus.skills[skill] + t.background.bonus.skills[skill] +
				(t.xp != null ? t.cclass.bonus.skills[skill] * t.xp.Level : 0));
		}
	}

	public int this[string skillName] { get { return this[Skill.Get(skillName)]; } }
}

public class Bonus
{
	public int hp, damage, attack, defence, armor;
	public float mtm, atm;
	public Dictionary<Skill, float> skills = new Dictionary<Skill, float>();

	public Bonus(XmlNode xnode)
	{
		hp = MyXml.GetInt(xnode, "hp");
		damage = MyXml.GetInt(xnode, "damage");
		attack = MyXml.GetInt(xnode, "attack");
		defence = MyXml.GetInt(xnode, "defence");
		armor = MyXml.GetInt(xnode, "armor");
		mtm = MyXml.GetFloat(xnode, "mtm", 1);
		atm = MyXml.GetFloat(xnode, "atm", 1);

		foreach (Skill skill in BigBase.Instance.skills.data)
			skills.Add(skill, MyXml.GetFloat(xnode, skill.name));
	}
}

public class Origin : NamedObject
{
	public Bonus bonus;
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
		bonus = new Bonus(xnode);
	}

	public static Origin Get(string name) { return BB.origins.Get(name); }
}

public class Background : NamedObject
{
	public Bonus bonus;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		bonus = new Bonus(xnode);
	}

	public static Background Get(string name) { return BB.backgrounds.Get(name); }
}

public class Race : NamedObject
{
	public ClassAbility ability;
	public Bonus bonus;
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
		bonus = new Bonus(xnode);
		ability = BigBase.Instance.abilities.Get(MyXml.GetString(xnode, "ability"));
	}

	public static Race Get(string name) { return BB.races.Get(name); }
}

public class CharacterClass : NamedObject
{
	public List<ClassAbility> abilities = new List<ClassAbility>();
	public Dictionary<Race, Texture2D> textures = new Dictionary<Race, Texture2D>();
	public Bonus bonus;
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
		bonus = new Bonus(xnode);

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
			abilities.Add(BigBase.Instance.abilities.Get(MyXml.GetString(xnode, "name")));
	}

	public static void LoadTextures()
	{
		foreach (CharacterClass c in BigBase.Instance.classes.data)
			foreach (Race r in BigBase.Instance.races.data)
			{
				Texture2D t = M.game.Content.Load<Texture2D>("characters/" + r.name + " " + c.name);
				c.textures.Add(r, t);
			}
	}

	public static CharacterClass Get(string name) { return BB.classes.Get(name); }
}
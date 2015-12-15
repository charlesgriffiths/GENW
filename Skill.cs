using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

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

		mtm = MyXml.GetFloat(xnode, "mtm");
		if (mtm == 0) mtm = 1;
		atm = MyXml.GetFloat(xnode, "atm");
		if (atm == 0) atm = 1;

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
}

public class Background : NamedObject
{
	public Bonus bonus;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		bonus = new Bonus(xnode);
	}
}

public class Race : NamedObject
{
	public Ability ability;
	public Bonus bonus;
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
		bonus = new Bonus(xnode);
		ability = BigBase.Instance.abilities.Get(MyXml.GetString(xnode, "ability"));
	}
}

public class CClass : NamedObject
{
	public Collection<Ability> abilities = new Collection<Ability>();
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
}
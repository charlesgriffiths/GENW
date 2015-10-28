using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

class Skill : NamedObject
{
	public string description;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
	}
}

class Bonus
{
	public int hp, damage, attack, defence;
	public Dictionary<Skill, float> skills = new Dictionary<Skill, float>();

	public Bonus(XmlNode xnode)
	{
		hp = MyXml.GetInt(xnode, "hp");
		damage = MyXml.GetInt(xnode, "damage");
		attack = MyXml.GetInt(xnode, "attack");
		defence = MyXml.GetInt(xnode, "defence");

		foreach (Skill skill in BigBase.Instance.skills.data)
			skills.Add(skill, MyXml.GetFloat(xnode, skill.name));
	}
}

class Gift : NamedObject
{
	public Bonus bonus;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		bonus = new Bonus(xnode);
	}
}

class Origin : NamedObject
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

class Background : NamedObject
{
	public Bonus bonus;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		bonus = new Bonus(xnode);
	}
}

class Race : NamedObject
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

class CClass : NamedObject
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
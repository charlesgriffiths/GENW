using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class CComponent : NamedObject
{
	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
	}

	public bool NameIs(params string[] names)
	{
		foreach (string name in names) if (BigBase.Instance.ccomponents.Get(name) == this) return true;
		return false;
	}
}

public class ItemShape : NamedObject
{
	public List<Tuple<CComponent, int>> cComponents = new List<Tuple<CComponent, int>>();

	public Texture2D texture;
	public Bonus bonus;
	public float value, weight;
	public string active, description;
	public bool isStackable, isEquippable, isArmor;
	public int hands, craftLevel;
	//public List<string> properties = new List<string>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		value = MyXml.GetFloat(xnode, "value");
		weight = MyXml.GetFloat(xnode, "weight");
		active = MyXml.GetString(xnode, "active");
		description = MyXml.GetString(xnode, "description");
		bonus = new Bonus(xnode);
		isArmor = MyXml.GetBool(xnode, "isArmor");
		hands = MyXml.GetInt(xnode, "hands");
		isStackable = MyXml.GetBool(xnode, "stackable");
		isEquippable = MyXml.GetBool(xnode, "equippable");
		craftLevel = MyXml.GetInt(xnode, "craftable");
		if (craftLevel == 0) craftLevel = 100;

		XmlNode cnode = xnode.FirstChild;
		for (cnode = cnode.FirstChild; cnode != null; cnode = cnode.NextSibling)
		{
			int amount = MyXml.GetInt(cnode, "amount");
			if (amount == 0) amount = 1;

            cComponents.Add(new Tuple<CComponent, int>(BigBase.Instance.ccomponents.Get(MyXml.GetString(cnode, "name")), amount));
		}

		/*XmlNode xnode2 = xnode.FirstChild;
		if (xnode2 != null)
		{
			for (xnode2 = xnode2.FirstChild; xnode2 != null; xnode2 = xnode2.NextSibling)
				properties.Add(MyXml.GetString(xnode2, "name"));
		}
		*/

		if (hands > 0 || isArmor) isEquippable = true;
	}

	public static void LoadTextures()
	{
		foreach (ItemShape i in BigBase.Instance.items.data)
			i.texture = M.game.Content.Load<Texture2D>("items/" + i.name);
	}

	public void DrawDescription(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(MyGame.Instance.battle ? 240 : 192, 190));
		//screen.Fill(new Color(0, 0, 0, 0.9f));
		screen.DrawString(M.fonts.verdanaBold, name, new ZPoint(0, 0), Color.White);
		SpriteFont font = M.fonts.small;
		int previousOffset = 0, hOffset = 0;

		Action<int> skip = step =>
		{
            if (screen.offset > previousOffset) screen.offset += step;
			previousOffset = screen.offset;
		};

		Action<string> draw = s => screen.DrawString(font, s, new ZPoint(hOffset, screen.offset), Color.White);
		Action<string, int> drawInt = (s, n) => { if (n != 0) draw(s + " " + Stuff.ShowSgn(n)); };

		skip(8);
		if (active != "") draw("ACTIVE: " + active);
		if (bonus.mtm != 1) draw("MOVEMENT TIME MULT.: " + bonus.mtm);
		if (bonus.atm != 1) draw("ATTACK TIME MULT.: " + bonus.atm);

		skip(8);
		drawInt("Damage", bonus.damage);
		drawInt("Armor", bonus.armor);
		drawInt("Attack", bonus.attack);
		drawInt("Defence", bonus.defence);
		drawInt("HP", bonus.hp);

		skip(8);
		screen.DrawString(font, description, new ZPoint(0, screen.offset), Color.White, screen.size.x);

		skip(8);
		draw("VALUE: " + value);
		draw("WEIGHT: " + weight);

		skip(8);
		draw("COMPONENTS:");
		hOffset += 16;
		foreach (var t in cComponents) draw(t.Item1.name + (t.Item2 > 1 ? " x" + t.Item2 : ""));
	}

	public List<CComponent> MultilessComponents { get { return cComponents.Select(t => t.Item1).ToList(); } }

	private int this[CComponent cc]
	{
		get
		{
			var query = from tuple in cComponents where tuple.Item1 == cc select tuple.Item2;
			return query.Count() > 0 ? query.Single() : 0;
		}
	}

	public bool IsComposable(Dictionary<CComponent, int> d)
	{
		foreach (var t in cComponents) if (!d.ContainsKey(t.Item1) || d[t.Item1] < t.Item2) return false;
		return true;
	}

	public bool IsComposable()
	{
		Player P = World.Instance.player;

		foreach (var t in cComponents)
		{
			int crafted = 0;
			foreach (var pair in P.craftableShapes)
				for (int n = 0; n < pair.Value; n++)
					crafted += pair.Key[t.Item1];

			int wanted = t.Item2;
			int available = P.crafting.CComponents.ContainsKey(t.Item1) ? P.crafting.CComponents[t.Item1] : 0;

			if (available < crafted + wanted) return false;
		}

		return true;
    }
}

public class Item
{
	public ItemShape data;
	public int numberOfStacks;

	public Item(ItemShape shape)
	{
		data = shape;
		numberOfStacks = 1;
	}

	public Item(Item item)
	{
		data = item.data;
		numberOfStacks = 1;
	}

	//public bool HasProperty(string propertyName) { return data.properties.Contains(propertyName); }
}
using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class CraftingComponent : NamedObject
{
	public float weight, value;
	public bool isRenewable;
	public int craftingComplexity;
	public ClassAbility requirement;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		weight = MyXml.GetFloat(xnode, "weight");
		value = MyXml.GetFloat(xnode, "value");
		isRenewable = MyXml.GetBool(xnode, "renewable");
		craftingComplexity = MyXml.GetInt(xnode, "crafting");

		string s = MyXml.GetString(xnode, "requires");
		if (s != "") requirement = ClassAbility.Get(s);
	}

	public bool NameIs(params string[] names)
	{
		foreach (string name in names) if (BB.components.Get(name) == this) return true;
		return false;
	}

	public static CraftingComponent Get(string name) { return BB.components.Get(name); }
}

public class ItemShape : NamedObject
{
	public List<Tuple<CraftingComponent, int>> components = new List<Tuple<CraftingComponent, int>>();

	public Texture2D texture;
	public Bonus bonus;
	public string description;
	public bool isStackable, isEquippable, isCraftable, isArmor;
	public int hands, range;
	public ItemAbility ability;

	public static ItemShape Get(string name) { return BigBase.Instance.items.Get(name); }

	public float Weight { get { return (from pair in components select pair.Item1.weight * pair.Item2).Sum(); } }
	public int CraftingComplexity { get { return (from pair in components select pair.Item1.craftingComplexity).Sum(); } }
	public float Value { get { return (from pair in components select pair.Item1.value * pair.Item2).Sum() * (1.0f + 0.2f * CraftingComplexity); } }

	public bool IsRenewable { get {
			foreach (CraftingComponent cc in MultilessComponents) if (!cc.isRenewable) return false;
			return true; } }
	
	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
		bonus = new Bonus(xnode);
		isArmor = MyXml.GetBool(xnode, "isArmor");
		hands = MyXml.GetInt(xnode, "hands");
		isStackable = MyXml.GetBool(xnode, "stackable");
		isEquippable = MyXml.GetBool(xnode, "equippable");
		isCraftable = MyXml.GetBool(xnode, "craftable");

		range = MyXml.GetInt(xnode, "range");
		if (range == 0) range = 1;

		string abilityName = MyXml.GetString(xnode, "ability");
		if (abilityName != "")
		{
			ability = new ItemAbility(BigBase.Instance.iAbilityTypes.Get(abilityName), this);
			isEquippable = true;
		}

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
		{
			if (xnode.Name == "component")
			{
				int amount = MyXml.GetInt(xnode, "amount");
				if (amount == 0) amount = 1;

				components.Add(new Tuple<CraftingComponent, int>(CraftingComponent.Get(MyXml.GetString(xnode, "name")), amount));
			}
		}

		if (hands > 0 || isArmor) isEquippable = true;
	}

	public static void LoadTextures()
	{
		foreach (ItemShape i in BigBase.Instance.items.data)
			i.texture = M.game.Content.Load<Texture2D>("items/" + i.name);
	}

	public void DrawDescription(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(MyGame.Instance.battle ? 240 : 192, 1));
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
		if (ability != null) draw("ABILITY: " + ability.name);
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

		if (MyGame.Instance.debug)
		{
			skip(8);
			draw("WEIGHT: " + Weight);
			draw("CMPLXTY: " + CraftingComplexity);
			draw("VALUE: " + Value);
		}

		skip(8);
		draw("COMPONENTS:");
		hOffset += 16;
		foreach (var t in components) draw(t.Item1.name + (t.Item2 > 1 ? " x" + t.Item2 : ""));
	}

	public List<CraftingComponent> MultilessComponents { get { return components.Select(t => t.Item1).ToList(); } }

	private int this[CraftingComponent cc]
	{
		get
		{
			var query = from tuple in components where tuple.Item1 == cc select tuple.Item2;
			return query.Count() > 0 ? query.Single() : 0;
		}
	}

	public bool IsComposable(Dictionary<CraftingComponent, int> d)
	{
		foreach (var t in components)
		{
			if (World.Instance.map[World.Instance.player.position].components.Contains(t.Item1)) continue;
			if (!d.ContainsKey(t.Item1) || d[t.Item1] < t.Item2) return false;
		}
		return true;
	}

	public bool IsComposable()
	{
		Player P = World.Instance.player;

		foreach (var t in components)
		{
			if (World.Instance.map[P.position].components.Contains(t.Item1)) continue;

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

	public Item(ItemShape shape, int n) {
		data = shape;
		numberOfStacks = n;	}

	public Item(ItemShape shape) : this(shape, 1) { }
	public Item(string shapeName, int n) : this(ItemShape.Get(shapeName), n) { }

	public float Weight { get { return data.Weight * numberOfStacks; } }
}
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ItemShape : NamedObject
{
	public Texture2D texture;
	public Bonus bonus;
	public float value, weight;
	public string active, description;
	public bool isStackable, isEquippable, isArmor;
	public int hands, nutritionalValue;
	public List<string> properties = new List<string>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		value = MyXml.GetFloat(xnode, "value");
		weight = MyXml.GetFloat(xnode, "weight");
		active = MyXml.GetString(xnode, "active");
		description = MyXml.GetString(xnode, "description");
		nutritionalValue = MyXml.GetInt(xnode, "nutritionalValue");
		bonus = new Bonus(xnode);
		isArmor = MyXml.GetBool(xnode, "isArmor");
		hands = MyXml.GetInt(xnode, "hands");
		isStackable = MyXml.GetBool(xnode, "stackable");
		isEquippable = MyXml.GetBool(xnode, "equippable");

		XmlNode xnode2 = xnode.FirstChild;
		if (xnode2 != null)
		{
			for (xnode2 = xnode2.FirstChild; xnode2 != null; xnode2 = xnode2.NextSibling)
				properties.Add(MyXml.GetString(xnode2, "name"));
		}

		if (hands > 0 || isArmor) isEquippable = true;
		if (nutritionalValue > 0) { isStackable = true; isEquippable = false; }
	}

	public static void LoadTextures()
	{
		foreach (ItemShape i in BigBase.Instance.items.data)
			i.texture = M.game.Content.Load<Texture2D>("items/" + i.name);
	}

	public void DrawDescription(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(MyGame.Instance.battle ? 240 : 192, 190));
		screen.Fill(new Color(0, 0, 0, 0.9f));

		screen.DrawString(M.fonts.verdanaBold, name, new ZPoint(0, 0), Color.White);
		SpriteFont font = M.fonts.small;

		screen.offset += 8;
		int previousOffset = screen.offset;
		if (active != "") screen.DrawString(font, "ACTIVE: " + active, new ZPoint(0, screen.offset), Color.White);
		if (nutritionalValue > 0) screen.DrawString(font, "NUTRITIONAL VALUE: " + nutritionalValue, new ZPoint(0, screen.offset), Color.White);
		if (bonus.mtm != 1) screen.DrawString(font, "MOVEMENT TIME MULT.: " + bonus.mtm, new ZPoint(0, screen.offset), Color.White);
		if (bonus.atm != 1) screen.DrawString(font, "ATTACK TIME MULT.: " + bonus.atm, new ZPoint(0, screen.offset), Color.White);

		if (screen.offset > previousOffset) screen.offset += 8;	previousOffset = screen.offset;
		if (bonus.damage != 0) screen.DrawString(font, "Damage " + Stuff.ShowSgn(bonus.damage), new ZPoint(0, screen.offset), Color.White);
		if (bonus.armor != 0) screen.DrawString(font, "Armor " + Stuff.ShowSgn(bonus.armor), new ZPoint(0, screen.offset), Color.White);
		if (bonus.attack != 0) screen.DrawString(font, "Attack " + Stuff.ShowSgn(bonus.attack), new ZPoint(0, screen.offset), Color.White);
		if (bonus.defence != 0) screen.DrawString(font, "Defence " + Stuff.ShowSgn(bonus.defence), new ZPoint(0, screen.offset), Color.White);
		if (bonus.hp != 0) screen.DrawString(font, "HP " + Stuff.ShowSgn(bonus.hp), new ZPoint(0, screen.offset), Color.White);

		if (screen.offset > previousOffset) screen.offset += 8;	previousOffset = screen.offset;
		screen.DrawString(font, description, new ZPoint(0, screen.offset), Color.White, screen.size.x);

		if (screen.offset > previousOffset) screen.offset += 8;	previousOffset = screen.offset;
		screen.DrawString(font, "VALUE: " + value, new ZPoint(0, screen.offset), Color.White);
		screen.DrawString(font, "WEIGHT: " + weight, new ZPoint(0, screen.offset), Color.White);
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

	public bool HasProperty(string propertyName) { return data.properties.Contains(propertyName); }
}
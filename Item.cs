using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

abstract class ItemShape : NamedObject
{
	public Texture2D texture;
	public float value, weight;
	public string active, passive;

	public bool isStackable, isEquippable;
	public int hands;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		value = MyXml.GetFloat(xnode, "value");
		weight = MyXml.GetFloat(xnode, "weight");
		active = MyXml.GetString(xnode, "active");
		passive = MyXml.GetString(xnode, "passive");
	}

	public static void LoadTextures()
	{
		foreach (ItemShape i in BigBase.Instance.items.data)
			i.texture = MainScreen.Instance.game.Content.Load<Texture2D>("items/" + i.name);
	}
}

class ItemBase
{
	public List<ItemShape> data = new List<ItemShape>();

	public void Load(string filename)
	{
		for (XmlNode xnode = MyXml.SecondChild("Data/" + filename); xnode != null; xnode = xnode.NextSibling)
		{
			ItemShape item;
			if (xnode.Name == "item") item = new ClassicItemShape();
			else if (xnode.Name == "food") item = new Food();
			else if (xnode.Name == "weapon") item = new Weapon();
			else if (xnode.Name == "armor") item = new Armor();
			else
			{
				Log.Error("wrong item type " + xnode.Name);
				item = null;
			}

			item.Load(xnode);
			data.Add(item);
		}
	}

	public ItemShape Get(string name)
	{ return (from i in data where i.name == name select i).Single(); }
}

class ClassicItemShape : ItemShape
{
	public override void Load(XmlNode xnode)
	{
		base.Load(xnode);
		isStackable = MyXml.GetBool(xnode, "stackable");
		isEquippable = MyXml.GetBool(xnode, "equippable");
		if (isEquippable) hands = MyXml.GetInt(xnode, "hands");
	}
}

class Food : ItemShape
{
	public int nutritionalValue;

	public Food()
	{
		isStackable = true;
		isEquippable = true;
		hands = 0;
	}

	public override void Load(XmlNode xnode)
	{
		base.Load(xnode);
		nutritionalValue = MyXml.GetInt(xnode, "nutritionalValue");
	}
}

class Weapon : ItemShape
{
	public int damage;
	public float speedMultiplier;

	public Weapon()
	{
		isStackable = false;
		isEquippable = true;
	}

	public override void Load(XmlNode xnode)
	{
		base.Load(xnode);
		hands = MyXml.GetInt(xnode, "hands");
		damage = MyXml.GetInt(xnode, "damage");
	}
}

class Armor : ItemShape
{
	public int armor;

	public Armor()
	{
		isStackable = false;
		isEquippable = true;
		hands = 0;
	}

	public override void Load(XmlNode xnode)
	{
		base.Load(xnode);
		armor = MyXml.GetInt(xnode, "armor");
	}
}

class Item
{
	public ItemShape data;
	public int numberOfStacks;

	public Item(ItemShape shape)
	{
		data = shape;
		numberOfStacks = 1;
	}
}

class Inventory
{
	private Dictionary<int, Item> data = new Dictionary<int, Item>();

	public int Size { get { return data.Count; } }

	public Inventory(int size)
	{
		Log.Assert(size > 0 && size <= 100, "wrong inventory size");
		for (int i = 0; i < size; i++) data.Add(i, null);
	}

	public void Add(string name)
	{
		ItemShape shape = BigBase.Instance.items.Get(name);

		if (shape.isStackable)
		{
			var query = from i in data where i.Value != null && i.Value.data.name == name select i.Key;
			if (query.Count() > 0)
			{
				data[query.First()].numberOfStacks++;
				return;
			}
		}

		var EmptyCells = from i in data where i.Value == null select i.Key;
		if (EmptyCells.Count() > 0) data[EmptyCells.First()] = new Item(shape);
    }

	//public bool Has(ItemShape i) { return (from j in inventory where j.data == i select j).Count() > 0; }
	//public bool Has(string itemName) { return (from j in inventory where j.data.name == itemName select j).Count() > 0; }

	public void Draw(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(6 * 32, 32));
		if (Size > 6) screen.size = new ZPoint(6 * 32, 4 * 32);

		screen.Fill(new Color(0, 0, 0, 0.9f));

		for (int i = 0; i < Size; i++)
			if (data[i] != null) screen.Draw(data[i].data.texture, new ZPoint(i * 32, 0));
	}
}
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ItemShape : NamedObject
{
	public Texture2D texture;
	public Bonus bonus;
	public float value, weight;
	public string active;

	public bool isStackable, isEquippable, isArmor;
	public int hands, nutritionalValue;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		value = MyXml.GetFloat(xnode, "value");
		weight = MyXml.GetFloat(xnode, "weight");
		active = MyXml.GetString(xnode, "active");
		nutritionalValue = MyXml.GetInt(xnode, "nutritionalValue");
		bonus = new Bonus(xnode);

		isArmor = MyXml.GetBool(xnode, "isArmor");
		hands = MyXml.GetInt(xnode, "hands");

		isStackable = MyXml.GetBool(xnode, "stackable");
		isEquippable = MyXml.GetBool(xnode, "equippable");

		if (hands > 0 || isArmor) isEquippable = true;
		if (nutritionalValue > 0) { isStackable = true; isEquippable = true; }
	}

	public static void LoadTextures()
	{
		foreach (ItemShape i in BigBase.Instance.items.data)
			i.texture = MainScreen.Instance.game.Content.Load<Texture2D>("items/" + i.name);
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
}

public class Inventory
{
	private Dictionary<int, Item> data = new Dictionary<int, Item>();
	private Character character;

	public int Size { get { return data.Count; } }
	public Item this[int k] { get { return data[k]; } }

	public Inventory(int size, Character c)
	{
		Log.Assert(size > 0 && size <= 100, "wrong inventory size");
		for (int i = 0; i < size; i++) data.Add(i, null);
		character = c;
	}

	public List<Item> Items { get { return (from pair in data where pair.Value != null select pair.Value).Cast<Item>().ToList(); } }

	private bool HasRoomFor(ItemShape itemShape)
	{
		int totalHands = (from i in Items select i.data.hands).Sum() + itemShape.hands;
		bool armorAlreadyEquipped = (from i in Items where i.data.isArmor select i).Count() > 0 && itemShape.isArmor;
		return totalHands <= 2 && !armorAlreadyEquipped;
	}

	public bool CanAdd(Item item, int cell)
	{
		if (data[cell] == null)
		{
			if (character == null) return true;
			else return item.data.isEquippable && HasRoomFor(item.data);
		}
		else if (data[cell].data == item.data && data[cell].data.isStackable) return true;
		else return false;
	}

	public void Add(Item item, int cell)
	{
		if (item != null && item.data.nutritionalValue > 0 && character != null) character.AddEndurance(item.data.nutritionalValue);
		else if (data[cell] == null) data[cell] = item;
		else if (data[cell].data == item.data) data[cell].numberOfStacks++;
	}

	public void Add(ItemShape shape)
	{
		if (shape.isStackable)
		{
			var query = from i in data where i.Value != null && i.Value.data.name == shape.name select i.Key;
			if (query.Count() > 0)
			{
				data[query.First()].numberOfStacks++;
				return;
			}
		}

		var EmptyCells = from i in data where i.Value == null select i.Key;
		if (EmptyCells.Count() > 0) data[EmptyCells.First()] = new Item(shape);
    }

	public void Add(string name) { Add(BigBase.Instance.items.Get(name)); }
	public void Add(string name, int number) { for (int i = 0; i < number; i++) Add(name); }

	public void Remove(int cell)
	{
		if (data[cell] == null) return;
		else if (data[cell].numberOfStacks > 1) data[cell].numberOfStacks--;
		else data[cell] = null;
	}

	public void RemoveStack(int cell) {	data[cell] = null; }

	private ZPoint CellPosition(int cell) { return new ZPoint((cell % 6) * 32, (cell / 6) * 32); }

	public void Draw(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(6 * 32, 32));
		if (Size > 6) screen.size = new ZPoint(6 * 32, 4 * 32);

		screen.Fill(new Color(0, 0, 0, 0.8f));

		for (int i = 0; i < Size; i++)
		{
			Color color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
			ZPoint p = CellPosition(i);
            screen.DrawRectangle(p, new ZPoint(32, 1), color);
			screen.DrawRectangle(p, new ZPoint(1, 32), color);
			MouseTriggerInventory.Set(this, i, screen.position + p, new ZPoint(32, 32));

			if (data[i] != null)
			{
				screen.Draw(data[i].data.texture, p);
				if (data[i].numberOfStacks > 1)
					screen.DrawStringWithShading(MainScreen.Instance.smallFont, data[i].numberOfStacks.ToString(), p + new ZPoint(24, 18), Color.White);
			}
		}

		MouseTriggerInventory mti = MouseTriggerInventory.GetUnderMouse();
		if (mti != null && mti.inventory == this) screen.Draw(MainScreen.Instance.zSelectionTexture, CellPosition(mti.cell));
	}
}
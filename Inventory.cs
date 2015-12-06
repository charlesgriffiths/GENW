using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Inventory
{
	private Dictionary<int, Item> data = new Dictionary<int, Item>();
	private Character character;
	string name;

	private static MainScreen M { get { return MainScreen.Instance; } }

	public int Size { get { return data.Count; } }
	public Item this[int k] { get { return data[k]; } }

	public Inventory(int size, Character c, string namei)
	{
		Log.Assert(size > 0 && size <= 100, "wrong inventory size");
		for (int i = 0; i < size; i++) data.Add(i, null);

		character = c;
		name = namei;
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
		if (item != null && item.data.nutritionalValue > 0 && character != null)
			for (int i = 0; i < item.numberOfStacks; i++)
				character.Eat(item.data);
		//character.AddEndurance(item.data.nutritionalValue * item.numberOfStacks);

		else if (data[cell] == null) data[cell] = item;
		else if (data[cell].data == item.data) data[cell].numberOfStacks += item.numberOfStacks;
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

	public void RemoveStack(int cell) { data[cell] = null; }

	private ZPoint CellPosition(int cell) { return new ZPoint((cell % 6) * 32, (cell / 6) * 32); }

	public void Draw(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(6 * 32, 32));
		if (Size > 6) screen.size = new ZPoint(6 * 32, 4 * 32);

		if (!MyGame.Instance.battle) screen.Fill(Stuff.MyColor("Very Dark Grey"));
		if (name != "") screen.DrawString(M.fonts.verySmall, name, -9, Color.White);

		if (MyGame.Instance.battle) MyGame.Instance.mouseTriggerInventories.Clear();

		for (int i = 0; i < Size; i++)
		{
			Color color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
			ZPoint p = CellPosition(i);
			MouseTriggerInventory.Set(this, i, screen.position + p, new ZPoint(32, 32));

			/*if (!MyGame.Instance.battle)
			{
				screen.DrawRectangle(p, new ZPoint(32, 1), color);
				screen.DrawRectangle(p, new ZPoint(1, 32), color);
			}
			*/

			if (data[i] != null)
			{
				screen.Draw(data[i].data.texture, p);
				if (data[i].numberOfStacks > 1)
					screen.DrawStringWithShading(MainScreen.Instance.fonts.small, data[i].numberOfStacks.ToString(), p + new ZPoint(24, 18), Color.White);
			}
		}

		MouseTriggerInventory mti = MouseTriggerInventory.GetUnderMouse();
		if (mti != null && mti.inventory == this)
		{
			screen.Draw(MainScreen.Instance.zSelectionTexture, CellPosition(mti.cell));
			if (mti.GetItem() != null)
			{
				ZPoint p;
				if (MyGame.Instance.battle) p = position + new ZPoint(24, 32 + 48 + 8);
				else p = new ZPoint(48, 16 + 4 * 32 + (from c in World.Instance.player.party where c is Character select c).Count() * 40);
				mti.GetItem().data.DrawDescription(p);
			}
		}
	}
}
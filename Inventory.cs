﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Inventory
{
	private Dictionary<int, Item> data = new Dictionary<int, Item>();
	private Character character;
	private string name;
	private int width, height;

	private static MainScreen M { get { return MainScreen.Instance; } }
	private static MyGame G { get { return MyGame.Instance; } }

	public int Size { get { return /*data.Count*/ width * height; } }
	public Item this[int k] { get { return data[k]; } }
	public int Width { get { return width; } }
	public int Height { get { return height; } }

	public Inventory(int widthi, int heighti, Character c, string namei)
	{
		width = widthi;
		height = heighti;

		Log.Assert(Size > 0 && Size <= 100, "wrong inventory size");
		for (int i = 0; i < Size; i++) data.Add(i, null);

		character = c;
		name = namei;
	}

	public List<Item> Items { get { return (from pair in data where pair.Value != null select pair.Value).Cast<Item>().ToList(); } }
	public bool IsEmpty { get { return Items.Count == 0; } }

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
		if (data[cell] == null) data[cell] = item;
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

	private ZPoint CellPosition(int cell) { return new ZPoint((cell % width) * 32, (cell / width) * 32); }

	public void Draw(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(width * 32, height * 32));

		if (G.battle) G.mouseTriggerInventories.Clear();
		for (int i = 0; i < Size; i++) MouseTriggerInventory.Set(this, i, screen.position + CellPosition(i), new ZPoint(32, 32));
		MouseTriggerInventory mti = MouseTriggerInventory.GetUnderMouse();

		if (((mti != null && mti.inventory == this) || !IsEmpty) && name != "") screen.DrawString(M.fonts.superSmall, name, Color.White);
		if (!G.battle) screen.Fill(Stuff.MyColor("Very Dark Grey"));

		for (int i = 0; i < Size; i++)
		{
			ZPoint p = CellPosition(i);
			if (data[i] != null)
			{
				screen.Draw(data[i].data.texture, p);
				if (data[i].numberOfStacks > 1)
					screen.DrawStringWithShading(M.fonts.small, data[i].numberOfStacks.ToString(), p + new ZPoint(24, 18), Color.White);
			}
		}
		
		if (mti != null && mti.inventory == this)
		{
			screen.Draw(M.zSelectionTexture, CellPosition(mti.cell));
			Item item = mti.GetItem();
            if (item != null) item.data.DrawDescription(G.battle ? position + new ZPoint(24, 88) : new ZPoint(248, 8));
		}
	}

	//public List<Tuple<CComponent, int>> CComponents
	public Dictionary<CComponent, int> CComponents
	{
		get
		{
			Dictionary<CComponent, int> result = new Dictionary<CComponent, int>();
			foreach (Item i in Items)
				for (int n = 0; n < i.numberOfStacks; n++)
					foreach(var t in i.data.cComponents)
					{
						if (result.ContainsKey(t.Item1)) result[t.Item1] += t.Item2;
						else result.Add(t.Item1, t.Item2);
					}
			return result;
        }
	}
}
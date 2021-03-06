﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Inventory
{
	private Dictionary<int, Item> data = new Dictionary<int, Item>();
	public GlobalObject globalOwner;
	private LocalObject localOwner;
	public string name;
	private int width, height;
	public bool isInParty;

	private static MainScreen M { get { return MainScreen.Instance; } }
	private static MyGame G { get { return MyGame.Instance; } }
	private static Battlefield B { get { return World.Instance.battlefield; } }

	public int Size { get { return width * height; } }
	public Item this[int k] { get { return data[k]; } }
	public int Width { get { return width; } }
	public int Height { get { return height; } }

	public Inventory(int _width, int _height, string _name, bool _isInParty, GlobalObject _globalOwner = null, LocalObject _localOwner = null)
	{
		width = _width;
		height = _height;

		Log.Assert(Size > 0 && Size <= 100, "wrong inventory size");
		for (int i = 0; i < Size; i++) data.Add(i, null);

		globalOwner = _globalOwner;
		localOwner = _localOwner;
		name = _name;
		isInParty = _isInParty;
	}

	public List<Item> Items { get { return (from pair in data where pair.Value != null select pair.Value).Cast<Item>().ToList(); } }
	public bool IsEmpty { get { return Items.Count == 0; } }
	public float Value { get { return (from i in Items select i.data.Value * i.numberOfStacks).Sum(); } }

	private bool HasRoomFor(ItemShape itemShape)
	{
		int totalHands = (from i in Items select i.data.hands).Sum() + itemShape.hands;
		bool armorAlreadyEquipped = (from i in Items where i.data.isArmor select i).Count() > 0 && itemShape.isArmor;
		return totalHands <= 2 && !armorAlreadyEquipped;
	}

	public bool CanAdd(Item item, int cell, bool ignoreStacking = false)
	{
		if (globalOwner != null && Weight + item.Weight > globalOwner.WeightLimit) return false;
		else if (data[cell] == null)
		{
			if (localOwner == null) return true;
			else return item.data.isEquippable && HasRoomFor(item.data);
		}
		else if (data[cell].data == item.data && data[cell].data.isStackable && !ignoreStacking) return true;
		else return false;
	}

	public bool CanAdd(Item item, bool ignoreStackibg = false)
	{
		for (int i = 0; i < Size; i++) if (CanAdd(item, i, ignoreStackibg)) return true;
		return false;
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

		data[(from pair in data where pair.Value == null select pair.Key).First()] = new Item(shape);
	}

	public void Add(Item item, bool ignoreStacking = false)
	{
		if (ignoreStacking)	data[(from pair in data where pair.Value == null select pair.Key).First()] = item;
		else Add(item.data, item.numberOfStacks);
	}

	public void Add(ItemShape shape, int number) { for (int i = 0; i < number; i++) Add(shape); }
	public void Add(string name) { Add(BigBase.Instance.items.Get(name)); }
	public void Add(string name, int number) { for (int i = 0; i < number; i++) Add(name); }

	public void Clear() { for (int i = 0; i < Size; i++) data[i] = null; }

	public void Remove(int cell)
	{
		if (data[cell] == null) return;
		else if (data[cell].numberOfStacks > 1) data[cell].numberOfStacks--;
		else data[cell] = null;
	}

	public void Remove(ItemShape shape) {
		Remove((from pair in data where pair.Value.data == shape select pair.Key).First());	}

	public void RemoveStack(int cell) { data[cell] = null; }

	private float Weight { get { return (from i in Items select i.Weight).Sum(); } }

	private ZPoint CellPosition(int cell) { return new ZPoint((cell % width) * 32, (cell / width) * 32); }

	public void Draw(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(width * 32, height * 32));

		if (G.battle) MouseTrigger.Clear<MouseTriggerInventory>();
		for (int i = 0; i < Size; i++) MouseTriggerInventory.Set(this, i, screen.position + CellPosition(i), new ZPoint(32, 32));
		var mti = MouseTrigger.GetUnderMouse<MouseTriggerInventory>();

		if (((mti != null && mti.inventory == this) || !IsEmpty) && name != "" && !G.battle) screen.DrawString(M.fonts.superSmall, name, Color.White);
		screen.Fill(name == "ground" ? Stuff.MyColor("Very Dark Blue") : Stuff.MyColor("Very Dark Grey"));
		if (globalOwner == World.Instance.player) screen.DrawRectangle(ZPoint.Zero, new ZPoint(screen.size.x, 
			(int)(Weight * screen.size.y / globalOwner.WeightLimit)), new Color(0.2f, 0.2f, 0.2f, 0.5f));

		for (int i = 0; i < Size; i++)
		{
			ZPoint p = CellPosition(i);
			if (data[i] != null)
			{
				screen.Draw(data[i].data.texture, p);
				if (data[i].numberOfStacks > 1)
					screen.DrawStringWithShading(M.fonts.small, data[i].numberOfStacks.ToString(), p + new ZPoint(24, 18), Color.White);
				if (G.battle && localOwner == B.current && name == "" && data[i].data.ability != null)
					screen.DrawStringWithShading(M.fonts.small, data[i].cooldown > 0 ? "(" + (int)data[i].cooldown + ")" 
						: Stuff.ItemHotkeys[i].ToString(), p, Color.White);
			}
		}
		
		if (mti != null && mti.inventory == this)
		{
			screen.Draw(NamedTexture.Get("other/zSelection"), CellPosition(mti.cell));
			Item item = mti.GetItem();
			if (item != null)
			{
				item.data.DrawDescription(G.battle ? name == "ground" ? position + new ZPoint(-168, 88) : position + 
					new ZPoint(24, 88) : new ZPoint(248, 48));

				if (G.battle && G.RightMouseButtonClicked && B.current.inventory != null)
				{
					Inventory inventory = B.current.inventory;
					if (this == inventory)
					{
						B.Add(new LocalObject(item), B.current.p.value);
						Remove(mti.cell);
					}
					else if (name == "ground" && inventory.CanAdd(item))
					{
						inventory.Add(item);
						B.RemoveItem(item);
					}
				}
			}
		}
	}

	public Dictionary<CraftingComponent, int> CComponents
	{
		get
		{
			Dictionary<CraftingComponent, int> result = new Dictionary<CraftingComponent, int>();
			foreach (Item i in Items)
				for (int n = 0; n < i.numberOfStacks; n++)
					foreach(var t in i.data.components)
					{
						if (result.ContainsKey(t.Item1)) result[t.Item1] += t.Item2;
						else result.Add(t.Item1, t.Item2);
					}
			return result;
        }
	}

	public bool Contains(ItemShape shape) { return Items.Where(i => i.data == shape).Count() > 0; }

	public int Sum(Func<Bonus, int> func) { return (from i in Items select func(i.data.bonus)).Sum(); }
	public float Prod(Func<Bonus, float> func)
	{
		float result = 1;
		foreach (float f in from i in Items select func(i.data.bonus)) result *= f;
		return result;
	}

	public bool HasAbility(string name)
	{
		Ability a = BigBase.Instance.iAbilityTypes.Get(name);
		foreach (Item item in Items) if (item.data.ability != null && item.data.ability.name == a.name) return true;
		return false;
	}

	public void CopyTo(Inventory inventory)
	{
		int counter = 0;
		for (int i = 0; i < Size && counter < inventory.Size; i++)
		{
			if (data[i] != null)
			{
				inventory.Add(data[i]);
				//RemoveStack(i);
				counter++;
			}
		}
	}

	public float TimeMultiplier
	{
		get
		{
			if (localOwner == null) return 1.0f;
			else if (localOwner.skills == null) return 1.0f;
			else return (float)(1 + 0.1 * Weight * Math.Exp(-0.25 * localOwner.skills["Strength"]));
		}
	}

	public void UpdateCooldowns(float time)
	{
		foreach (Item item in Items)
		{
			item.cooldown -= time;
			if (item.cooldown < 0) item.cooldown = 0;
		}
	}
}
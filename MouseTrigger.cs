using System.Linq;
using System.Collections.Generic;

public abstract class MouseTrigger
{
	public ZPoint position, size;

	protected static MyGame G { get { return MyGame.Instance; } }
}

public class MouseTriggerLCreature : MouseTrigger
{
	public LCreature creature;

	private MouseTriggerLCreature(LCreature c, ZPoint positioni, ZPoint sizei)
	{
		creature = c;
		position = positioni;
		size = sizei;
	}

	public static void Clear() { G.mouseTriggerLCreatures.Clear(); }

	public static void Set(LCreature creature, ZPoint position, ZPoint size)
	{
		var query = from t in G.mouseTriggerLCreatures where t.creature == creature select t;
		if (query.Count() == 0) G.mouseTriggerLCreatures.Add(new MouseTriggerLCreature(creature, position, size));
		else
		{
			MouseTriggerLCreature t = query.Single();
			t.position = position;
		}
	}

	public static MouseTriggerLCreature GetUnderMouse()
	{
		var query = from t in G.mouseTriggerLCreatures where G.Mouse.IsIn(t) select t;
		if (query.Count() > 0) return query.First();
		else return null;
	}
}

public class MouseTriggerInventory : MouseTrigger
{
	public Inventory inventory;
	public int cell;

	public MouseTriggerInventory(Inventory i, int celli, ZPoint positioni, ZPoint sizei)
	{
		inventory = i;
		cell = celli;
		position = positioni;
		size = sizei;
	}

	public static void Set(Inventory inventory, int cell, ZPoint position, ZPoint size)
	{
		var query = from t in G.mouseTriggerInventories where t.inventory == inventory && t.cell == cell select t;
		if (query.Count() == 0) G.mouseTriggerInventories.Add(new MouseTriggerInventory(inventory, cell, position, size));
		else
		{
			MouseTriggerInventory mti = query.Single();
			mti.position = position;
		}
	}

	public static MouseTriggerInventory GetUnderMouse()
	{
		var query = from t in G.mouseTriggerInventories where G.Mouse.IsIn(t) select t;
		if (query.Count() > 0) return query.First();
		else return null;
	}

	public Item GetItem() { return inventory[cell]; }
}

public class MouseTriggerKeyword : MouseTrigger
{
	public string name;
	public int parameter;

	private MouseTriggerKeyword(string namei, int parameteri, ZPoint positioni, ZPoint sizei)
	{
		name = namei;
		parameter = parameteri;
		position = positioni;
		size = sizei;
	}

	public static MouseTriggerKeyword GetUnderMouse(string name)
	{
		var query = from t in G.mouseTriggerKeywords where G.Mouse.IsIn(t) && t.name == name select t;
		if (query.Count() > 0) return query.First();
		else return null;
	}

	public static MouseTriggerKeyword Get(string name, int parameter)
	{
		var query = from t in G.mouseTriggerKeywords where t.name == name && t.parameter == parameter select t;
		if (query.Count() > 0) return query.Single();
		else return null;
	}

	public static List<MouseTriggerKeyword> GetAll(string name)
	{ return (from t in G.mouseTriggerKeywords where t.name == name select t).ToList(); }

	public static void Set(string name, int parameter, ZPoint position, ZPoint size)
	{
		var query = from t in G.mouseTriggerKeywords where t.name == name && t.parameter == parameter select t;
		if (query.Count() == 0)	G.mouseTriggerKeywords.Add(new MouseTriggerKeyword(name, parameter, position, size));
	}

	public static void Set(string name, ZPoint position, ZPoint size) { Set(name, 0, position, size); }

	public static MouseTriggerKeyword GetUnderMouse()
	{
		var query = from t in G.mouseTriggerKeywords where G.Mouse.IsIn(t) select t;
		if (query.Count() > 0) return query.First();
		else return null;
	}

	public static void Clear(string name)
	{
		var query = G.mouseTriggerKeywords.Where(t => t.name == name).ToList();
		foreach (MouseTriggerKeyword t in query) G.mouseTriggerKeywords.Remove(t);
	}
}

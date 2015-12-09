using System.Linq;
using System.Collections.Generic;

public abstract class MouseTrigger
{
	public ZPoint position, size;

	protected static MyGame G { get { return MyGame.Instance; } }

	public static T GetUnderMouse<T>() where T : MouseTrigger {
		var query = from mt in G.mouseTriggers where mt is T && G.Mouse.IsIn(mt) select mt as T;
		return query.Count() > 0 ? query.First() : null; }

	public static List<T> All<T>() where T : MouseTrigger {
		return (from mt in G.mouseTriggers where mt is T select mt as T).ToList(); }

	public static List<T> AllUnderMouse<T>() where T : MouseTrigger	{
		return (from mt in G.mouseTriggers where mt is T && G.Mouse.IsIn(mt) select mt as T).ToList(); }

	public static void Clear<T>() where T : MouseTrigger {
		var query = G.mouseTriggers.Where(mt => mt is T).ToList();
        foreach (var mt in query) G.mouseTriggers.Remove(mt); }
}

public class MouseTriggerObject<T> : MouseTrigger where T : class
{
	public T t;

	private MouseTriggerObject(T ti, ZPoint positioni, ZPoint sizei)
	{
		t = ti;
		position = positioni;
		size = sizei;
	}

	public static void Set(T ti, ZPoint position, ZPoint size)
	{
		var query = G.mouseTriggers.Where(mt => mt is MouseTriggerObject<T> && (mt as MouseTriggerObject<T>).t == ti);
		if (query.Count() == 0) G.mouseTriggers.Add(new MouseTriggerObject<T>(ti, position, size));
		else query.Single().position = position;
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
		var query = All<MouseTriggerInventory>().Where(mti => mti.inventory == inventory && mti.cell == cell);
		if (query.Count() == 0) G.mouseTriggers.Add(new MouseTriggerInventory(inventory, cell, position, size));
		else query.Single().position = position;
	}

	public Item GetItem() { return inventory[cell]; }
}

public class MouseTriggerKeyword : MouseTrigger
{
	public string name, parameter;

	private MouseTriggerKeyword(string namei, string parameteri, ZPoint positioni, ZPoint sizei)
	{
		name = namei;
		parameter = parameteri;
		position = positioni;
		size = sizei;
	}

	public static MouseTriggerKeyword GetUnderMouse(string name) {
		var query = AllUnderMouse<MouseTriggerKeyword>().Where(mtk => mtk.name == name);
		return query.Count() > 0 ? query.First() : null; }

	public static void Set(string name, string parameter, ZPoint position, ZPoint size) {
		var query = All<MouseTriggerKeyword>().Where(mtk => mtk.name == name && mtk.parameter == parameter);
		if (query.Count() == 0)	G.mouseTriggers.Add(new MouseTriggerKeyword(name, parameter, position, size)); }

	public static void Set(string name, ZPoint position, ZPoint size) { Set(name, "", position, size); }

	public static void Clear(string name) {
		foreach (var mt in All<MouseTriggerKeyword>().Where(mtk => mtk.name == name)) G.mouseTriggers.Remove(mt); }

	public static MouseTriggerKeyword Get(string name, string parameter) {
		var query = All<MouseTriggerKeyword>().Where(mtk => mtk.name == name && mtk.parameter == parameter);
		return query.Count() > 0 ? query.First() : null; }
}

using System.Linq;
using System.Collections.Generic;

public abstract class MouseTrigger
{
	public ZPoint position, size;

	protected static MyGame G { get { return MyGame.Instance; } }
}

public class MouseTriggerCreature : MouseTrigger
{
	public Creature creature;

	private MouseTriggerCreature(Creature c, ZPoint positioni, ZPoint sizei)
	{
		creature = c;
		position = positioni;
		size = sizei;
	}

	public static void Clear() { G.mouseTriggerCreatures.Clear(); }

	public static void Set(Creature creature, ZPoint position, ZPoint size)
	{
		var query = from t in G.mouseTriggerCreatures where t.creature == creature select t;
		if (query.Count() == 0) G.mouseTriggerCreatures.Add(new MouseTriggerCreature(creature, position, size));
		else
		{
			MouseTriggerCreature t = query.Single();
			t.position = position;
		}
	}

	public static MouseTriggerCreature GetUnderMouse()
	{
		var query = from t in G.mouseTriggerCreatures where G.Mouse.IsIn(t) select t;
		if (query.Count() > 0) return query.First();
		else return null;
	}
}

public class MouseTriggerKeyword : MouseTrigger
{
	private string name;
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
	{ return (from t in G.mouseTriggerKeywords where t.name == name select t).Cast<MouseTriggerKeyword>().ToList(); }

	public static void Set(string name, int parameter, ZPoint position, ZPoint size)
	{
		var query = from t in G.mouseTriggerKeywords where t.name == name && t.parameter == parameter select t;
		if (query.Count() == 0)	G.mouseTriggerKeywords.Add(new MouseTriggerKeyword(name, parameter, position, size));
	}

	public static MouseTriggerKeyword GetUnderMouse()
	{
		var query = from t in G.mouseTriggerKeywords where G.Mouse.IsIn(t) select t;
		if (query.Count() > 0) return query.First();
		else return null;
	}

}

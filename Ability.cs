using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Ability : NamedObject
{
	public TargetType targetType;
	public int range, cost;
	public float castTime, cooldownTime;

	public enum TargetType { Passive, None, Direction, Point, Creature };

	public static TargetType GetTargetType(string s)
	{
		if (s == "passive") return TargetType.Passive;
		else if (s == "none") return TargetType.None;
		else if (s == "direction") return TargetType.Direction;
		else if (s == "point") return TargetType.Point;
		else if (s == "creature") return TargetType.Creature;
		else
		{
			Log.Error("unknown ability target type " + s);
			return TargetType.Passive;
		}
	}

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		targetType = GetTargetType(MyXml.GetString(xnode, "target"));
		range = MyXml.GetInt(xnode, "range");
		cost = MyXml.GetInt(xnode, "cost");
		castTime = MyXml.GetFloat(xnode, "castTime");
		cooldownTime = MyXml.GetFloat(xnode, "cooldownTime");
	}
}

public class IAbility : Ability
{
	public ItemShape itemShape;

	public override void Load(XmlNode xnode) { Log.Error("should not be called"); }

	public IAbility(Ability a, ItemShape s)
	{
		itemShape = s;
		name = a.name;
		targetType = a.targetType;
		range = a.range;
		cost = a.cost;
		castTime = a.castTime;
		cooldownTime = a.cooldownTime;
	}
}

public class CAbility : Ability
{
	public Texture2D texture;
	public string description;
	//public int cost;//, range;
	//public float castTime, cooldownTime;
	public Color color;
	//public TargetType targetType;

	public static CAbility Get (string name) { return BigBase.Instance.abilities.Get(name); }

	//public enum TargetType { Passive, None, Direction, Point, Creature };

	public static string Name(string s)
	{
		if (BigBase.Instance.abilities.Get(s) != null) return s;
		else
		{
			Log.Error("Unknown ability name " + s);
			return "ERROR";
		}
	}

	public bool NameIs(string s) { return name == Name(s); }

	public override void Load(XmlNode xnode)
	{
		//name = MyXml.GetString(xnode, "name");
		//targetType = GetTargetType(MyXml.GetString(xnode, "target"));
		//range = MyXml.GetInt(xnode, "range");
		base.Load(xnode);
		color = Stuff.MyColor(MyXml.GetString(xnode, "color"));
		description = MyXml.GetString(xnode, "description");
	}

	public static void LoadTextures()
	{
		foreach (CAbility a in BigBase.Instance.abilities.data)
			a.texture = M.game.Content.Load<Texture2D>("abilities/" + a.name);
	}

	public void DrawDescription(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(240, 190));
		//screen.Fill(new Color(0, 0, 0.5f, 0.5f));

		screen.DrawString(M.fonts.verdanaBold, name, new ZPoint(0, 0), Color.White);
		SpriteFont font = M.fonts.small;

		if (range != 0 || targetType != TargetType.Passive) screen.offset += 8;
		if (range != 0) screen.DrawString(font, "RANGE: " + range, new ZPoint(0, screen.offset), Color.White);
		if (targetType != TargetType.Passive)
		{
			screen.DrawString(font, "COST: " + cost, new ZPoint(0, screen.offset), Color.White);
			screen.DrawString(font, "CAST TIME: " + castTime, new ZPoint(0, screen.offset), Color.White);
			screen.DrawString(font, "COOLDOWN: " + cooldownTime, new ZPoint(0, screen.offset), Color.White);
		}

		screen.offset += 8;
		screen.DrawString(font, description, new ZPoint(0, screen.offset), Color.White, screen.size.x);
	}
}
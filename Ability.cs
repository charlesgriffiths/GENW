using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Ability : NamedObject
{
	public Texture2D texture;
	public string description;
	public TargetType targetType;
	public int cost, range;
	public float castTime, cooldownTime;

	public enum TargetType { Passive, None, Direction, Point, Creature };

	private static TargetType GetTargetType(string s)
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
		name = MyXml.GetString(xnode, "name");
		targetType = GetTargetType(MyXml.GetString(xnode, "target"));
		cost = MyXml.GetInt(xnode, "cost");
		range = MyXml.GetInt(xnode, "range");
		castTime = MyXml.GetFloat(xnode, "castTime");
		cooldownTime = MyXml.GetFloat(xnode, "cooldownTime");
		description = MyXml.GetString(xnode, "description");
	}

	public static void LoadTextures()
	{
		foreach (Ability a in BigBase.Instance.abilities.data)
			a.texture = MainScreen.Instance.game.Content.Load<Texture2D>("abilities/" + a.name);
	}

	public void DrawDescription(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(240, 190));
		//screen.Fill(new Color(0, 0, 0.5f, 0.5f));

		screen.DrawString(MainScreen.Instance.verdanaBoldFont, name, new ZPoint(0, 0), Color.White);
		SpriteFont font = MainScreen.Instance.smallFont;

		screen.offset += 8;
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

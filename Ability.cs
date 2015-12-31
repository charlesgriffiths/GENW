using System;
using System.Xml;
using System.Collections.Generic;
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

	public virtual bool NameIs(string s)
	{
		Log.Error("Ability.NameIs should not be called");
		return false;
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

	public override bool NameIs(string s) { return name == BigBase.Instance.iAbilityTypes.Get(s).name; }
}

public class CAbility : Ability
{
	public Texture2D texture;
	public string description;
	public Color color;

	public static CAbility Get (string name) { return BigBase.Instance.abilities.Get(name); }
	
	private static string Name(string s)
	{
		if (BigBase.Instance.abilities.Get(s) != null) return s;
		else
		{
			Log.Error("Unknown ability name " + s);
			return "ERROR";
		}
	}

	public override bool NameIs(string s) { return name == Name(s); }

	public override void Load(XmlNode xnode)
	{
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

public partial class Abilities : LocalComponent
{
	public Abilities(LocalObject o) : base(o) { }

	public List<CAbility> list
	{
		get
		{
			List<CAbility> result = new List<CAbility>();
			if (t.shape != null) foreach (var a in t.shape.data.abilities) result.Add(a);
			if (t.race != null) result.Add(t.race.ability);
			if (t.cclass != null) foreach (var a in t.cclass.abilities) result.Add(a);
			return result;
		}
	}

	public bool Has(string name) { return list.Contains(BigBase.Instance.abilities.Get(name)); }

	public void Draw(Screen screen, ZPoint position)
	{
		Func<int, ZPoint> aPosition = k => screen.position + position + new ZPoint(48 * k, 0);
		ZPoint aSize = new ZPoint(48, 48);

		for (int n = 0; n < 6; n++) MouseTriggerKeyword.Set("ability", n.ToString(), aPosition(n), aSize);
		var mtk = MouseTriggerKeyword.GetUnderMouse("ability");

		int i = 0;
		foreach (CAbility a in list)
		{
			bool mouseOn = mtk != null && mtk.parameter == i.ToString();

			M.Draw(a.texture, aPosition(i), mouseOn ? a.color : Color.White);

			if (a.targetType == Ability.TargetType.Passive) M.DrawRectangle(aPosition(i), aSize, new Color(0, 0, 0, 0.7f));

			else if (t == B.current) M.DrawStringWithShading(M.fonts.small, Stuff.AbilityHotkeys[i].ToString(),
				aPosition(i)/* + new ZPoint(37, 33)*/, Color.White);

			if (mouseOn) a.DrawDescription(screen.position + position + new ZPoint(24, 56));

			i++;
		}
	}
}
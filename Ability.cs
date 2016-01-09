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

public class ItemAbility : Ability
{
	public ItemShape itemShape;

	public override void Load(XmlNode xnode) { Log.Error("should not be called"); }

	public ItemAbility(Ability a, ItemShape s)
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

public class ClassAbility : Ability
{
	public Texture2D texture;
	public string description;
	public Color color;

	public static ClassAbility Get (string name) { return BigBase.Instance.abilities.Get(name); }
	
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
		foreach (ClassAbility a in BigBase.Instance.abilities.data)
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
	public Dictionary<ClassAbility, float> cooldowns;

	public Abilities(LocalObject o) : base(o)
	{
		cooldowns = new Dictionary<ClassAbility, float>();
		foreach (ClassAbility a in list) cooldowns.Add(a, 0);
	}

	public List<ClassAbility> list
	{
		get
		{
			var result = new List<ClassAbility>();
			if (t.shape != null) foreach (var a in t.shape.data.abilities) result.Add(a);
			if (t.race != null) result.Add(t.race.ability);
			if (t.cclass != null) foreach (var a in t.cclass.abilities) result.Add(a);
			return result;
		}
	}

	public bool Has(ClassAbility ability)
	{
		if (ability == null) return true;
		else
		{
			bool isLearned = t.xp == null ? true : t.xp.learned.Contains(ability);
			return list.Contains(ability) && isLearned;
		}
	}

	public bool Has(string name) { return Has(ClassAbility.Get(name)); }

	public void Draw(Screen screen, ZPoint position)
	{
		Func<int, ZPoint> aPosition = k => screen.position + position + new ZPoint(48 * k, 0);
		ZPoint aSize = new ZPoint(48, 48);

		for (int n = 0; n < 6; n++) MouseTriggerKeyword.Set("ability", n.ToString(), aPosition(n), aSize);
		var mtk = MouseTriggerKeyword.GetUnderMouse("ability");

		int i = 0;
		foreach (ClassAbility a in list)
		{
			bool mouseOn = mtk != null && mtk.parameter == i.ToString();
			bool has = Has(a);
			bool levelup = t.xp != null && t.xp.AbilityPoints > 0;
			bool passive = a.targetType == Ability.TargetType.Passive;

			if (has || levelup)
			{
				M.Draw(a.texture, aPosition(i), mouseOn ? a.color : Color.White);
				if (passive || cooldowns[a] > 0) M.DrawRectangle(aPosition(i), aSize, new Color(0, 0, 0, 0.7f));
				if (t == B.current && !passive) M.DrawStringWithShading(M.fonts.small, 
					cooldowns[a] > 0 ? "(" + (int)cooldowns[a] + ")" : Stuff.AbilityHotkeys[i].ToString(),	aPosition(i), Color.White);
				if (mouseOn) a.DrawDescription(screen.position + position + new ZPoint(24, 56));
			}

			if (!has && levelup) M.Draw(B.plusIcon, aPosition(i));

			i++;
		}
	}

	public void UpdateCooldowns(float time)
	{
		foreach (var a in list)
		{
			cooldowns[a] -= time;
			if (cooldowns[a] < 0) cooldowns[a] = 0;
		}
	}
}
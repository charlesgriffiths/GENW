﻿using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public partial class Battlefield
{
	private char[,] data;
	public Palette palette;

	private List<LocalObject> objects = new List<LocalObject>();

	public LocalObject current, spotlight;
	public Ability ability = null;
	private GlobalObject global;

	private Texture2D arrowTexture, targetTexture;
	public Texture2D damageIcon, armorIcon, plusIcon;

	public AnimationQueue scaleAnimations = new AnimationQueue();
	public AnimationQueue combatAnimations = new AnimationQueue();
	public CombatLog log = new CombatLog();

	private List<DelayedDrawing> delayedDrawings = new List<DelayedDrawing>();
	private float expectedInitiative = 0.0f;
	
	private static MainScreen M { get { return MainScreen.Instance; } }
	private static Player P { get { return World.Instance.player; } }
	private static Random R { get { return World.Instance.random; } }

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public List<LocalObject> ActiveObjects { get { return objects.Where(u => u.initiative != null).ToList(); } }
	public List<LocalObject> Items { get { return objects.Where(o => o.item != null).ToList(); } }

	public ZPoint Mouse { get { return ZCoordinates(MyGame.Instance.mouseState.Position.ToVector2()); } }

	public LocalTile this[ZPoint p]
	{
		get
		{
			if (InRange(p)) return palette[data[p.x, p.y]];
			else { Log.Error("battlefield index out of range");	return null; }
		}
	}

	public void SetTile(ZPoint p, char value) { if (InRange(p)) data[p.x, p.y] = value; }
	public void SetTile(char value) { SetTile(Mouse, value); }

	private bool InRange(ZPoint p) { return p.InBoundaries(new ZPoint(0, 0), Size - new ZPoint(1, 1)); }

	public List<LocalObject> GetAll(ZPoint p) { return objects.Where(o => o.p.value.TheSameAs(p)).ToList(); }
	public LocalObject Get(ZPoint p)	{
		var query = from o in objects where o.p.value.TheSameAs(p) orderby o.Importance select o;
		return query.Count() > 0 ? query.First() : null; }

	public bool IsWalkable(ZPoint p)
	{
		if (!InRange(p)) return false;
		if (!this[p].IsWalkable) return false;

		LocalObject o = Get(p);
		if (o != null && !o.p.IsWalkable) return false;
	
		return true;
    }

	public bool IsFlat(ZPoint p)
	{
		if (!InRange(p)) return false;
		if (!this[p].IsFlat) return false;

		LocalObject o = Get(p);
		if (o != null && !o.p.IsFlat) return false;

		return true;
	}

	private ZPoint RandomFreeTile()
	{
		for (int i = 0; i < 100; i++)
		{
			int zx = World.Instance.random.Next(Size.x);
			int zy = World.Instance.random.Next(Size.y);

			ZPoint z = new ZPoint(zx, zy);
			if (IsWalkable(z)) return z;
		}

		return new ZPoint(0, 0);
	}

	public void Remove(LocalObject o) { objects.Remove(o); }
	public void Add(LocalObject o, ZPoint position = null, bool isInParty = false, bool isAIControlled = false)
	{
		o.p = new LocalPosition(o);
		if (position != null) o.p.Set(position, 0.01f, false);

		o.drawing = new LocalDrawing(o);

		if (o.GetCreatureType != null)
		{
			o.effects = new Effects(o);
			o.team = new Team(isInParty, o);
			o.initiative = new Initiative(isAIControlled, o);
			o.initiative.Set(ActiveObjects.Count > 0 ? (from q in ActiveObjects select q.initiative.value).Average() : 0, 0.01f, false);
		}

		objects.Add(o);
	}

	public void StartBattle(GlobalObject _global)
	{
		global = _global;
		//GlobalTile gTile = World.Instance.map[g.position];
		//string battlefieldName;
		//if (gTile.type.name == "mountainPass") battlefieldName = "Custom Mountain";
		//else battlefieldName = "Custom Mountain";
		//Load(battlefieldName);
		Generate();

		objects.Clear();
		foreach (LocalObject c in P.party) Add(c, null, true, false);
		foreach (LocalObject c in global.party) Add(c, null, false, true);

		for (int i = 0; i <= Size.x * Size.y / 10; i++) Add(new LocalObject(LocalShape.Get("Tree")));
		for (int i = 0; i * 6 < global.inventory.Items.Count; i++) Add(new LocalObject(LocalShape.Get("Chest"), "", global.inventory));
		
		foreach (LocalObject l in objects)
		{
			l.p.Set(RandomFreeTile(), 0.01f, false);
			if (l.initiative != null) l.initiative.Set(l.uniqueName == P.uniqueName ? 0.1f : -R.Next(100) / 100.0f, 0.01f, false);
		}

		LocalObject next = NextObject;
		if (next.initiative.IsAIControlled) next.initiative.Run();

		current = next;
		spotlight = current;
		MyGame.Instance.battle = true;
	}

	private void Generate()
	{
		palette = BigBase.Instance.palettes.data.Random();

		int width = 20;
		int height = 10;
		data = new char[width, height];

		for (int j = 0; j < height; j++) for (int i = 0; i < width; i++)
			{
				data[i, j] = palette.data.RandomKey();
			}
	}

	private void Load(string name)
	{
		XmlNode xnode = MyXml.FirstChild("Data/Battlefields/" + name + ".xml");
		palette = BigBase.Instance.palettes.Get(MyXml.GetString(xnode, "palette"));

		string text = xnode.InnerText;
		char[] delimiters = new char[] { '\r', '\n', ' ' };
		string[] dataLines = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
		int width = dataLines[0].Length;
		int height = dataLines.Length;
		data = new char[width, height];

		for (int j = 0; j < height; j++) for (int i = 0; i < width; i++) data[i, j] = dataLines[j][i];
	}

	public void SetSpotlight()	{
        var query = from c in objects where c.p.TheSameAs(Mouse) orderby c.Importance select c;
		if (query.Count() > 0) spotlight = query.First(); }

	public LocalObject NextObject {	get	{
		var query = from o in objects where o.initiative != null orderby -o.initiative.value select o;
		return query.Count() != 0 ? query.First() : null; }	}

	private enum Resolution { Not, Victory, Retreat };
	private Resolution GetResolution()
	{
		Func<LocalObject, bool> onBorder = lc => lc.p.x == 0 || lc.p.y == 0 || lc.p.x == Size.x - 1 || lc.p.y == Size.y - 1;
		Func<LocalObject, bool> noEnemiesNearby = lc => ActiveObjects.Where(c => lc.team.IsEnemyTo(c) && lc.p.Distance(c) <= 2).Count() == 0;

		if (ActiveObjects.Where(c => !c.team.isInParty).Count() == 0) return Resolution.Victory;
		else if (ActiveObjects.Where(c => c.team.isInParty && (!onBorder(c) || !noEnemiesNearby(c))).Count() == 0) return Resolution.Retreat;
		else return Resolution.Not;
	}

	public void EndBattle()
	{
		Resolution resolution = GetResolution();
		Log.Assert(resolution == Resolution.Victory || resolution == Resolution.Retreat, "can't end battle");

		Action<GlobalObject, bool> reshape = (g, party) =>
		{
			g.party.Clear();

			foreach(LocalObject o in ActiveObjects.Where(o => o.team != null && o.team.isInParty == party).OrderBy(o => o.Importance).ToList())
			{
				o.p = null;
				o.drawing = null;
				o.effects = null;
				o.team = null;
				o.initiative = null;

				g.party.Add(o);
			}
		};

		reshape(P, true);
		reshape(global, false);
		
		MyGame.Instance.battle = false;

		if (resolution == Resolution.Victory)
		{
			foreach (var o in Items) P.ground.Add(o.item);
			global.Kill();
		}
	}

	public List<ZPoint> EveryPoint
	{
		get
		{
			List<ZPoint> result = new List<ZPoint>();
			for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++) result.Add(new ZPoint(i, j));
			return result;
		}
	}

	public List<ZPoint> Ray(ZPoint position, ZPoint.Direction d, int range, bool penetration)
	{
		List<ZPoint> result = new List<ZPoint>();
		ZPoint p = position + d;
		result.Add(position);

		while ((IsFlat(p) || (penetration && Get(p) != null)) && MyMath.ManhattanDistance(p, position) <= range)
		{
			result.Add(p);
			p += d;
		}

		if (!p.TheSameAs(position)) result.Add(p);
		return result;
	}

	public bool IsReachable(ZPoint p, ZPoint q, int range)
	{
		if (p.x != q.x && p.y != q.y) return false;
		if (MyMath.ManhattanDistance(p, q) > range) return false;
		ZPoint.Direction d = (p - q).GetDirection();
		for (ZPoint z = q + d; !z.TheSameAs(p); z += d) if (!IsFlat(z)) return false;
		return true;
	}

	public List<ZPoint> Range { get { return EveryPoint.Where(p => current.p.Distance(p) <= ability.range).ToList(); } }

	public List<ZPoint> AbilityZone
	{
		get
		{
			System.Collections.IEnumerable query;

			if (ability.name == "Destroy Wall") query = Range.Where(p => this[p].type.name == "wall");

			else if (ability.targetType == Ability.TargetType.Point)
				query = from p in Range where IsWalkable(p) select p;
			else if (ability.targetType == Ability.TargetType.Direction)
				query = from p in EveryPoint where current.p.Distance(p) == 1 select p;
			else query = from c in ActiveObjects where current.p.Distance(c) <= ability.range select c.p.value;

			List<ZPoint> result = query.Cast<ZPoint>().ToList();

			if (ability.name == "Overgrowth")
				result.AddRange(from p in Range let o = Get(p) where o != null && o.TypeName == "Tree" select p);
			
			return result;
		}
	}

	public void RemoveItem(Item item) { objects.Remove(Items.Where(i => i.item == item).Single()); }
}
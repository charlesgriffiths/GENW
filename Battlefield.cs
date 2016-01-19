using System;
using System.Linq;
using System.Collections.Generic;

public partial class Battlefield
{
	private LocalCell[,] data;
	public Palette palette;

	private List<LocalObject> objects = new List<LocalObject>();
	public List<ZPoint> points = new List<ZPoint>();

	public LocalObject current, spotlight;
	public Ability ability = null;
	private GlobalObject global;

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

	public GlobalTile Terrain { get { return World.Instance.map[P.position]; } }
	private int NumberOfCreatures { get { return P.party.Count + global.party.Count; } }

	public LocalTile this[ZPoint p]
	{
		get
		{
			if (InRange(p)) return palette[data[p.x, p.y].tile];
			else { Log.Error("battlefield index out of range");	return null; }
		}
	}

	public void SetTile(ZPoint p, char value)
	{
		if (InRange(p))
		{
			data[p.x, p.y].tile = value;
			data[p.x, p.y].variation = R.Next(palette[value].variations);
		}
	}
	
	private bool InRange(ZPoint p) { return p.InBoundaries(new ZPoint(0, 0), Size - new ZPoint(1, 1)); }

	public List<LocalObject> GetAll(ZPoint p) { return objects.Where(o => o.p.value.TheSameAs(p)).ToList(); }
	public LocalObject Get(ZPoint p)	{
		var query = from o in objects where o.p.value != null && o.p.value.TheSameAs(p) orderby o.Importance select o;
		return query.Count() > 0 ? query.First() : null; }

	public bool IsWalkable(ZPoint p, LocalObject t = null)
	{
		if (!InRange(p)) return false;

		LocalObject o = Get(p);
		if (o != null) return o.p.IsWalkable;
		else return (t != null && t.HasAbility("Flying")) ? this[p].IsFlat : this[p].IsWalkable;
    }

	public bool IsFlat(ZPoint p)
	{
		if (!InRange(p)) return false;
		if (!this[p].IsFlat) return false;

		LocalObject o = Get(p);
		if (o != null && !o.p.IsFlat) return false;

		return true;
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

		MyGame.Instance.battle = false;

		reshape(P, true);
		reshape(global, false);
		
		if (resolution == Resolution.Victory)
		{
			foreach (var o in Items) P.ground.Add(o.item);
			global.Kill();
		}
		else if (resolution == Resolution.Retreat)
		{
			var chests = objects.Where(o => o.shape != null && o.shape.data == LocalShape.Get("Chest"));
			foreach (LocalObject chest in chests) chest.inventory.CopyTo(global.inventory);
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

	public List<ZPoint> Range(ZPoint p, int value) { return points.Where(q => MyMath.ManhattanDistance(p, q) <= value).ToList(); }
	public List<ZPoint> Range() { return Range(current.p.value, ability.range); }
	//public List<ZPoint> Range { get { return points.Where(p => current.p.Distance(p) <= ability.range).ToList(); } }

	public List<ZPoint> AbilityZone
	{
		get
		{
			System.Collections.IEnumerable query;

			if (ability.name == "Destroy Wall") query = Range().Where(p => this[p].type.name == "wall");

			else if (ability.targetType == Ability.TargetType.Point)
				query = from p in Range() where IsWalkable(p) select p;
			else if (ability.targetType == Ability.TargetType.Direction)
				query = from p in points where current.p.Distance(p) == 1 select p;
			else query = from c in ActiveObjects where current.p.Distance(c) <= ability.range select c.p.value;

			List<ZPoint> result = query.Cast<ZPoint>().ToList();

			if (ability.name == "Overgrowth")
				result.AddRange(from p in Range() let o = Get(p) where o != null && o.TypeName == "Tree" select p);
			
			return result;
		}
	}

	public void RemoveItem(Item item) { objects.Remove(Items.Where(i => i.item == item).Single()); }
}

struct LocalCell
{
	public char tile;
	public int variation;
}
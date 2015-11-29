using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

partial class Battlefield
{
	private char[,] data;
	public Palette palette;

	private Collection<LObject> objects = new Collection<LObject>();

	public LObject currentObject, spotlightObject;
	public Ability ability = null;
	private GObject gObject;

	private Texture2D arrowTexture, targetTexture, damageIcon, armorIcon;

	public AnimationQueue scaleAnimations = new AnimationQueue();
	public AnimationQueue combatAnimations = new AnimationQueue();
	public CombatLog log = new CombatLog();

	private List<DelayedDrawing> delayedDrawings = new List<DelayedDrawing>();
	private float expectedInitiative = 0.0f;
	
	private MainScreen M { get { return MainScreen.Instance; } }
	private Player P { get { return World.Instance.player; } }

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public List<LCreature> Creatures { get { return (from c in objects where c is LCreature select c as LCreature).Cast<LCreature>().ToList(); } }
	public List<LCreature> AliveCreatures { get { return Creatures.Where(c => c.IsAlive).Cast<LCreature>().ToList(); } }

	public ZPoint Mouse { get { return ZCoordinates(MyGame.Instance.mouseState.Position.ToVector2()); } }

	public LTile this[ZPoint p]
	{
		get
		{
			if (InRange(p)) return palette[data[p.x, p.y]];
			else
			{
				Log.Error("battlefield index out of range");
				return null;
			}
		}
	}

	public void SetTile(char value) { ZPoint p = Mouse;	if (InRange(p)) data[p.x, p.y] = value;	}

	private bool InRange(ZPoint p) { return p.InBoundaries(new ZPoint(0, 0), Size - new ZPoint(1, 1)); }

	
	public LObject GetLObject(ZPoint p)
	{
		foreach (LObject l in objects) if (l.position.TheSameAs(p)) return l;
		return null;
	}
	
	public LCreature GetLCreature(ZPoint p)
	{
		var query = from c in AliveCreatures where c.position.TheSameAs(p) select c;
		if (query.Count() > 0) return query.First() as LCreature;
		else return null;
	}

	public LCreature CurrentLCreature { get { return currentObject as LCreature; } }

	public bool IsWalkable(ZPoint p)
	{
		if (!InRange(p)) return false;
		if (!this[p].IsWalkable) return false;

		LObject lObject = GetLObject(p);
		if (lObject != null && !lObject.IsWalkable) return false;
	
		return true;
    }

	public bool IsFlat(ZPoint p)
	{
		if (!InRange(p)) return false;
		if (!this[p].IsFlat) return false;

		LObject lObject = GetLObject(p);
		if (lObject != null && !lObject.IsFlat) return false;

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

	public void Remove(LObject o) { objects.Remove(o); }
	public void Add(LObject o, ZPoint position)
	{
		o.SetPosition(position, 0.01f, false);
		o.SetInitiative((from q in AliveCreatures select q.initiative).Average(), 0.01f, false);
		objects.Add(o);
	}

	public void StartBattle(GObject g)
	{
		gObject = g;
		GTile gTile = World.Instance.map[g.position];
		string battlefieldName;
		if (gTile.type.name == "mountainPass") battlefieldName = "Custom Mountain";
		else battlefieldName = "Custom Mountain";
		Load(battlefieldName);

		objects.Clear();
		foreach (Creature c in P.party) objects.Add(new LCreature(c, true, false));
		foreach (Creature c in g.party) objects.Add(new LCreature(c, false, true));

		for (int i = 0; i <= Size.x * Size.y / 10; i++)	objects.Add(new PureLObject("Tree"));
		
		foreach (LObject l in objects)
		{
			l.SetPosition(RandomFreeTile(), 0.01f, false);
			if (l is LCreature && (l as LCreature).data.UniqueName == P.uniqueName) l.SetInitiative(0.1f, 0.01f, false);
			else l.SetInitiative(-World.Instance.random.Next(100) / 100.0f, 0.01f, false);
		}

		if(NextLObject is LCreature) { if ((NextLObject as LCreature).IsAIControlled) NextLObject.Run(); }
		else NextLObject.Run();

		currentObject = NextLObject;
		spotlightObject = currentObject;
		MyGame.Instance.battle = true;
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

		//screenPosition = new ZPoint(16, 16);
	}

	public void SetSpotlight()
	{
		ZPoint p = Mouse;
        var query = from c in AliveCreatures where c.position.TheSameAs(p) orderby c.Importance select c;
		if (query.Count() > 0) spotlightObject = query.First();
	}

	public LObject NextLObject
	{
		get
		{
			var query = from c in AliveCreatures orderby -c.initiative select c;
			if (query.Count() != 0) return query.First();
			else return null;
		}
	}

	public void CheckForEvents()
	{
		var aliveMonsters = from c in AliveCreatures where !c.isInParty select c;
		if (aliveMonsters.Count() == 0)
		{
			List<Creature> deadParty = P.party.Where(c => !c.IsAlive).Cast<Creature>().ToList();
			foreach (Creature c in deadParty) P.party.Remove(c);

			var newParty = from lc in AliveCreatures where !P.party.Contains(lc.data) && lc.isInParty select lc.data;
			foreach (Creature c in newParty) P.party.Add(c);

			P.party = P.party.OrderBy(c => c.Importance).ToList();

			gObject.Kill();
			MyGame.Instance.battle = false;
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

	public List<ZPoint> Range { get { return EveryPoint.Where(p => CurrentLCreature.Distance(p) <= ability.range).ToList(); } }

	public List<ZPoint> AbilityZone
	{
		get
		{
			System.Collections.IEnumerable query;

			if (ability.targetType == Ability.TargetType.Point)
				query = from p in Range where IsWalkable(p) select p;
			else if (ability.targetType == Ability.TargetType.Direction)
				query = from p in EveryPoint where CurrentLCreature.Distance(p) == 1 select p;
			else query = from c in AliveCreatures where CurrentLCreature.Distance(c) <= ability.range select c.position;

			List<ZPoint> result = query.Cast<ZPoint>().ToList();

			if (ability.NameIs("Nature's Call")) result.AddRange(from p in Range let o = GetLObject(p) where o != null && o.Name == "Tree" select p);
			
			return result;
		}
	}
}
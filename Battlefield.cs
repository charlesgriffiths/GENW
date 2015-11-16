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

	public Collection<LObject> objects = new Collection<LObject>();

	public LObject currentObject, spotlightObject;
	public Ability ability = null;
	private GObject gObject;

	private Texture2D arrowTexture, targetTexture, damageIcon, armorIcon;

	public AnimationQueue scaleAnimations = new AnimationQueue();
	public AnimationQueue combatAnimations = new AnimationQueue();

	private List<DelayedDrawing> delayedDrawings = new List<DelayedDrawing>();
	private float expectedInitiative = 0.0f;
	private ZPoint screenPosition;

	private MainScreen M { get { return MainScreen.Instance; } }
	private Player P { get { return World.Instance.player; } }

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public List<LCreature> Creatures { get { return (from c in objects where c is LCreature select c as LCreature).Cast<LCreature>().ToList(); } }
	public List<LCreature> AliveCreatures { get { return Creatures.Where(c => c.isAlive).Cast<LCreature>().ToList(); } }

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
		if (!this[p].IsWalkable || GetLCreature(p) != null) return false;
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

	private void AddCreature(Creature c, bool isInParty, bool isAIControlled)
	{
		if (c.uniqueName == P.Name)
		{
			LPlayer item = new LPlayer(c as Character);
			objects.Add(item);
		}
		else if (c is Character)
		{
			LCharacter item = new LCharacter(c as Character, isInParty, isAIControlled);
			objects.Add(item);
		}
		else if (c is Creep)
		{
			LCreep item = new LCreep(c as Creep, isInParty, isAIControlled);
			objects.Add(item);
		}
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
		foreach (Creature c in World.Instance.player.party) AddCreature(c, true, false);
		foreach (Creature c in g.party) AddCreature(c, false, true);

		LObject item = new LObject("Tree");
		objects.Add(item);

		foreach (LObject l in objects)
		{
			l.SetPosition(RandomFreeTile(), 60.0f, false);
			if (l is LPlayer) l.SetInitiative(0.1f, 60.0f, false);
			else l.SetInitiative(-World.Instance.random.Next(100) / 100.0f, 60.0f, false);
		}

		if(NextLObject is LCreature) { if ((NextLObject as LCreature).isAIControlled) NextLObject.Run(); }
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

		screenPosition = new ZPoint(16, 16);
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
			//P.party.Clear();
			//var aliveParty = from c in AliveCreatures where c.isInParty orderby c.Importance select c;
			//foreach (LCreature c in aliveParty)	P.party.Add(c.data);

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

	public List<ZPoint> AbilityZone
	{
		get
		{
			System.Collections.IEnumerable query = from p in EveryPoint where false select p;

			if (ability.name == "Leap")
				query = from p in EveryPoint where IsWalkable(p) && MyMath.ManhattanDistance(p, CurrentLCreature.position) == 2 select p;
			else if (ability.name == "Pommel Strike" || ability.name == "Decapitate")
				query = from c in AliveCreatures where c.IsAdjacentTo(CurrentLCreature) select c.position;

			return query.Cast<ZPoint>().ToList();
		}
	}
}
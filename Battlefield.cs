using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Battlefield
{
	private char[,] data;
	public Palette palette;
	public Collection<LObject> objects = new Collection<LObject>();

	public LObject currentObject, spotlightObject;
	private GObject gObject;
	private Texture2D currentObjectSymbol, zSelectionTexture;

	public AnimationQueue scaleAnimations = new AnimationQueue();
	public AnimationQueue combatAnimations = new AnimationQueue();

	private MainScreen M { get { return MainScreen.Instance; } }
	private Player P { get { return World.Instance.player; } }

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public List<Creature> Creatures	{ get {	return (from c in objects where c is Creature select c as Creature).Cast<Creature>().ToList(); } }
	public List<Creature> AliveCreatures { get { return (from c in Creatures where c.isActive select c).Cast<Creature>().ToList(); } }

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

	public void SetTile(ZPoint p, char value) {	if (InRange(p)) data[p.x, p.y] = value;	}

	public void LoadTextures()
	{
		currentObjectSymbol = MainScreen.Instance.game.Content.Load<Texture2D>("other/currentObject");
		zSelectionTexture = MainScreen.Instance.game.Content.Load<Texture2D>("other/zSelection");
	}

	private bool InRange(ZPoint p)
	{
		return p.InBoundaries(new ZPoint(0, 0), Size - new ZPoint(1, 1));
	}

	public LObject GetLObject(ZPoint p)
	{
		foreach (LObject l in objects) if (l.position.TheSameAs(p)) return l;
		return null;
	}

	public Creature GetCreature(ZPoint p)
	{
		var query = from l in objects where l is Creature && l.position.TheSameAs(p) && l.isActive select l;
		if (query.Count() > 0) return query.First() as Creature;
		else return null;
	}

	public Creature CurrentCreature { get { return currentObject as Creature; } }

	public bool IsWalkable(ZPoint p)
	{
		if (!InRange(p)) return false;
		if (!this[p].IsWalkable || GetCreature(p) != null) return false;
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

	private void AddCreature(PartyCreature pc, bool isInParty, bool isAIControlled)
	{
		if (pc.uniqueName == P.Name)
		{
			LPlayer item = new LPlayer(pc as PartyCharacter);
			objects.Add(item);
		}
		else if (pc is PartyCharacter)
		{
			Character item = new Character(pc as PartyCharacter, isInParty, isAIControlled);
			objects.Add(item);
		}
		else if (pc is PartyCreep)
		{
			Creep item = new Creep(pc as PartyCreep, isInParty, isAIControlled);
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
		foreach (PartyCreature member in World.Instance.player.party) AddCreature(member, true, false);
		foreach (PartyCreature member in g.party) AddCreature(member, false, true);

		LObject item = new LObject("Tree");
		objects.Add(item);

		foreach (LObject l in objects)
		{
			l.SetPosition(RandomFreeTile(), 60.0f, false);
			if (l is LPlayer) l.SetInitiative(0.1f, 60.0f, false);
			else l.SetInitiative(-World.Instance.random.Next(100) / 100.0f, 60.0f, false);
		}

		if(NextLObject is Creature) { if ((NextLObject as Creature).isAIControlled) NextLObject.Run(); }
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
	}

	public Vector2 GraphicCoordinates(RPoint p)
	{
		return new Vector2(100, 100) + new Vector2(32 * p.x, 32 * p.y);
	}

	public ZPoint ZCoordinates(Vector2 mouse)
	{
		Vector2 logical = (mouse - new Vector2(100, 100)) / 32.0f;
		return new ZPoint((int)logical.X, (int)logical.Y);
	}

	public void SetSpotlight(ZPoint p)
	{
		var query = from o in objects where o is Creature && o.position.TheSameAs(p) && o.isActive orderby o.Importance select o;
		if (query.Count() > 0) spotlightObject = query.First();
	}

	private void DrawScale(ZPoint position, ZPoint zMouse)
	{
		int length = 500, height = 20;
		Screen screen = new Screen(position, new ZPoint(length, height));

		screen.Fill(Stuff.DarkDarkGray);

		var query = from l in objects where l.isActive orderby l.rInitiative.x select l;
		float zeroInitiative = -query.Last().rInitiative.x;

		foreach (Creature c in query)
		{
			int rInitiative = (int)(100.0f * (-c.rInitiative.x - zeroInitiative)) + 1;
			int y = -32, z = -32;
			if (c.isInParty) { y = height; z = 0; }

			Color color = Color.White;
			if (c.position.TheSameAs(zMouse)) color = c.RelationshipColor;

			if (scaleAnimations.CurrentTarget != c.rInitiative)
				screen.DrawRectangle(new ZPoint(rInitiative, z), new ZPoint(1, height + 32), color);

			screen.Draw(c.texture, new ZPoint(rInitiative + 1, y));
		}
	}

	private void DrawAbilities(Creature c, Screen screen, ZPoint position)
	{
		foreach (Ability a in c.partyCreature.Abilities)
			screen.Draw(a.texture, position + new ZPoint(48 * c.partyCreature.Abilities.IndexOf(a), 0));
	}

	private void DrawInfo(Creature c, ZPoint position)
	{
		int length = 288, height = 108;
		Screen screen = new Screen(position, new ZPoint(length, height));
		screen.Fill(Color.Black);

		float hpFraction = (float)c.HP / c.MaxHP;
		float enduranceFraction = (float)c.Endurance / c.MaxHP;

		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(hpFraction * length), 20), new Color(0.2f, 0, 0));
		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(enduranceFraction * length), 20), new Color(0.4f, 0, 0));
		for (int i = 1; i <= c.MaxHP; i++) screen.DrawRectangle(new ZPoint((int)(i * (float)length / c.MaxHP), 0), new ZPoint(1, 20), Color.Black);

		SpriteFont font = MainScreen.Instance.verdanaBoldFont;

		string name = c.Name;
		if (c.UniqueName != "") name = c.UniqueName + ", " + c.Name;
        screen.DrawString(font, name, 23, Color.White);

		screen.DrawString(font, c.Damage.ToString(), new ZPoint(5, 43), Color.White);
		screen.DrawString(font, c.Attack.ToString() + "/" + c.Defence, 43, Color.White);
		screen.DrawString(font, c.Armor.ToString(), new ZPoint(length - 12, 43), Color.White);

		DrawAbilities(c, screen, new ZPoint(0, height - 48));
	}

	private Collection<ZPoint> Zone(int radius)
	{
		Collection<ZPoint> result = new Collection<ZPoint>();

		result.Add(currentObject.position);
		for (int i = 0; i < radius; i++)
		{
			List<ZPoint> copy = (from item in result select item).Cast<ZPoint>().ToList();
			foreach (ZPoint p in copy)
				foreach (ZPoint.Direction d in ZPoint.Directions)
				{
					ZPoint candidate = p.Shift(d);
					var query = from q in result where q.TheSameAs(candidate) select q;
					if (IsWalkable(candidate) && query.Count() == 0) result.Add(candidate);
				}
		}

		return result;
	}

	private void DrawZones()
	{
		var greenZone = Zone(CurrentCreature.controlMovementCounter);
		var yellowZone = Zone(CurrentCreature.controlMovementCounter + 1).Except(Zone(CurrentCreature.controlMovementCounter));

		foreach (ZPoint p in yellowZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0.5f, 0, 0, 0.5f));
		//foreach (ZPoint p in greenZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0, 0.5f, 0, 0.5f));
	}

	public void Draw(Vector2 mouse)
	{
		foreach (LObject l in objects)
		{
			l.movementAnimations.Draw();
			l.scaleAnimations.Draw();
		}

		for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++)
		{
			ZPoint p = new ZPoint(i, j);
			M.Draw(this[p].texture, GraphicCoordinates(p));
		}

		DrawZones();

		var query = from l in objects orderby l.isActive select l;
		foreach (LObject l in query) M.Draw(l.texture, GraphicCoordinates(l.rPosition));

		combatAnimations.Draw();
		scaleAnimations.Draw();

		if (combatAnimations.IsEmpty) M.Draw(currentObjectSymbol, GraphicCoordinates(currentObject.rPosition) - new Vector2(0, 12));

		ZPoint zMouse = ZCoordinates(mouse);
		if (InRange(zMouse)) M.Draw(zSelectionTexture, GraphicCoordinates(zMouse));
		if (spotlightObject != null && spotlightObject != currentObject) M.Draw(zSelectionTexture, GraphicCoordinates(spotlightObject.rPosition));

		DrawScale(new ZPoint(100, 650), zMouse);
		DrawInfo(spotlightObject as Creature, new ZPoint(750, 400));
	}

	public LObject NextLObject
	{
		get
		{
			var query = from l in objects where l.isActive orderby -l.initiative select l;
			if (query.Count() != 0) return query.First();
			else return null;
		}
	}

	public void CheckForEvents()
	{
		var aliveMonsters = from c in objects where c is Creature && c.isActive && !(c as Creature).isInParty select c;
		if (aliveMonsters.Count() == 0)
		{
			P.party.Clear();
			var aliveParty = from c in objects where c is Creature && c.isActive && (c as Creature).isInParty orderby c.Importance select c;

			foreach (Creature c in aliveParty)
			{
				c.partyCreature.hp = c.HP;
				c.partyCreature.endurance = c.Endurance;

				P.party.Add(c.partyCreature);
			}

			gObject.Kill();
			MyGame.Instance.battle = false;
		}
	}
}

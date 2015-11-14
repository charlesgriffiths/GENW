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
	public Ability ability = null;
	private GObject gObject;

	private Texture2D /*zSelectionTexture,*/ arrowTexture, targetTexture, damageIcon, armorIcon;

	public AnimationQueue scaleAnimations = new AnimationQueue();
	public AnimationQueue combatAnimations = new AnimationQueue();

	private List<DelayedDrawing> delayedDrawings = new List<DelayedDrawing>();

	private MainScreen M { get { return MainScreen.Instance; } }
	private Player P { get { return World.Instance.player; } }

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public List<LCreature> Creatures { get { return (from c in objects where c is LCreature select c as LCreature).Cast<LCreature>().ToList(); } }
	public List<LCreature> AliveCreatures { get { return (from c in Creatures where c.isActive select c).Cast<LCreature>().ToList(); } }

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

	public void LoadTextures()
	{
		//zSelectionTexture = M.game.Content.Load<Texture2D>("other/zSelection");
		arrowTexture = M.game.Content.Load<Texture2D>("other/arrow");
		targetTexture = M.game.Content.Load<Texture2D>("other/target");
		damageIcon = M.game.Content.Load<Texture2D>("other/damage");
		armorIcon = M.game.Content.Load<Texture2D>("other/armor");
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

	public LCreature GetLCreature(ZPoint p)
	{
		var query = from l in objects where l is LCreature && l.position.TheSameAs(p) && l.isActive select l;
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

	public void SetSpotlight()
	{
		ZPoint p = Mouse;
        var query = from o in objects where o is LCreature && o.position.TheSameAs(p) && o.isActive orderby o.Importance select o;
		if (query.Count() > 0) spotlightObject = query.First();
	}

	private void Draw(Texture2D texture, ZPoint zPosition)
	{ M.Draw(texture, GraphicCoordinates(zPosition)); }

	private void DrawScale(ZPoint position, ZPoint zMouse)
	{
		int length = 500, height = 20;
		Screen screen = new Screen(position, new ZPoint(length, height));

		screen.Fill(Stuff.DarkDarkGray);

		var query = from l in objects where l.isActive orderby l.rInitiative.x select l;
		float zeroInitiative = -query.Last().rInitiative.x;

		MouseTriggerLCreature trigger = null;
		foreach (LCreature c in query)
		{
			int rInitiative = (int)(100.0f * (-c.rInitiative.x - zeroInitiative)) + 1;
			int y = -32, z = -32;
			if (c.isInParty) { y = height; z = 0; }

			ZPoint iconPosition = new ZPoint(rInitiative + 1, y);
			MouseTriggerLCreature.Set(c, screen.position + iconPosition, new ZPoint(32, 32));
			trigger = MouseTriggerLCreature.GetUnderMouse();

			Color color = Color.White;
			if (c.position.TheSameAs(zMouse) || (trigger != null && c == trigger.creature)) color = c.RelationshipColor;

			if (scaleAnimations.CurrentTarget != c.rInitiative)
				screen.DrawRectangle(new ZPoint(rInitiative, z), new ZPoint(1, height + 32), color);
			
            screen.Draw(c.texture, iconPosition);
		}

		if (trigger != null) Draw(M.zSelectionTexture, trigger.creature.position);
		MouseTriggerLCreature.Clear();
	}

	private void DrawAbilities(LCreature c, Screen screen, ZPoint position)
	{
		for (int i = 0; i < 6; i++) MouseTriggerKeyword.Set("Ability", i, screen.position + position + new ZPoint(48 * i, 0), new ZPoint(48, 48));

		foreach (Ability a in c.data.Abilities)
		{
			int i = c.data.Abilities.IndexOf(a);
			MouseTriggerKeyword t = MouseTriggerKeyword.Get("Ability", i);
			M.Draw(a.texture, t.position);

			if (a.targetType == Ability.TargetType.Passive) M.DrawRectangle(t.position, t.size, new Color(0, 0, 0, 0.7f));
			else if (c == CurrentLCreature) M.DrawStringWithShading(M.smallFont, Stuff.AbilityHotkeys[i].ToString(), t.position + new ZPoint(37, 33), Color.White);
		}

		MouseTriggerKeyword forDescription = MouseTriggerKeyword.GetUnderMouse("Ability");
		if (forDescription != null && forDescription.parameter < c.data.Abilities.Count)
		{
			Ability a = c.data.Abilities[forDescription.parameter];
			M.Draw(a.texture, forDescription.position, Color.Red);

			foreach (MouseTriggerKeyword t in MouseTriggerKeyword.GetAll("Ability"))
				if (t.parameter != forDescription.parameter) M.DrawRectangle(t.position, t.size, new Color(0, 0, 0, 0.8f));

			screen.DrawString(M.smallFont, a.name + ". " + a.description, position, Color.White, screen.size.x);
		}
	}

	private void DrawInfo(LCreature c, ZPoint position)
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

		screen.Draw(damageIcon, new ZPoint(0, 40));
		screen.DrawString(font, c.Damage.ToString(), new ZPoint(22, 43), Color.White);
		screen.DrawString(font, c.Attack.ToString() + "/" + c.Defence, 43, Color.White);
		screen.DrawString(font, c.Armor.ToString(), new ZPoint(length - 32, 43), Color.White);
		screen.Draw(armorIcon, new ZPoint(length - 20, 40));

		DrawAbilities(c, screen, new ZPoint(0, height - 48));
	}

	private void AddToFrontier(List<FramedZPoint> list, ZPoint zPoint, ZPoint.Direction d, ZPoint start)
	{
		if (!IsWalkable(zPoint) && !zPoint.TheSameAs(start)) return;

		FramedZPoint item = new FramedZPoint(zPoint, d, true);
        var query = from p in list where p.data.TheSameAs(zPoint) select p;
		if (query.Count() == 0) list.Add(item);
	}

	private void AddToFrontier(List<FramedZPoint> list, ZPoint zPoint) { AddToFrontier(list, zPoint, ZPoint.Direction.Right, new ZPoint(-2, -2)); }

	public List<ZPoint.Direction> Path(ZPoint start, ZPoint finish)
	{
		List<FramedZPoint> visited = new List<FramedZPoint>();
		visited.Add(new FramedZPoint(finish, true));

		while (!start.IsIn(visited))
		{
			List<FramedZPoint> frontier = 
				(from p in visited where p.onFrontier orderby MyMath.ManhattanDistance(p.data, start) select p)
				.Cast<FramedZPoint>().ToList();

			if (frontier.Count() == 0) return null;

			foreach (FramedZPoint p in frontier)
			{
				p.onFrontier = false;
				foreach (ZPoint.Direction d in ZPoint.Directions)
                    AddToFrontier(visited, p.data.Shift(d), ZPoint.Opposite(d), start);
			}
		}

		List<ZPoint.Direction> result = new List<ZPoint.Direction>();

		ZPoint position = start;
		while (!position.TheSameAs(finish))
		{
			ZPoint.Direction d = position.GetDirection(visited);
            result.Add(d);
			position = position.Shift(d);
		}

		return result;
	}

	private void DrawPath(ZPoint start, List<ZPoint.Direction> path, LCreature c)
	{
		ZPoint position = start;
		int i = 1;

		foreach (ZPoint.Direction d in path)
		{
			if (c != null && i == path.Count())
				delayedDrawings.Add(new DelayedDrawing(M.verdanaBoldFont, CurrentLCreature.HitChance(c).ToString() + "%",
					new ZPoint(GraphicCoordinates(position)) + 16 * new ZPoint(d) + new ZPoint(1, 8), Color.Red));
			else
            {
				M.spriteBatch.Draw(arrowTexture, position: GraphicCoordinates(position) + 16 * new ZPoint(d) + new Vector2(16, 16),
					rotation: ZPoint.Angle(d), origin: new Vector2(16, 16));
				position = position.Shift(d);
				i++;
			}
		}
	}

	private List<FramedZPoint> TotalFramedZone
	{
		get
		{
			List<FramedZPoint> visited = new List<FramedZPoint>();
			visited.Add(new FramedZPoint(CurrentLCreature.position, true));

			for (int i = 0; i <= CurrentLCreature.controlMovementCounter; i++)
			{
				List<FramedZPoint> frontier = (from p in visited where p.onFrontier select p).Cast<FramedZPoint>().ToList();
				foreach (FramedZPoint p in frontier)
				{
					p.onFrontier = false;
					foreach (ZPoint.Direction d in ZPoint.Directions) AddToFrontier(visited, p.data.Shift(d));
				}
			}

			return visited.Cast<FramedZPoint>().ToList();
		}
	}

	private List<ZPoint> TotalZone { get { return (from p in TotalFramedZone select p.data).Cast<ZPoint>().ToList(); } }
	private List<ZPoint> GreenZone { get { return (from p in TotalFramedZone where !p.onFrontier select p.data).Cast<ZPoint>().ToList(); } }
	private List<ZPoint> YellowZone { get { return (from p in TotalFramedZone where p.onFrontier select p.data).Cast<ZPoint>().ToList(); } }

	private List<ZPoint> ReachableCreaturePositions
	{ get {	return (from c in AliveCreatures where !c.isInParty && c.IsAdjacentTo(GreenZone) select c.position).Cast<ZPoint>().ToList(); } }

	private void DrawZones()
	{
		if (ability == null)
		{
			foreach (ZPoint p in GreenZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0, 0.3f, 0, 0.1f));
			foreach (ZPoint p in YellowZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0.3f, 0.3f, 0, 0.1f));

			foreach (ZPoint p in ReachableCreaturePositions)
				M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0.3f, 0.3f, 0, 0.1f));

			if (Mouse.IsIn(TotalZone)) DrawPath(CurrentLCreature.position, Path(CurrentLCreature.position, Mouse), null);
			else if (Mouse.IsIn(ReachableCreaturePositions)) DrawPath(CurrentLCreature.position, Path(CurrentLCreature.position, Mouse), GetLCreature(Mouse));
		}
		else
		{
			foreach (ZPoint p in AbilityZone)
				M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0, 0.3f, 0, 0.1f));
		}
	}

	public void GoTo()
	{
		if (Mouse.IsIn(TotalZone) || Mouse.IsIn(ReachableCreaturePositions))
		{
			ZPoint start = CurrentLCreature.position;
			foreach (ZPoint.Direction d in Path(start, Mouse)) CurrentLCreature.TryToMove(d, true);
		}
	}

	public void Draw()
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

		if (combatAnimations.IsEmpty) DrawZones();

		var query = from l in objects orderby l.isActive select l;
		foreach (LObject l in query) M.Draw(l.texture, GraphicCoordinates(l.rPosition));

		combatAnimations.Draw();
		scaleAnimations.Draw();

		if (ability != null && Mouse.IsIn(AbilityZone)) Draw(targetTexture, Mouse);
		else if (InRange(Mouse)) Draw(M.zSelectionTexture, Mouse);
		if (spotlightObject != null && spotlightObject != currentObject) M.Draw(M.zSelectionTexture, GraphicCoordinates(spotlightObject.rPosition));

		foreach (DelayedDrawing dd in delayedDrawings) dd.Draw();
		delayedDrawings.Clear();

		DrawScale(new ZPoint(100, 650), Mouse);
		DrawInfo(spotlightObject as LCreature, new ZPoint(750, 400));
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
		var aliveMonsters = from c in objects where c is LCreature && c.isActive && !(c as LCreature).isInParty select c;
		if (aliveMonsters.Count() == 0)
		{
			P.party.Clear();
			var aliveParty = from c in objects where c is LCreature && c.isActive && (c as LCreature).isInParty orderby c.Importance select c;

			foreach (LCreature c in aliveParty)
			{
				//c.partyCreature.hp = c.HP;
				//c.partyCreature.endurance = c.Endurance;

				P.party.Add(c.data);
			}

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

public class FramedZPoint
{
	public ZPoint data;
	public ZPoint.Direction d;
	public bool onFrontier;
	
	public FramedZPoint(ZPoint datai, ZPoint.Direction di, bool onFrontieri)
	{ data = datai; d = di; onFrontier = onFrontieri; }

	public FramedZPoint(ZPoint datai, bool onFrontieri)
	{ data = datai; d = ZPoint.Direction.Right; onFrontier = onFrontieri; }
}

class DelayedDrawing
{
	private SpriteFont font;
	private string text;
	private ZPoint position;
	private Color color;

	public DelayedDrawing(SpriteFont fonti, string texti, ZPoint positioni, Color colori)
	{ font = fonti; text = texti; position = positioni; color = colori; }

	public void Draw() { MainScreen.Instance.DrawString(font, text, position, color); }
}
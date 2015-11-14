﻿using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Player : GObject
{
	public bool[,] visitedLocations;
	private List<GObject> visibleObjects = new List<GObject>();

	public Player()
	{
		shape = new GObjectShape();
		shape.name = "Karl";
		shape.speed = 1.0f;
		shape.isActive = true;

		Character playerCharacter = new Character(shape.name, "Strong", "Floran", "Psionic", "Eden", "Merchant");
		playerCharacter.inventory.Add("Club");
		playerCharacter.inventory.Add("Leather Armor");
		party.Add(playerCharacter);

		Character c2 = new Character("Bob", "Agile", "Morlock", "Fighter", "The Scorch", "Pitfighter");
		c2.inventory.Add("Dagger");
		c2.inventory.Add("Force Staff");
		party.Add(c2);

		//party.Add(new Creep("Krokar"));

		inventory.Add("Net");
		inventory.Add("Rope");
		inventory.Add("Inperium Coins", 5);
		inventory.Add("Alliance Coins", 7);
		inventory.Add("Longbow", 2);
		inventory.Add("Pickaxe", 3);
		inventory.Add("Buckler");
		inventory.Add("Chainmail");
		inventory.Add("Banana", 4);
		inventory.Add("Corpse Meat", 2);
	}

	public void LoadTextures()
	{
		Texture = M.game.Content.Load<Texture2D>("other/player");
	}

	public void DrawParty(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(10, 10));

		foreach (Creature member in party)
		{
			int i = party.IndexOf(member);
			screen.Draw(member.texture, new ZPoint(0, i * 40));

			float hpMissing = 1 - (float)member.hp / member.MaxHP;
			float enduranceMissing = 1 - (float)member.endurance / member.MaxHP;

			screen.DrawRectangle(new ZPoint(0, i * 40 + 32), new ZPoint(32, -(int)(enduranceMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));
			screen.DrawRectangle(new ZPoint(0, i * 40 + 32), new ZPoint(32, -(int)(hpMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));
		}

		var characters = from c in party where c is Character select c;
		foreach (Character c in characters) c.inventory.Draw(position + new ZPoint(42, party.IndexOf(c) * 40));
		inventory.Draw(position + new ZPoint(42, (from c in party where c is Character select c).Count() * 40));
	}

	public override void Draw()
	{
		movementAnimations.Draw();
		M.spriteBatch.Draw(Texture, M.GraphicCoordinates(rPosition));
	}

	public override void Kill()
	{
		Log.WriteLine("Game Over!");
		MainScreen.Instance.game.Exit();
	}

	public override void ProcessCollisions(GObject g)
	{
		if (g.dialog != null) M.dialogScreen.StartDialog(g.dialog, g);
	}

	public override void Move(HexPoint.HexDirection d)
	{
		base.Move(d);
		UpdateVisitedLocations();
	}

	private bool NewObjectsVisible()
	{
		List<GObject> newVisibleObjects = (from o in W.gObjects where W.map.IsInView(position, o.position) select o).Cast<GObject>().ToList();
		List<GObject> query = (from o in newVisibleObjects where !visibleObjects.Contains(o) select o).Cast<GObject>().ToList();
		visibleObjects = newVisibleObjects;
		return query.Count > 0;
	}

	public override void Run(){}

	public bool this[ZPoint p]
	{
		get
		{
			if (W.map.InRange(p)) return visitedLocations[p.x, p.y];
			else return false;
		}
		set
		{
			if (W.map.InRange(p)) visitedLocations[p.x, p.y] = value;
		}
	}

	public void UpdateVisitedLocations()
	{
		if (visitedLocations == null)
		{
			visitedLocations = new bool[W.map.Size.x, W.map.Size.y];
			for (int j = 0; j < W.map.Size.y; j++)
				for (int i = 0; i < W.map.Size.x; i++)
					visitedLocations[i, j] = false;
		}

		for (int j=-2; j <= 2; j++) for (int i=-2; i <=2; i++)
		{
				ZPoint p = position + new ZPoint(i, j);
				if (W.map.IsInView(position, p)) this[p] = true;
		}

		ZPoint pvr = new ZPoint(3, 3);
		if (!((ZPoint)position).InBoundaries(W.camera - W.viewRadius + pvr, W.camera + W.viewRadius - pvr))	W.camera = position;
	}

	private void AddToFrontier(List<FramedHexPoint> list, HexPoint hexPoint, HexPoint.HexDirection d, float costSoFar)
	{
		if (!W.map.IsWalkable(hexPoint)) return;
		if (HexPoint.Distance(hexPoint, position) > 15) return;

		FramedHexPoint item = new FramedHexPoint(hexPoint, d, true, costSoFar + W.map[hexPoint].type.travelTime);

		var query = from p in list where p.data.TheSameAs(item.data) select p;

		if (query.Count() == 0) list.Add(item);
		else
		{
			FramedHexPoint old = query.Single();
			if (item.cost < old.cost)
			{
				list.Remove(old);
				list.Add(item);
			}
		}
	}

	public void GoTo()
	{
		HexPoint destination = M.Mouse;

		if (!W.map.IsWalkable(destination)) return;
		if (!W.player[destination]) return;

		List<FramedHexPoint> visited = new List<FramedHexPoint>();
		visited.Add(new FramedHexPoint(destination, HexPoint.HexDirection.N, true, 0));

		while (true)
		{
			List<FramedHexPoint> frontier = (from p in visited where p.onFrontier orderby p.cost + HexPoint.Distance(p.data, position) select p).Cast<FramedHexPoint>().ToList();
			if (frontier.Count() == 0) return;

			foreach (FramedHexPoint p in frontier)
			{
				p.onFrontier = false;
				foreach (HexPoint.HexDirection d in HexPoint.Directions)
					AddToFrontier(visited, p.data.Shift(d), HexPoint.Opposite(d), p.cost);
			}

			var isFinished = from p in visited where p.data.TheSameAs(position) && !p.onFrontier select p;
			if (isFinished.Count() > 0) break;
		}

		for (int i = 0; i < 20 && !position.TheSameAs(destination); i++)
		{
			HexPoint.HexDirection d = (from p in visited where p.data.TheSameAs(position) select p).Single().d;
			Move(d);

			if (NewObjectsVisible()) break;
		}
	}
}

class FramedHexPoint
{
	public HexPoint data;
	public HexPoint.HexDirection d;
	public bool onFrontier;
	public float cost;
	
	public FramedHexPoint(HexPoint datai, HexPoint.HexDirection di, bool onFrontieri, float costi)
		{ data = datai; d = di; onFrontier = onFrontieri; cost = costi; }
}
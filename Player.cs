﻿using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Player : GObject
{
	public bool FOVEnabled = true;
	public bool[,] visitedLocations;

	public Player()
	{
		shape = new GObjectShape();
		shape.name = "Karl";
		shape.speed = 1.0f;
		shape.isActive = true;

		PartyCharacter playerCharacter = new PartyCharacter(shape.name, "Agile", "Morlock", "Fighter", "The Scorch", "Merchant");
		party.Add(playerCharacter);

		party.Add(new PartyCreep("Krokar"));
	}

	public void LoadTextures()
	{
		Texture = M.game.Content.Load<Texture2D>("other/player");
	}

	public void DrawParty(ZPoint position)
	{
		const int length = 110, height = 400;
		Screen screen = new Screen(position, new ZPoint(length, height));

		foreach (PartyCreature member in party)
		{
			int i = party.IndexOf(member);
			screen.Draw(member.texture, new ZPoint(0, i * 40));

			float hpMissing = 1 - (float)member.hp / member.MaxHP;
			float enduranceMissing = 1 - (float)member.endurance / member.MaxHP;

			screen.DrawRectangle(new ZPoint(0, i * 40 + 32), new ZPoint(32, -(int)(enduranceMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));
			screen.DrawRectangle(new ZPoint(0, i * 40 + 32), new ZPoint(32, -(int)(hpMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));
		}
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
		if (HexPoint.Distance(hexPoint, position) > 10) return;

		FramedHexPoint item = new FramedHexPoint(hexPoint, d, true, costSoFar + W.map[hexPoint].type.travelTime);

		var query = from p in list where p.p.TheSameAs(item.p) select p;

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

	public void GoTo(HexPoint destination)
	{
		/*		for (int i = 0; i < 10 && !position.TheSameAs(destination); i++)
				{
					var viableDirections = from d in HexPoint.Directions where W.map.IsWalkable(position.Shift(d)) orderby HexPoint.Distance(position.Shift(d), destination) select d;
					if (viableDirections.Count() > 0) Move(viableDirections.First());
				}
		*/

		if (!W.map.IsWalkable(destination)) return;
		if (!W.player[destination]) return;

		List<FramedHexPoint> visited = new List<FramedHexPoint>();
		visited.Add(new FramedHexPoint(destination, HexPoint.HexDirection.N, true, 0));

		while (true)
		{
			List<FramedHexPoint> frontier = (from p in visited where p.onFrontier orderby p.cost + 0.5f * HexPoint.Distance(p.p, position) select p).Cast<FramedHexPoint>().ToList();
			if (frontier.Count() == 0) return;

			foreach (FramedHexPoint p in frontier)
			{
				p.onFrontier = false;
				foreach (HexPoint.HexDirection d in HexPoint.Directions)
					AddToFrontier(visited, p.p.Shift(d), HexPoint.Opposite(d), p.cost);
			}

			var isFinished = from p in visited where p.p.TheSameAs(position) && !p.onFrontier select p;
			if (isFinished.Count() > 0) break;
		}

		for (int i = 0; i < 20 && !position.TheSameAs(destination); i++)
		{
			HexPoint.HexDirection d = (from p in visited where p.p.TheSameAs(position) select p).Single().d;
			Move(d);
		}
	}
}

class FramedHexPoint
{
	public HexPoint p;
	public HexPoint.HexDirection d;
	public bool onFrontier;
	public float cost;
	
	public FramedHexPoint(HexPoint pi, HexPoint.HexDirection di, bool onFrontieri, float costi)
		{ p = pi; d = di; onFrontier = onFrontieri; cost = costi; }
}
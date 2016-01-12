using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public partial class Player : GlobalObject
{
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
			List<FramedHexPoint> frontier = (from p in visited where p.onFrontier
											 orderby p.cost + HexPoint.Distance(p.data, position)
											 select p).Cast<FramedHexPoint>().ToList();

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

public partial class Battlefield
{
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

	private void DrawPath(ZPoint start, List<ZPoint.Direction> path, LocalObject c)
	{
		ZPoint position = start;
		int i = 1;

		foreach (ZPoint.Direction d in path)
		{
			if (c != null && i == path.Count())
				delayedDrawings.Add(new DelayedDrawing(M.fonts.verdanaBold, current.attack.HitChance(c).ToString() + "%",
					new ZPoint(GraphicCoordinates(position)) + 16 * new ZPoint(d) + new ZPoint(1, 8), Color.Red));
			else
			{
				M.spriteBatch.Draw(NamedTexture.Get("other/arrow"), position: GraphicCoordinates(position) + 16 * new ZPoint(d) + new Vector2(16, 16),
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
			visited.Add(new FramedZPoint(current.p.value, true));

			for (int i = 0; i <= current.movement.counter; i++)
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

	private List<ZPoint> ReachableCreaturePositions { get {
		return (from c in ActiveObjects where !c.team.isInParty && c.p.IsReachableFrom(GreenZone) select c.p.value).Cast<ZPoint>().ToList(); } }

	public void GoTo()
	{
		bool melee = current.p.Range == 1, move = Mouse.IsIn(TotalZone), attack = Mouse.IsIn(ReachableCreaturePositions);
		if ((melee && (move || attack)) || (move && !attack))
		{
			ZPoint start = current.p.value;
			foreach (ZPoint.Direction d in Path(start, Mouse)) current.movement.MoveOrAttack(d, true);
		}
		else if (IsReachable(Mouse, current.p.value, current.p.Range))
			current.attack.Execute(Get(Mouse));
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
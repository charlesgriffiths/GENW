using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

public partial class Battlefield
{
	public void StartBattle(GlobalObject _global)
	{
		global = _global;
		//Load("Custom Mountain");
		Generate();
		MyGame.Instance.battle = true;
	}

	private void Generate()
	{
		//palette = Terrain.palette;
		palette = Palette.Get("Mountains");
		int width = Math.Min(6 + NumberOfCreatures, 27), height = Math.Min(6 + NumberOfCreatures, 22);

		data = new LocalCell[width, height];

		points.Clear();
		for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++) points.Add(new ZPoint(i, j));

		Fill();
	}

	public void Fill()
	{
		List<ZPoint> largestComponent;

		Func<List<ZPoint>, bool> isIsolated = (list) => {
			foreach (ZPoint p in list) if (p.x == 0 || p.y == 0 || p.x == Size.x - 1 || p.y == Size.y - 1) return false;
			return true; };

		do
		{
			FillRandom();
			FillWithObjects();
			largestComponent = LargestComponent();
		}
		while (largestComponent.Count < 4 * NumberOfCreatures || isIsolated(largestComponent));

		Run();
	}

	public void FillRandom()
	{
		var n1 = new OpenSimplexNoise();
		var n4 = new OpenSimplexNoise();

		foreach (ZPoint p in points)
		{
			double h1 = n1.Evaluate(0.1 * p.x, 0.1 * p.y);
			double h4 = n4.Evaluate(0.4 * p.x, 0.4 * p.y);
			Action<char> set = c => SetTile(p, c);

			if (palette == Palette.Get("Mountains"))
			{
				if (h1 < -0.2) set('.');
				else if (h1 < 0.2) set('_');
				else set('W');
			}
			else if (Palette.Get("Plains", "Barren Hills").Contains(palette))
			{
				if (h1 < -0.1) set('_');
				else set(',');
			}
			else if (palette == Palette.Get("Light Autumn Forest"))
			{
				if (h1 < 0) set(',');
				else set(';');

			}
			else if (palette == Palette.Get("Deep Autumn Forest"))
			{
				if (h1 < -0.3) set(',');
				else if (h1 < 0.3) set(';');
				else set(':');
			}
			else if (Palette.Get("Grass", "Light Forest", "Deep Forest", "Hills").Contains(palette)) set(h1 < 0 ? ',' : ';');
			else if (palette == Palette.Get("Swamp")) set(h4 < 0 ? '_' : '-');
			else SetTile(p, palette.data.RandomKey());

			if (Palette.Get("Hills", "Barren Hills").Contains(palette)) if (h4 < -0.4) set('W');
		}

		if (Terrain == GlobalTile.Get("Road")) AddRoad();
	}

	public void FillWithObjects()
	{
		objects.Clear();
		AddBridges(LocalShape.Get("Horizontal Wooden Bridge"), ZPoint.Direction.Right);
		AddBridges(LocalShape.Get("Vertical Wooden Bridge"), ZPoint.Direction.Down);

		foreach (ZPoint p in points)
		{
			if (String.Concat(from q in Range(p, 1) let o = Get(q) where o != null select o.CommonName).Contains("Bridge")) continue;
			LocalShape shape = this[p].spawns.Random(false);
			if (shape != null) Add(new LocalObject(shape), p, false, false);
		}

		foreach (LocalObject c in P.party) Add(c, null, true, false);
		foreach (LocalObject c in global.party) Add(c, null, false, true);

		for (int i = 0; i * 6 < global.inventory.Items.Count; i++) Add(new LocalObject(LocalShape.Get("Chest"), "", global.inventory));
	}

	private void Run()
	{
		global.inventory.Clear();
		var component = LargestComponent();

		foreach (LocalObject l in objects)
		{
			if (l.p.value == null) l.p.Set(component.Where(p => IsWalkable(p)).ToList().Random(), 0.01f, false);
			if (l.initiative != null) l.initiative.Set(l.uniqueName == P.uniqueName ? 0.1f : -R.Next(100) / 100.0f, 0.01f, false);
		}

		LocalObject next = NextObject;
		if (next.initiative.IsAIControlled) next.initiative.Run();

		current = next;
		spotlight = current;
	}

	private void Load(string name)
	{
		XmlNode xnode = MyXml.FirstChild("Data/Battlefields/" + name + ".xml");
		palette = Palette.Get(MyXml.GetString(xnode, "palette"));

		string text = xnode.InnerText;
		char[] delimiters = new char[] { '\r', '\n', ' ' };
		string[] dataLines = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
		int width = dataLines[0].Length;
		int height = dataLines.Length;

		data = new LocalCell[width, height];
		for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++) points.Add(new ZPoint(i, j));

		//for (int j = 0; j < height; j++) for (int i = 0; i < width; i++) data[i, j] = dataLines[j][i];
		foreach (ZPoint p in points) SetTile(p, dataLines[p.y][p.x]);
	}

	private void AddBridges(LocalShape shape, ZPoint.Direction d)
	{
		List<Tuple<ZPoint, ZPoint.Direction, int>> bridges = new List<Tuple<ZPoint, ZPoint.Direction, int>>();

		Func<ZPoint, ZPoint.Direction, int, bool> canAddBridge = (p, dir, l) =>
		{
			ZPoint end = p.Shift(dir, l + 1);
			ZPoint.Direction d1 = ZPoint.Previous(dir);
			ZPoint.Direction d2 = ZPoint.Next(dir);

			if (!InRange(end) || !InRange(p + d1) || !InRange(p + d2)) return false;
			if (data[p.x, p.y].tile != '_' || data[end.x, end.y].tile != '_') return false;

			for (int k = 1; k <= l; k++)
			{
				ZPoint p0 = p.Shift(dir, k);
				ZPoint p1 = p0 + d1, p2 = p0 + d2;
				LocalTile sky = LocalTile.Get("Sky");

				if (this[p0] != sky || this[p1] != sky || this[p2] != sky) return false;
			}
			return true;
		};

		for (int l = 1; l <= 10; l++)
		{
			for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++)
				{
					ZPoint p = new ZPoint(i, j);
					if (canAddBridge(p, d, l)) bridges.Add(new Tuple<ZPoint, ZPoint.Direction, int>(p, d, l));
				}
		}

		foreach (var t in bridges.Random(2)) for (int k = 1; k <= t.Item3; k++) Add(new LocalObject(shape), t.Item1.Shift(t.Item2, k));
	}

	private void AddRoad()
	{
		bool vertical = R.Next(2) == 0 ? false : true;
		ZPoint.Direction d = vertical ? ZPoint.Direction.Down : ZPoint.Direction.Right;
		int z = vertical ? R.Next(Size.x - 3) : R.Next(Size.y - 3);

		if (vertical)
		{
			for (int i = 0; i < Size.y; i++)
			{
				//SetTile(new ZPoint(z, i), '_');
				SetTile(new ZPoint(z + 1, i), 'r');
				SetTile(new ZPoint(z + 2, i), 'r');
				//SetTile(new ZPoint(z + 3, i), '_');
			}
		}
		else
		{
			for (int i = 0; i < Size.x; i++)
			{
				//SetTile(new ZPoint(i, z), '_');
				SetTile(new ZPoint(i, z + 1), 'r');
				SetTile(new ZPoint(i, z + 2), 'r');
				//SetTile(new ZPoint(i, z + 3), '_');
			}
		}
	}

	private List<ZPoint> LargestComponent()
	{
		var components = ConnectedComponents();
		if (components.Count == 0) return new List<ZPoint>();
		else return components.OrderBy(l => l.Count).Last();
	}

	private List<List<ZPoint>> ConnectedComponents()
	{
		List<List<ZPoint>> result = new List<List<ZPoint>>();
		List<ZPoint> remaining = points.Where(p => IsWalkable(p)).ToList();

		while (remaining.Count > 0)
		{
			List<ZPoint> frontier = new List<ZPoint>();
			List<ZPoint> component = new List<ZPoint>();

			ZPoint start = remaining.First();
			frontier.Add(start);

			while (frontier.Count > 0)
			{
				foreach (ZPoint p in frontier) component.Add(p);
				foreach (ZPoint p in frontier) if (p.IsIn(remaining)) remaining.Remove(p);
				foreach (ZPoint p in remaining) if (p.IsAdjacentTo(frontier)) frontier.Add(p);
				foreach (ZPoint p in points) if (p.IsIn(frontier) && !p.IsIn(remaining)) frontier.Remove(p);
			}

			result.Add(component);
		}

		return result;
	}
}
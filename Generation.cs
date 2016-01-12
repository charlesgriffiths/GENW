using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

public class CharPair
{
	private char c1, c2;

	public CharPair(char _c1, char _c2) { c1 = _c1;  c2 = _c2; }

	public bool AreTwins { get { return c1 == c2; } }
	public bool Contains(char c) { return c == c1 || c == c2; }
	public bool TheSameAs(char d1, char d2) { return (c1 == d1 && c2 == d2) || (c1 == d2 && c2 == d1); }
}

public partial class Battlefield
{
	private void Run()
	{
		foreach (LocalObject l in objects)
		{
			if (l.p.value == null) l.p.Set(RandomFreeTile(), 0.01f, false);
			if (l.initiative != null) l.initiative.Set(l.uniqueName == P.uniqueName ? 0.1f : -R.Next(100) / 100.0f, 0.01f, false);
		}

		LocalObject next = NextObject;
		if (next.initiative.IsAIControlled) next.initiative.Run();

		current = next;
		spotlight = current;
	}

	public void FillWithObjects()
	{
		objects.Clear();
		AddBridges(LocalShape.Get("Horizontal Wooden Bridge"), ZPoint.Direction.Right);
		AddBridges(LocalShape.Get("Vertical Wooden Bridge"), ZPoint.Direction.Down);

		foreach (LocalObject c in P.party) Add(c, null, true, false);
		foreach (LocalObject c in global.party) Add(c, null, false, true);

		foreach (ZPoint p in points)
		{
			if (String.Concat(from q in Range(p, 1) let o = Get(q) where o != null select o.CommonName).Contains("Bridge")) continue;
			LocalShape shape = this[p].spawns.Random(false);
			if (shape != null) Add(new LocalObject(shape), p);
		}

		for (int i = 0; i * 6 < global.inventory.Items.Count; i++) Add(new LocalObject(LocalShape.Get("Chest"), "", global.inventory));

		Run();
	}

	public void StartBattle(GlobalObject _global)
	{
		global = _global;
		//Load("Custom Mountain");
		Generate();
		objects.Clear();
		Add(P.party[0], null, true, false);
		Run();
		MyGame.Instance.battle = true;
	}

	public void FillRandom()
	{
		var noise_1 = new OpenSimplexNoise();

		//for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++)
		foreach (ZPoint p in points)
		{
			double h = noise_1.Evaluate(0.1 * p.x, 0.1 * p.y);

			if (h <= -0.2) SetTile(p, '.');
			else if (h <= 0.2) SetTile(p, '_');
			else SetTile(p, 'W');
		}
	}

	private void Generate()
	{
		palette = Palette.Get("mountains");
		int width = 27, height = 22;

		if (data == null) data = new LocalCell[width, height];
		for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++) points.Add(new ZPoint(i, j));

		FillRandom();
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
}
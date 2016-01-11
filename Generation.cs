using System;
using System.Xml;
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
	public double temperature = 0.1;

	private void Run()
	{
		foreach (LocalObject l in objects)
		{
			l.p.Set(RandomFreeTile(), 0.01f, false);
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
		foreach (LocalObject c in P.party) Add(c, null, true, false);
		foreach (LocalObject c in global.party) Add(c, null, false, true);

		int treeDensity = 15, deadTreeDensity = 40, poisonedTreeDensity = 1000;
		if (palette.name == "autumn") { treeDensity = 1000; deadTreeDensity = 50; poisonedTreeDensity = 10; }

		for (int i = 0; i < Size.x * Size.y / treeDensity; i++) Add(new LocalObject(LocalShape.Get("Tree")));
		for (int i = 0; i < Size.x * Size.y / deadTreeDensity; i++) Add(new LocalObject(LocalShape.Get("Dead Tree")));
		for (int i = 0; i < Size.x * Size.y / poisonedTreeDensity; i++) Add(new LocalObject(LocalShape.Get("Poisoned Tree")));

		for (int i = 0; i * 6 < global.inventory.Items.Count; i++) Add(new LocalObject(LocalShape.Get("Chest"), "", global.inventory));

		Run();
	}

	public void StartBattle(GlobalObject _global)
	{
		global = _global;
		//GlobalTile gTile = World.Instance.map[g.position];
		//string battlefieldName;
		//if (gTile.type.name == "mountainPass") battlefieldName = "Custom Mountain";
		//else battlefieldName = "Custom Mountain";
		//Load(battlefieldName);
		Generate();
		objects.Clear();
		Add(P.party[0], null, true, false);
		Run();
		MyGame.Instance.battle = true;
	}

	public void FillRandom() { for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++) data[i, j] = palette.data.RandomKey(); }

	private void Generate()
	{
		palette = Palette.Get("mountains");
		//palette = Palette.Get((new string[] { "mountains", "autumn" }).Random());
		int width = 27, height = 22;
		//int width = 12, height = 10;
		if (data == null) data = new char[width, height];
		FillRandom();
	}

	/*private double Energy(CharPair pair, ZPoint.Direction d)
	//{
		//if (pair.Contains('-') || pair.Contains('|'))
		//{
			double number = 1;
			if (pair.TheSameAs('-', '|')) return 10;
			else if (pair.Contains('-'))
			{
				if (d == ZPoint.Direction.Right || d == ZPoint.Direction.Left)
				{
					if (pair.TheSameAs('-', '-') || pair.TheSameAs('-', '_')) return -number;
					else return number;
				}
				else
				{
					if (pair.TheSameAs('-', 'o')) return -number;
					else return number;
				}
			}
			else
			{
				Log.Assert(pair.Contains('|'), "Battlefield.Energy");
				if (d == ZPoint.Direction.Up || d == ZPoint.Direction.Down)
				{
					if (pair.TheSameAs('|', '|') || pair.TheSameAs('|', '_')) return -number;
					else return number;
				}
				else
				{
					if (pair.TheSameAs('|', 'o')) return -number;
					else return number;
				}
			}
		}
		else return pair.AreTwins ? -1 : 1;
	}*/

	private double Energy(CharPair pair, ZPoint.Direction d) // нафиг, я делаю noise
	{
		if (pair.TheSameAs('W', 'o')) return 1;
		else if (pair.AreTwins) return -1;
		else return 1;
	}

	private double Energy(ZPoint p, char tile)
	{
		double result = 0;
		//result += tile == '-' || tile == '|' ? 0.5 : -0.5;
		result += tile == '_' ? -0.5 : 0.5;

		foreach (var d in ZPoint.Directions)
		{
			ZPoint z = p + d;
			if (!InRange(z)) continue;
			char a = data[z.x, z.y];

			result += Energy(new CharPair(tile, a), d);
			//result += tile == a ? -1 : 1;
		}

		return result;
	}

	public void Step(ZPoint p)
	{
		Dictionary<char, double> probabilities = new Dictionary<char, double>();
		foreach (var pair in palette.data)
		{
			double value = Math.Exp(-Energy(p, pair.Key) / temperature);
			//double value = pair.Key == '_' ? 10 : 1;
			probabilities.Add(pair.Key, value);
		}

		data[p.x, p.y] = probabilities.Random();
	}

	public void Step()
	{
		for (int i = 0; i < Size.x; i++) for (int j = 0 + i % 2; j < Size.y; j += 2) Step(new ZPoint(i, j));
		for (int i = 0; i < Size.x; i++) for (int j = 1 - i % 2; j < Size.y; j += 2) Step(new ZPoint(i, j));
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
}
using System.Xml;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Battlefield
{
	private char[,] data;
	private ZPoint p1, p2, m1, m2;
	private Collection<LObject> lObjects = new Collection<LObject>();

	public LObject currentLObject;

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public LTile this[ZPoint p]
	{
		get
		{
			if (InRange(p)) return BigBase.Instance.lTiles.Get(data[p.x, p.y].ToString());
			else
			{
				Log.Error("battlefield index out of range");
				return null;
			}
		}
		set
		{
			if (InRange(p)) data[p.x, p.y] = value.name[0];
		}
	}

	private bool InRange(ZPoint p)
	{
		return p.InBoundaries(new ZPoint(0, 0), Size - new ZPoint(1, 1));
	}

	public LObject GetLObject(ZPoint p)
	{
		foreach (LObject l in lObjects) if (l.position.TheSameAs(p) && l.isActive) return l;
		return null;
	}

	public bool IsWalkable(ZPoint p)
	{
		if (!this[p].IsWalkable || GetLObject(p) != null) return false;
		else return true;
	}

	private ZPoint RandomFreeTile(bool inParty)
	{
		ZPoint z1, z2;
		if (inParty) { z1 = p1; z2 = p2; }
		else { z1 = m1; z2 = m2; }

		for (int i = 0; i < 100; i++)
		{
			int zx = World.Instance.random.Next(z1.x, z2.x + 1);
			int zy = World.Instance.random.Next(z1.y, z2.y + 1);

			ZPoint z = new ZPoint(zx, zy);
			if (IsWalkable(z)) return z;
		}

		return new ZPoint(0, 0);
	}

	private void AddLObjects(string type, bool inParty, bool aiControlled, int quantity)
	{
		for (int i = 0; i < quantity; i++)
		{
			LObject lObject = new LObject(type, inParty, aiControlled);
			lObjects.Add(lObject);
		}
	}

	public void StartBattle(GObject g)
	{
		GTile gTile = World.Instance.map[g.position];
		string battlefieldName;
		if (gTile.picture == "Deep Forest" || gTile.picture == "Light Forest") battlefieldName = "foresty";
		else battlefieldName = "grassy";
		Load(battlefieldName);

		lObjects.Add(new LPlayer());
		AddLObjects("Morlock", true, false, World.Instance.player.partySize);
		if (g.name == "Morlocks") AddLObjects("Morlock", false, true, 2);
		else if (g.name == "Wild Dogs") AddLObjects("Wild Dog", false, true, 3);

		foreach (LObject l in lObjects) l.position = RandomFreeTile(l.isInParty);

		currentLObject = NextLObject;
		MainScreen.Instance.gameState = MainScreen.GameState.Local;
	}

	public void CheckForEvents()
	{
		Collection<LObject> aliveMonsters = new Collection<LObject>();
		foreach (LObject l in lObjects) if (l.isActive && !l.isInParty) aliveMonsters.Add(l);
		if (aliveMonsters.Count == 0)
		{
			lObjects.Clear();
			MainScreen.Instance.gameState = MainScreen.GameState.Global;
		}
	}

	private void Load(string name)
	{
		XmlNode xnode = MyXml.FirstChild("Battlefields/" + name + ".xml");
		int width = MyXml.GetInt(xnode, "width");
		int height = MyXml.GetInt(xnode, "height");
		data = new char[width, height];

		m1 = new ZPoint(MyXml.GetInt(xnode, "m1x"), MyXml.GetInt(xnode, "m1y"));
		m2 = new ZPoint(MyXml.GetInt(xnode, "m2x"), MyXml.GetInt(xnode, "m2y"));
		p1 = new ZPoint(MyXml.GetInt(xnode, "p1x"), MyXml.GetInt(xnode, "p1y"));
		p2 = new ZPoint(MyXml.GetInt(xnode, "p2x"), MyXml.GetInt(xnode, "p2y"));

		string text = xnode.InnerText;
		text = text.Replace('\n', ' ');
		text = text.Replace('\r', ' ');
		text = text.Replace(" ", "");
		Log.Assert(text.Length == width * height, "wrong battlefield data");

		for (int j = 0; j < height; j++)
		{
			for (int i = 0; i < width; i++) data[i, j] = text[i + j * width];
		}
	}

	private Vector2 GraphicCoordinates(ZPoint p)
	{
		return new Vector2(100, 100) + new Vector2(p.x * 32, p.y * 32);
    }

	public void Draw(SpriteBatch sb)
	{
		if (MainScreen.Instance.gameState != MainScreen.GameState.Local) return;

		for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++)
		{
			ZPoint p = new ZPoint(i, j);
			sb.Draw(this[p].texture, GraphicCoordinates(p));
		}

		foreach (LObject l in lObjects) sb.Draw(l.texture, GraphicCoordinates(l.position));
	}

	public LObject NextLObject //переписать на LINQ!
	{
		get
		{
			LObject result = lObjects[0]; //тут ошибка в случае отсутствия существ, потом исправить
			foreach (LObject l in lObjects) if (l.initiative > result.initiative && l.isActive) result = l;
			return result;
		}
	}
}

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
	//private ZPoint p1, p2, m1, m2;
	private Collection<LObject> lObjects = new Collection<LObject>();

	public LObject currentLObject;
	private Texture2D currentLObjectSymbol, zSelectionTexture;

	public Queue<RMove> scaleAnimations = new Queue<RMove>();

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public LTile this[ZPoint p]
	{
		get
		{
			if (InRange(p)) return palette[data[p.x, p.y]];
			//return BigBase.Instance.lTiles.Get(data[p.x, p.y].ToString());
			else
			{
				Log.Error("battlefield index out of range");
				return null;
			}
		}
		//set
		//{
			//if (InRange(p)) data[p.x, p.y] = value.name[0]; // !!!
		//}
	}

	public void SetTile(ZPoint p, char value) {	if (InRange(p)) data[p.x, p.y] = value;	}

	public void LoadTextures()
	{
		currentLObjectSymbol = MainScreen.Instance.game.Content.Load<Texture2D>("other/currentObject");
		zSelectionTexture = MainScreen.Instance.game.Content.Load<Texture2D>("other/zSelection");
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
		if (!InRange(p)) return false;
		if (!this[p].IsWalkable || GetLObject(p) != null) return false;
		return true;
	}

	private ZPoint RandomFreeTile(bool inParty)
	{
		//ZPoint z1, z2;
		//if (inParty) { z1 = p1; z2 = p2; }
		//else { z1 = m1; z2 = m2; }

		for (int i = 0; i < 100; i++)
		{
			//int zx = World.Instance.random.Next(z1.x, z2.x + 1);
			//int zy = World.Instance.random.Next(z1.y, z2.y + 1);
			int zx = World.Instance.random.Next(Size.x);
			int zy = World.Instance.random.Next(Size.y);

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
		if (gTile.type.name == "mountainPass") battlefieldName = "Custom Mountain";
		else battlefieldName = "Custom Mountain";
		Load(battlefieldName);

		lObjects.Clear();
		lObjects.Add(new LPlayer());
		AddLObjects("Morlock", true, false, World.Instance.player.partySize);
		if (g.name == "Morlocks") AddLObjects("Morlock", false, true, 2);
		else if (g.name == "Wild Dogs") AddLObjects("Wild Dog", false, true, 3);

		foreach (LObject l in lObjects)
		{
			l.SetPosition(RandomFreeTile(l.isInParty), 60.0f, false);
			if (l is LPlayer) l.SetInitiative(0.1f, 60.0f);
			else l.SetInitiative(-World.Instance.random.Next(100) / 100.0f, 60.0f);
		}

        if (NextLObject.isAIControlled) NextLObject.Run();
		currentLObject = NextLObject;
		MyGame.Instance.gameState = MyGame.GameState.Local;
	}

	public void CheckForEvents()
	{
		Collection<LObject> aliveMonsters = new Collection<LObject>();
		foreach (LObject l in lObjects) if (l.isActive && !l.isInParty) aliveMonsters.Add(l);
		if (aliveMonsters.Count == 0)
		{
			lObjects.Clear();
			MyGame.Instance.gameState = MyGame.GameState.Global;
		}
	}

	private void Load(string name)
	{
		XmlNode xnode = MyXml.FirstChild("Data/Battlefields/" + name + ".xml");
		palette = BigBase.Instance.palettes.Get(MyXml.GetString(xnode, "palette"));

		//int width = MyXml.GetInt(xnode, "width");
		//int height = MyXml.GetInt(xnode, "height");

		string text = xnode.InnerText;
		char[] delimiters = new char[] { '\r', '\n', ' ' };
		string[] dataLines = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
		int width = dataLines[0].Length;
		int height = dataLines.Length;
		data = new char[width, height];

		for (int j = 0; j < height; j++) for (int i = 0; i < width; i++) data[i, j] = dataLines[j][i];
		

		//m1 = new ZPoint(MyXml.GetInt(xnode, "m1x"), MyXml.GetInt(xnode, "m1y"));
		//m2 = new ZPoint(MyXml.GetInt(xnode, "m2x"), MyXml.GetInt(xnode, "m2y"));
		//p1 = new ZPoint(MyXml.GetInt(xnode, "p1x"), MyXml.GetInt(xnode, "p1y"));
		//p2 = new ZPoint(MyXml.GetInt(xnode, "p2x"), MyXml.GetInt(xnode, "p2y"));

		//(!)аккуратно: хотим, чтобы он сам определил ширину и высоту
		/*
				string text = xnode.InnerText;
				text = text.Replace('\n', ' ');
				text = text.Replace('\r', ' ');
				text = text.Replace(" ", "");
				Log.Assert(text.Length == width * height, "wrong battlefield data");

				for (int j = 0; j < height; j++)
				{
					for (int i = 0; i < width; i++) data[i, j] = text[i + j * width];
				}
		*/
	}

	private Vector2 GraphicCoordinates(RPoint p)
	{
		return new Vector2(100, 100) + new Vector2(32 * p.x, 32 * p.y);
	}

	public ZPoint ZCoordinates(Vector2 mouse)
	{
		Vector2 logical = (mouse - new Vector2(100, 100)) / 32.0f;
		return new ZPoint((int)logical.X, (int)logical.Y);
	}

	private void DrawScale(ZPoint position, ZPoint zMouse)
	{
		int length = 500, height = 20;
		Screen screen = new Screen(position, new ZPoint(length, height));

		screen.Fill(MyMath.DarkDarkGray);

		var query = from l in lObjects where l.isActive orderby l.rInitiative.x select l;
		float zeroInitiative = -query.Last().rInitiative.x;

		foreach (LObject l in query)
		{
			int rInitiative = (int)(100.0f * (-l.rInitiative.x - zeroInitiative)) + 1;
			int y = -32, z = -32;
			if (l.isInParty) { y = height; z = 0; }

			Color color = Color.White;
			if (l.position.TheSameAs(zMouse)) color = l.RelationshipColor;

			if (scaleAnimations.Count == 0 || scaleAnimations.Peek().rPoint != l.rInitiative)
				screen.DrawRectangle(new ZPoint(rInitiative, z), new ZPoint(1, height + 32), color);

			screen.Draw(l.texture, new ZPoint(rInitiative + 1, y));
		}
	}

	public void Draw(Vector2 mouse)
	{
		RPoint.Update(scaleAnimations);

		MainScreen M = MainScreen.Instance;
		if (MyGame.Instance.gameState != MyGame.GameState.Local) return;

		for (int j = 0; j < Size.y; j++) for (int i = 0; i < Size.x; i++)
		{
			ZPoint p = new ZPoint(i, j);
			M.spriteBatch.Draw(this[p].texture, GraphicCoordinates(p));
		}

		var query = from l in lObjects orderby l.isActive select l;
		foreach (LObject l in query) M.spriteBatch.Draw(l.texture, GraphicCoordinates(l.rPosition));

		M.spriteBatch.Draw(currentLObjectSymbol, GraphicCoordinates(currentLObject.rPosition) - new Vector2(0, 12));

		ZPoint zMouse = ZCoordinates(mouse);
		M.Draw(zSelectionTexture, GraphicCoordinates(zMouse));

		DrawScale(new ZPoint(100, 650), zMouse);
	}

	public LObject NextLObject
	{
		get
		{
			var query = from l in lObjects where l.isActive orderby -l.initiative select l;
			if (query.Count() != 0) return query.First();
			else return null;
		}
	}
}

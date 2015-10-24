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
	private Collection<LObject> lObjects = new Collection<LObject>();

	public LObject currentLObject;
	private GObject gObject;
	private Texture2D currentLObjectSymbol, zSelectionTexture;

	public Queue<RMove> scaleAnimations = new Queue<RMove>();

	private Player P { get { return World.Instance.player; } }

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

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
		currentLObjectSymbol = MainScreen.Instance.game.Content.Load<Texture2D>("other/currentObject");
		zSelectionTexture = MainScreen.Instance.game.Content.Load<Texture2D>("other/zSelection");
	}

	private bool InRange(ZPoint p)
	{
		return p.InBoundaries(new ZPoint(0, 0), Size - new ZPoint(1, 1));
	}

	public LObject GetLObject(ZPoint p)
	{
		foreach (LObject l in lObjects) if (l.position.TheSameAs(p)) return l;
		return null;
	}

	public Creature GetCreature(ZPoint p)
	{
		var query = from l in lObjects where l is Creature && l.position.TheSameAs(p) && l.isActive select l;
		if (query.Count() > 0) return query.First() as Creature;
		else return null;
	}

	public Creature CurrentCreature { get { return currentLObject as Creature; } }

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
			lObjects.Add(item);
		}
		else if (pc is PartyCharacter)
		{
			Character item = new Character(pc as PartyCharacter, isInParty, isAIControlled);
			lObjects.Add(item);
		}
		else if (pc is PartyCreep)
		{
			Creep item = new Creep(pc as PartyCreep, isInParty, isAIControlled);
			lObjects.Add(item);
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

		lObjects.Clear();
		foreach (PartyCreature member in World.Instance.player.party) AddCreature(member, true, false);
		foreach (PartyCreature member in g.party) AddCreature(member, false, true);

		LObject item = new LObject("Tree");
		lObjects.Add(item);

		foreach (LObject l in lObjects)
		{
			l.SetPosition(RandomFreeTile(), 60.0f, false);
			if (l is LPlayer) l.SetInitiative(0.1f, 60.0f);
			else l.SetInitiative(-World.Instance.random.Next(100) / 100.0f, 60.0f);
		}

		if(NextLObject is Creature) { if ((NextLObject as Creature).isAIControlled) NextLObject.Run(); }
		else NextLObject.Run();

		currentLObject = NextLObject;
		MyGame.Instance.gameState = MyGame.GameState.Local;
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

		foreach (Creature c in query)
		{
			int rInitiative = (int)(100.0f * (-c.rInitiative.x - zeroInitiative)) + 1;
			int y = -32, z = -32;
			if (c.isInParty) { y = height; z = 0; }

			Color color = Color.White;
			if (c.position.TheSameAs(zMouse)) color = c.RelationshipColor;

			if (scaleAnimations.Count == 0 || scaleAnimations.Peek().rPoint != c.rInitiative)
				screen.DrawRectangle(new ZPoint(rInitiative, z), new ZPoint(1, height + 32), color);

			screen.Draw(c.texture, new ZPoint(rInitiative + 1, y));
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

	public void CheckForEvents()
	{
		var aliveMonsters = from c in lObjects where c is Creature && c.isActive && !(c as Creature).isInParty select c;
		if (aliveMonsters.Count() == 0)
		{
			P.party.Clear();
			var aliveParty = from c in lObjects where c is Creature && c.isActive && (c as Creature).isInParty orderby c.Importance select c;

			foreach (Creature c in aliveParty)
			{
				c.partyCreature.hp = c.HP;
				P.party.Add(c.partyCreature);
			}

			gObject.Kill();
			MyGame.Instance.gameState = MyGame.GameState.Global;
		}
	}
}

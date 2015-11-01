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

		//party.Add(new PartyCreep("Krokar"));
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
		//if (!MyGame.Instance.battle) DrawParty(new ZPoint(1100, 50));
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
}

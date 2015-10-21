using Microsoft.Xna.Framework.Graphics;

class Player : GObject
{
	public Texture2D textureHidden;

	public bool FOVEnabled = true;
	public bool[,] visitedLocations;

	public int partySize;

	private MainScreen M { get { return MainScreen.Instance; } }

	public Player()
	{
		name = "player";
		speed = 1.0f;
		partySize = 0;
	}

	public override void LoadTexture()
	{
		texture = M.game.Content.Load<Texture2D>("other/player");
		textureHidden = M.game.Content.Load<Texture2D>("other/playerHidden");
	}

	public override void Draw()
	{
		rPosition.Update();
		if (W.map[position].type.name != BigBase.Instance.gTileTypes.SafeName("forest"))
			M.spriteBatch.Draw(texture, M.GraphicCoordinates(rPosition));
		else M.spriteBatch.Draw(textureHidden, M.GraphicCoordinates(rPosition));
	}

	public override void Kill()
	{
		Log.WriteLine("Game Over!");
		MainScreen.Instance.game.Exit();
	}

	private void StartDialog(string name, GObject g) { M.dialogScreen.StartDialog(name, g);	}

	public override void ProcessCollisions(GObject g)
	{
		if (g.name == "Morlocks") StartDialog("Morlocks Encounter", g);
		else if (g.name == "Wild Dogs") StartDialog("Wild Dogs Encounter", g);
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

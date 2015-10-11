using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Player : GObject
{
	public Texture2D textureHidden;

	public bool FOVEnabled = true;
	public bool[,] visitedLocations;

	public int partySize;

	public Player()
	{
		name = "player";
		speed = 1.0f;
		partySize = 0;
	}

	public override void LoadTexture(Game game)
	{
		texture = game.Content.Load<Texture2D>("gPlayer");
		textureHidden = game.Content.Load<Texture2D>("gPlayerHidden");
	}

	public override void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		if (World.Instance.map[position].type != "forest") spriteBatch.Draw(texture, mainScreen.GraphicCoordinates(position));
		else spriteBatch.Draw(textureHidden, mainScreen.GraphicCoordinates(position));
	}

	public override void Kill()
	{
		Log.WriteLine("Game Over!");
	}

	public override void ProcessCollisions(GObject g)
	{
		if (g.name == "Neutral")
		{
			partySize++;
			g.Kill();
		}
		else if (g.name == "Monster" && partySize > 0)
		{
			partySize--;
			g.Kill();
		}
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
			if (World.Instance.map.InRange(p)) return visitedLocations[p.x, p.y];
			else return false;
		}
		set
		{
			if (World.Instance.map.InRange(p)) visitedLocations[p.x, p.y] = value;
		}
	}

	public void UpdateVisitedLocations()
	{
		if (visitedLocations == null)
		{
			visitedLocations = new bool[World.Instance.map.Size.x, World.Instance.map.Size.y];
			for (int j = 0; j < World.Instance.map.Size.y; j++)
				for (int i = 0; i < World.Instance.map.Size.x; i++)
					visitedLocations[i, j] = false;
		}

		for (int j=-2; j <= 2; j++) for (int i=-2; i <=2; i++)
		{
				ZPoint p = position + new ZPoint(i, j);
				if (World.Instance.map.IsInView(position, p)) this[p] = true;
		}
	}
}

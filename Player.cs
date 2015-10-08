using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Player
{
	public HexPoint position = new HexPoint();
	public Texture2D texture, textureHidden;

	public bool FOVEnabled = true;
	public bool[,] visitedLocations;

	public void LoadTexture(Game game)
	{
		texture = game.Content.Load<Texture2D>("player");
		textureHidden = game.Content.Load<Texture2D>("playerHidden");
	}

	public void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		if (World.Instance.map[position].type != "forest") spriteBatch.Draw(texture, mainScreen.GraphicCoordinates(position));
		else spriteBatch.Draw(textureHidden, mainScreen.GraphicCoordinates(position));
	}

	public void Move(HexPoint.HexDirection d)
	{
		ZPoint destination = position.Shift(d);
		if (World.Instance.map.IsWalkable(destination))
		{
			position = destination;
			UpdateVisitedLocations();
		}
	}

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

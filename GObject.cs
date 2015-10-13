using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GObject
{
	public string name;
	public HexPoint position;
	public float initiative = -5.0f, speed = 3.0f;

	public Texture2D texture;

	public GObject() { name = ""; position = new HexPoint(); }
	public GObject(string namei, HexPoint p) { name = namei; position = p; }

	public virtual void LoadTexture(Game game)
	{
		texture = game.Content.Load<Texture2D>("g" + name);
	}

	public virtual void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		if (!World.Instance.player.FOVEnabled || World.Instance.map.IsInView(World.Instance.player.position, position))
			spriteBatch.Draw(texture, mainScreen.GraphicCoordinates(position));
	}

	public virtual void Kill()
	{
		World.Instance.gObjects.Remove(this);
	}

	public virtual void ProcessCollisions(GObject g)
	{
		if (MyMath.SamePairs("Morlocks", "Wild Dogs", name, g.name))
		{
			Kill();
			g.Kill();
		}
	}

	public static void ProcessCollisions(Collection<GObject> c)
	{
		if (c.Count == 2) c[0].ProcessCollisions(c[1]);
		else if (c.Count > 2) Log.WriteLine("That is an interesting development!");
	}

	public virtual void Move(HexPoint.HexDirection d)
	{
		ZPoint destination = position.Shift(d);
		if (World.Instance.map.IsWalkable(destination)) position = destination;

		ProcessCollisions(World.Instance[position]);

		PassTurn(speed);
	}

	public virtual void Run()
	{
		Move(HexPoint.GetDirection(World.Instance.random.Next(6)));
	}

	public void PassTurn(float time)
	{
		initiative -= time;
		World.Instance.NextGObject.Run();
	}
}
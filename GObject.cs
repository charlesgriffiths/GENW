using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GObject
{
	public HexPoint position = new HexPoint();
	public RPoint rPosition = new RPoint();

	public string name;
	public float initiative = -5.0f, speed = 3.0f;
	public Texture2D texture;

	protected World W { get { return World.Instance; } }

	public GObject() { name = ""; position = new HexPoint(); }
	public GObject(string namei, HexPoint p)
	{
		name = namei;
		SetPosition (p, 100.0f);
	}

	public virtual void LoadTexture(Game game)
	{
		texture = game.Content.Load<Texture2D>("g" + name);
	}

	public virtual void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		rPosition.Update();
		if (!W.player.FOVEnabled || W.map.IsInView(W.player.position, position))
			spriteBatch.Draw(texture, mainScreen.GraphicCoordinates(rPosition));
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
		if (W.map.IsWalkable(destination)) SetPosition(destination, 2.0f);

		ProcessCollisions(W[position]);

		PassTurn(speed);
	}

	public virtual void Run()
	{
		Move(HexPoint.GetDirection(W.random.Next(6)));
	}

	public void PassTurn(float time)
	{
		initiative -= time;
		W.NextGObject.Run();
	}

	public void SetPosition(HexPoint p, float speed)
	{
		rPosition.Add((ZPoint)position, (ZPoint)p, speed);
		position = p;
	}
}
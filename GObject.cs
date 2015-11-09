using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

partial class GObject
{
	protected GObjectShape shape;
	public List<Creature> party = new List<Creature>();
	public Inventory inventory = new Inventory(24);

	public HexPoint position = new HexPoint();
	public RPoint rPosition = new RPoint();
	public AnimationQueue movementAnimations = new AnimationQueue();

	public string uniqueName;
	public float initiative;
	public Dialog dialog;

	public string Name { get { return shape.name; } }
	public float Speed { get { return shape.speed; } }
	public bool IsActive { get { return shape.isActive; } }

	public Texture2D Texture
	{
		get { return shape.texture; }
		set { shape.texture = value; }
	}

	protected World W { get { return World.Instance; } }
	protected MainScreen M { get { return MainScreen.Instance; } }

	protected GObject() {}

	private bool IsVisible()
	{
		if (!MyGame.Instance.FOVEnabled) return true;
		else if (IsActive) return W.map.IsInView(W.player.position, position);
		else return W.player[position];
    }

	public void DrawAnnotation()
	{
		if (IsVisible() && uniqueName != "")
		{
			Vector2 offset = M.smallFont.MeasureString(uniqueName);
			Vector2 v = M.GraphicCoordinates(rPosition) + new Vector2(24 - offset.X / 2, 48);
			M.DrawRectangle(new ZPoint(v), new ZPoint(offset), new Color(0.0f, 0.0f, 0.0f, 0.8f));
			M.DrawString(M.smallFont, uniqueName, new ZPoint(v), Color.White);
		}
	}

	public virtual void Draw()
	{
		movementAnimations.Draw();
		if (IsVisible()) M.Draw(Texture, M.GraphicCoordinates(rPosition));
	}

	public virtual void Kill()
	{
		World.Instance.gObjects.Remove(this);
	}

	public virtual void ProcessCollisions(GObject g)
	{
		//if (MyMath.SamePairs("Morlocks", "Wild Dogs", name, g.name)) { Kill(); g.Kill(); }
	}

	public static void ProcessCollisions(List<GObject> c)
	{
		if (c.Count == 2) c[0].ProcessCollisions(c[1]);
		else if (c.Count > 2) Log.WriteLine("That is an interesting development!");
	}

	public static void CheckForEvents()
	{
		var query = from pc in World.Instance.player.party where pc.uniqueName == "Boo-Boo" select pc;
		if (query.Count() == 0 && World.Instance.random.Next(300) == 0) MainScreen.Instance.dialogScreen.StartDialog("Boo-Boo Died");
	}

	public virtual void Move(HexPoint.HexDirection d)
	{
		ZPoint destination = position.Shift(d);
		float travelTime = Speed * W.map[destination].type.travelTime;
        if (W.map.IsWalkable(destination)) SetPosition(destination, 4.0f / travelTime);
		PassTurn(travelTime);
	}

	public virtual void Run()
	{
		Move(HexPoint.GetDirection(W.random.Next(6)));
	}

	public void PassTurn(float time)
	{
		CheckForEvents();
		ProcessCollisions(W[position]);

		initiative -= time;
		if (!MyGame.Instance.dialog) W.NextGObject.Run();
	}

	public void SetPosition(HexPoint p, float speed)
	{
		movementAnimations.Add(new RMove(rPosition, (ZPoint)p - position, speed));
		position = p;
	}
}
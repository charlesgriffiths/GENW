using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GObjectShape : NamedObject
{
	private string textureName;
	public Texture2D texture;
	public float speed;
	public bool isActive;
	public Dialog dialog;

	public Dictionary<string, int> partyShape = new Dictionary<string, int>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		textureName = MyXml.GetString(xnode, "icon");
		if (textureName == "") textureName = name;
		isActive = MyXml.GetBool(xnode, "active");

		if (isActive) speed = MyXml.GetFloat(xnode, "speed");
		else speed = 1.0f;

		string dialogName = MyXml.GetString(xnode, "dialog");
		if (dialogName != "") dialog = BigBase.Instance.dialogs.Get(dialogName);

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
			partyShape.Add(MyXml.GetString(xnode, "name"), MyXml.GetInt(xnode, "quantity"));
	}

	public static void LoadTextures()
	{
		foreach (GObjectShape s in BigBase.Instance.gShapes.data)
			s.texture = MainScreen.Instance.game.Content.Load<Texture2D>("global/" + s.textureName);
	}
}

class GObject
{
	public HexPoint position = new HexPoint();

	public RPoint rPosition = new RPoint();
	public AnimationQueue movementAnimations = new AnimationQueue();

	protected GObjectShape shape;

	public Collection<PartyCreature> party = new Collection<PartyCreature>();

	public string uniqueName;
	public float initiative;// = -5.0f;
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
	public GObject(GObjectShape shapei)
	{
		shape = shapei;
		dialog = shapei.dialog;

		foreach (KeyValuePair<string, int> pair in shape.partyShape)
		{
			for (int i = 0; i < pair.Value; i++)
			{
				PartyCreep item = new PartyCreep(pair.Key);
				party.Add(item);
			}
		}

		initiative = -0.1f;
	}

	private bool IsVisible()
	{
		if (!W.player.FOVEnabled) return true;
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

	public static void ProcessCollisions(Collection<GObject> c)
	{
		if (c.Count == 2) c[0].ProcessCollisions(c[1]);
		else if (c.Count > 2) Log.WriteLine("That is an interesting development!");
	}

	public static void CheckForEvents()
	{
		var query = from pc in World.Instance.player.party where pc.uniqueName == "Boo-Boo" select pc;
		if (query.Count() == 0 && World.Instance.random.Next(30) == 0) MainScreen.Instance.dialogScreen.StartDialog("Boo-Boo Died");
	}

	public virtual void Move(HexPoint.HexDirection d)
	{
		ZPoint destination = position.Shift(d);
		if (W.map.IsWalkable(destination)) SetPosition(destination, 4.0f);
		PassTurn(Speed);
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
		W.NextGObject.Run();
	}

	public void SetPosition(HexPoint p, float speed)
	{
		movementAnimations.Add(new RMove(rPosition, (ZPoint)p - position, speed));
		position = p;
	}
}
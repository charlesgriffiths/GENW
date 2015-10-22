using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class GObjectShape : NamedObject
{
	private string textureName;
	public Texture2D texture;
	public float speed;
	public bool isActive;

	public Dictionary<string, int> partyShape = new Dictionary<string, int>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		textureName = MyXml.GetString(xnode, "icon");
		if (textureName == "") textureName = name;
		isActive = MyXml.GetBool(xnode, "active");
		if (isActive) speed = MyXml.GetFloat(xnode, "speed");
		else speed = 1.0f;

		xnode = xnode.FirstChild;
		while (xnode != null)
		{
			partyShape.Add(MyXml.GetString(xnode, "name"), MyXml.GetInt(xnode, "quantity"));
			xnode = xnode.NextSibling;
		}
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

	protected GObjectShape shape;

	public Collection<PartyCreature> party = new Collection<PartyCreature>();

	//public string name;
	//public string uniqueName;
	public float initiative = -5.0f;// speed = 3.0f;
	//public Texture2D texture;

	public string Name { get { return shape.name; } }
	public float Speed { get { return shape.speed; } }
	public Texture2D Texture
	{
		get { return shape.texture; }
		set { shape.texture = value; }
	}

	protected World W { get { return World.Instance; } }

	//public GObject() { name = ""; position = new HexPoint(); }
	//public GObject(string namei, HexPoint p)
	//{
	//name = namei;
	//SetPosition (p, 100.0f);
	//}

	public GObject() {}
	public GObject(GObjectShape shapei)
	{
		shape = shapei;
		foreach (KeyValuePair<string, int> pair in shape.partyShape)
		{
			for (int i = 0; i < pair.Value; i++)
			{
				PartyCreep item = new PartyCreep(pair.Key);
				party.Add(item);
			}
		}
	}

	//public virtual void LoadTexture() // !!!
	//{
		//texture = MainScreen.Instance.game.Content.Load<Texture2D>("global/g" + name);
	//}

	public virtual void Draw()
	{
		rPosition.Update();
		if (!W.player.FOVEnabled || W.map.IsInView(W.player.position, position))
			MainScreen.Instance.spriteBatch.Draw(Texture, MainScreen.Instance.GraphicCoordinates(rPosition));
	}

	public virtual void Kill()
	{
		World.Instance.gObjects.Remove(this);
	}

	public virtual void ProcessCollisions(GObject g)
	{
		/*if (MyMath.SamePairs("Morlocks", "Wild Dogs", name, g.name))
		{
			Kill();
			g.Kill();
		}*/
	}

	public static void ProcessCollisions(Collection<GObject> c)
	{
		if (c.Count == 2) c[0].ProcessCollisions(c[1]);
		else if (c.Count > 2) Log.WriteLine("That is an interesting development!");
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
		ProcessCollisions(W[position]);
		initiative -= time;
		W.NextGObject.Run();
	}

	public void SetPosition(HexPoint p, float speed)
	{
		rPosition.Add((ZPoint)position, (ZPoint)p, speed);
		position = p;
	}
}
using System;
using System.Xml;
using System.Collections.Generic;

class World
{
	private static readonly World instance = new World();
	public static World Instance { get { return instance; } }
	private World() {}
		
	public Map map = new Map();
	public Battlefield battlefield = new Battlefield();
	public Player player;
	public List<GObject> gObjects = new List<GObject>();

	public ZPoint viewRadius = new ZPoint(16, 8);
	public ZPoint camera = new ZPoint();
	public Random random = new Random();

	public void Load()
	{
		Log.Write("loading the world... ");
		XmlNode xnode = MyXml.SecondChild("Data/world.xml");
		Log.Assert(xnode.Name == "map", "wrong world file format");

		int width = MyXml.GetInt(xnode, "width");
		int height = MyXml.GetInt(xnode, "height");
		map.margin.x = MyXml.GetInt(xnode, "vMargin");
		map.margin.y = MyXml.GetInt(xnode, "hMargin");
		string method = MyXml.GetString(xnode, "method");

		map.Load(width, height, method);

		xnode = xnode.NextSibling;
		camera = new ZPoint(MyXml.GetInt(xnode, "x"), MyXml.GetInt(xnode, "y"));

		player = new Player();
		player.SetPosition(camera, 60.0f);
		player.UpdateVisitedLocations();

		for (xnode = xnode.NextSibling.FirstChild; xnode != null; xnode = xnode.NextSibling)
		{
			GObject item = new GObject(BigBase.Instance.gShapes.Get(MyXml.GetString(xnode, "name")));
			item.uniqueName = MyXml.GetString(xnode, "uniqueName");

			string dialogName = MyXml.GetString(xnode, "dialog");
			if (dialogName != "") item.dialog = BigBase.Instance.dialogs.Get(dialogName);

			item.SetPosition(new HexPoint(MyXml.GetInt(xnode, "x"), MyXml.GetInt(xnode, "y")), 60.0f);
			gObjects.Add(item);
		}

		Log.WriteLine("OK");
	}

	public void LoadTextures()
	{
		map.LoadTextures();
		player.LoadTextures();
		battlefield.LoadTextures();
	}

	public void Draw()
	{
		map.Draw(false);
		foreach (GObject g in gObjects) g.Draw();
		player.Draw();
		map.Draw(true);
		foreach (GObject g in gObjects) g.DrawAnnotation();
		map.DrawMask();

		if (MyGame.Instance.battle) battlefield.Draw();
		else player.DrawParty(new ZPoint(10, 10));
	}

	public GObject NextGObject
	{
		get
		{
			GObject result = player;
			foreach (GObject g in gObjects) if (g.initiative > result.initiative && g.IsActive) result = g;
			return result;
		}
	}

	public List<GObject> this[HexPoint p]
	{
		get
		{
			List<GObject> result = new List<GObject>();
			if (player.position.TheSameAs(p)) result.Add(player);
			foreach (GObject g in gObjects) if (g.position.TheSameAs(p)) result.Add(g);
			return result;
        }
	}
}

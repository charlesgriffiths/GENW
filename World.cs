using System;
using System.Xml;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class World
{
	private static readonly World instance = new World();
	public static World Instance { get { return instance; } }
	
	public Map map = new Map();
	public Battlefield battlefield = new Battlefield();
	public Player player = new Player();
	public Collection<GObject> gObjects = new Collection<GObject>();

	public ZPoint viewRadius = new ZPoint(16, 8);
	public ZPoint camera = new ZPoint();
	public Random random = new Random();

	public void Load()
	{
		Log.Write("loading the world... ");
		XmlNode xnode = MyXml.SecondChild("world.xml");
		Log.Assert(xnode.Name == "map", "wrong world file format");

		int width = MyXml.GetInt(xnode, "width");
		int height = MyXml.GetInt(xnode, "height");
		map.margin.x = MyXml.GetInt(xnode, "vMargin");
		map.margin.y = MyXml.GetInt(xnode, "hMargin");
		string method = MyXml.GetString(xnode, "method");

		map.Load(width, height, method);

		xnode = xnode.NextSibling;
		//player.position.x = MyXml.GetInt(xnode, "x");
		//player.position.y = MyXml.GetInt(xnode, "y");
		camera = new ZPoint(MyXml.GetInt(xnode, "x"), MyXml.GetInt(xnode, "y"));
		//player.SetPosition(new HexPoint(MyXml.GetInt(xnode, "x"), MyXml.GetInt(xnode, "y")), 100);
		//camera = new ZPoint(px, py);
		player.SetPosition(camera, 60.0f);
		player.UpdateVisitedLocations();
		Log.WriteLine("OK");

		gObjects.Add(new GObject("Morlocks", new HexPoint(13, 13)));
		gObjects.Add(new GObject("Morlocks", new HexPoint(20, 10)));
		gObjects.Add(new GObject("Morlocks", new HexPoint(5, 16)));
		gObjects.Add(new GObject("Morlocks", new HexPoint(15, 9)));
		gObjects.Add(new GObject("Morlocks", new HexPoint(20, 16)));

		gObjects.Add(new GObject("Wild Dogs", new HexPoint(16, 13)));
		gObjects.Add(new GObject("Wild Dogs", new HexPoint(16, 18)));
		gObjects.Add(new GObject("Wild Dogs", new HexPoint(24, 15)));
		gObjects.Add(new GObject("Wild Dogs", new HexPoint(19, 20)));
	}

	public void LoadTextures(Game game)
	{
		player.LoadTexture(game);
		foreach (GObject gObject in gObjects) gObject.LoadTexture(game);
		battlefield.LoadTextures(game);
	}

	public void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		map.Draw(spriteBatch);
		foreach (GObject g in gObjects) g.Draw(mainScreen, spriteBatch);
		player.Draw(mainScreen, spriteBatch);
		battlefield.Draw(spriteBatch);
	}

	public GObject NextGObject
	{
		get
		{
			GObject result = player;
			foreach (GObject g in gObjects) if (g.initiative > result.initiative) result = g;
			return result;
		}
	}

	public Collection<GObject> this[HexPoint p]
	{
		get
		{
			Collection<GObject> result = new Collection<GObject>();
			if (player.position.TheSameAs(p)) result.Add(player);
			foreach (GObject g in gObjects) if (g.position.TheSameAs(p)) result.Add(g);
			return result;
        }
	}
}

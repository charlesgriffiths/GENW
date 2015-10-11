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
	public Player player = new Player();
	public Collection<GObject> gObjects = new Collection<GObject>();

	public ZPoint viewRadius = new ZPoint(16, 8);
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
		player.position.x = MyXml.GetInt(xnode, "x");
		player.position.y = MyXml.GetInt(xnode, "y");
		player.UpdateVisitedLocations();
		Log.WriteLine("OK");

		gObjects.Add(new GObject("Neutral", new HexPoint(13, 13)));
		gObjects.Add(new GObject("Neutral", new HexPoint(20, 10)));
		gObjects.Add(new GObject("Neutral", new HexPoint(5, 16)));
		gObjects.Add(new GObject("Neutral", new HexPoint(15, 9)));

		gObjects.Add(new GObject("Monster", new HexPoint(16, 13)));
		gObjects.Add(new GObject("Monster", new HexPoint(16, 18)));
		gObjects.Add(new GObject("Monster", new HexPoint(24, 15)));
		gObjects.Add(new GObject("Monster", new HexPoint(19, 20)));
	}

	public void LoadTextures(Game game)
	{
		player.LoadTexture(game);
		foreach (GObject gObject in gObjects) gObject.LoadTexture(game);
	}

	public ZPoint Camera
	{
		get
		{
			ZPoint camera = player.position;
			ZPoint min = viewRadius;
			ZPoint max = map.Size - viewRadius - new ZPoint(1, 1);
			return camera.Boundaries(min, max);
		}
	}

	public void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		map.Draw(mainScreen, spriteBatch);
		foreach (GObject gObject in gObjects) gObject.Draw(mainScreen, spriteBatch);
		player.Draw(mainScreen, spriteBatch);
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

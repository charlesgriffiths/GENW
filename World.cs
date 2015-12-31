using System;
using System.Xml;
using System.Collections.Generic;

public class World
{
	private static readonly World instance = new World();
	public static World Instance { get { return instance; } }
	private World() {}
		
	public Map map = new Map();
	public Battlefield battlefield = new Battlefield();
	public Player player;
	public List<GlobalObject> gObjects = new List<GlobalObject>();

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
		player.SetPosition(camera, 0.01f);
		player.UpdateVisitedLocations();

		for (xnode = xnode.NextSibling.FirstChild; xnode != null; xnode = xnode.NextSibling)
		{
			GlobalObject o = new GlobalObject(BigBase.Instance.gShapes.Get(MyXml.GetString(xnode, "name")));
			o.uniqueName = MyXml.GetString(xnode, "uniqueName");

			string dialogName = MyXml.GetString(xnode, "dialog");
			if (dialogName != "") o.dialog = BigBase.Instance.dialogs.Get(dialogName);

			o.SetPosition(new HexPoint(MyXml.GetInt(xnode, "x"), MyXml.GetInt(xnode, "y")), 0.01f);

			for (XmlNode xitem = xnode.FirstChild; xitem != null; xitem = xitem.NextSibling)
				o.inventory.Add(new Item(MyXml.GetString(xitem, "name"), MyXml.GetInt(xitem, "amount", 1)));

			gObjects.Add(o);
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
		foreach (GlobalObject g in gObjects) g.Draw();
		player.Draw();
		map.Draw(true);
		foreach (GlobalObject g in gObjects) g.DrawAnnotation();
		map.DrawMask();

		if (MyGame.Instance.battle) battlefield.Draw();
		else player.DrawParty(new ZPoint(8, 8));
	}

	public GlobalObject NextGObject
	{
		get
		{
			GlobalObject result = player;
			foreach (GlobalObject g in gObjects) if (g.initiative > result.initiative && g.IsActive) result = g;
			return result;
		}
	}

	public List<GlobalObject> this[HexPoint p]
	{
		get
		{
			List<GlobalObject> result = new List<GlobalObject>();
			if (player.position.TheSameAs(p)) result.Add(player);
			foreach (GlobalObject g in gObjects) if (g.position.TheSameAs(p)) result.Add(g);
			return result;
        }
	}
}

using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class World
{
	private static readonly World instance = new World();
	public static World Instance { get { return instance; } }
	
	public Map map = new Map();
	public Player player = new Player();

	public ZPoint viewRadius = new ZPoint(16, 8);

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
		Log.WriteLine("OK");
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
		player.Draw(mainScreen, spriteBatch);
	}
}

using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class GlobalMap
{
	private char[,] data;

	public ZPoint Size { get { return new ZPoint(data.GetUpperBound(0) + 1, data.GetUpperBound(1) + 1); } }

	public GlobalTile this[int i, int j]
	{
		get
		{
			return BigBase.Instance.globalTileBase.Get(data[i, j].ToString());
		}
	}

	public void Load()
	{
		Log.Write("loading global map... ");
		XmlNode xnode = MyXml.FirstChild("globalMap.xml");
		XmlElement xl = (XmlElement)xnode;
		Log.Assert (xl.Name == "map", "wrong map format");

		int width = int.Parse(xl.GetAttribute("width"));
		int height = int.Parse(xl.GetAttribute("height"));
		data = new char[width, height];

		string text = xnode.InnerText;
		text = text.Replace('\n', ' ');
		text = text.Replace('\r', ' ');
		text = text.Replace(" ", "");
		Log.Assert (text.Length == width * height, "wrong map data");

		for (int j = 0; j < height; j++)
		{
			for (int i = 0; i < width; i++) data[i,j] = text[i + j*width];
		}

		Log.WriteLine("OK");
	}

	public void Draw(SpriteBatch spriteBatch, ZPoint camera)
	{
		ZPoint min = ZPoint.Max(camera - Screen.Instance.viewRadius, ZPoint.Zero);
		ZPoint max = ZPoint.Min(camera + Screen.Instance.viewRadius + new ZPoint(1, 1), Size);

		for (int i = min.x; i < max.x; i++)
			for (int j = min.y; j < max.y; j++)
			{
                spriteBatch.Draw(this[i, j].texture, Screen.Instance.GraphicCoordinates(new ZPoint(i, j), camera));
			}
	}
}

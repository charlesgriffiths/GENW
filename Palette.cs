using System.Xml;
using System.Linq;
using System.Collections.Generic;

public class Palette : NamedObject
{
	public Dictionary<char, LTile> data = new Dictionary<char, LTile>();

	public LTile this[char code] { get { return data[code]; } }
	public int Size { get { return data.Count; } }

	public char GetKey(int i) { return data.Keys.ToList()[i]; }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");

		XmlNode node = xnode.FirstChild;
		while (node != null)
		{
			char code = MyXml.GetChar(node, "code");
			LTile item = BigBase.Instance.lTiles.Get(MyXml.GetString(node, "tile"));

			data.Add(code, item);
			node = node.NextSibling;
		}
	}
}

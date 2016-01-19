using System.Xml;
using System.Linq;
using System.Collections.Generic;

public class Palette : NamedObject
{
	public Dictionary<char, LocalTile> data = new Dictionary<char, LocalTile>();

	public LocalTile this[char code] { get { return data[code]; } }
	public int Size { get { return data.Count; } }

	public char GetKey(int i) { return data.Keys.ToList()[i]; }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");

		XmlNode node = xnode.FirstChild;
		while (node != null)
		{
			char code = MyXml.GetChar(node, "code");
			LocalTile item = BigBase.Instance.localTiles.Get(MyXml.GetString(node, "tile"));

			data.Add(code, item);
			node = node.NextSibling;
		}
	}

	public static Palette Get(string name) { return BB.palettes.Get(name); }
	public static IEnumerable<Palette> Get(params string[] names) { return BB.palettes.data.Where(p => names.Contains(p.name)); }

	public Palette Next
	{
		get
		{
			var s = BB.palettes.data;
			return this == s.Last() ? s.First() : s[s.IndexOf(this) + 1];
		}
	}
}

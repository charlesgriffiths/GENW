using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class Ability : NamedObject
{
	public Texture2D texture;
	public string description;
	public TargetType targetType;
	public int cost;

	public enum TargetType { Passive, None, Direction, Point, Object, Creature };

	private static TargetType GetTargetType(string s)
	{
		if (s == "passive") return TargetType.Passive;
		else if (s == "none") return TargetType.None;
		else if (s == "direction") return TargetType.Direction;
		else if (s == "point") return TargetType.Point;
		else if (s == "object") return TargetType.Object;
		else if (s == "creature") return TargetType.Creature;
		else
		{
			Log.Error("unknown ability target type " + s);
			return TargetType.Passive;
		}
	}

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		targetType = GetTargetType(MyXml.GetString(xnode, "target"));
		cost = MyXml.GetInt(xnode, "cost");
		description = MyXml.GetString(xnode, "description");
	}

	public static void LoadTextures()
	{
		foreach (Ability a in BigBase.Instance.abilities.data)
			a.texture = MainScreen.Instance.game.Content.Load<Texture2D>("abilities/" + a.name);
	}
}

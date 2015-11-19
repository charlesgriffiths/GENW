using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public abstract class LObject
{
	public ZPoint position = new ZPoint();
	
	public RPoint rInitiative = new RPoint();
	public RPoint rPosition = new RPoint();

	public AnimationQueue movementAnimations = new AnimationQueue();
	public AnimationQueue scaleAnimations = new AnimationQueue();

	public float initiative;
	public Texture2D texture;
	public float scaling = 1.0f;

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }

	public virtual string Name { get { return ""; } }
	public virtual bool IsWalkable { get { return true; } }

	protected virtual void Init()
	{
		initiative = 0.0f;
	}

	public virtual int Importance { get { return 4; } }

	public void SetPosition(ZPoint p, float speed, bool commonQueue)
	{
		RMove rMove = new RMove(rPosition, p - position, speed);
		if (commonQueue) B.combatAnimations.Add(rMove);
		else movementAnimations.Add(rMove);
		position = p;
	}

	public void SetInitiative(float initiativei, float speed, bool commonQueue)
	{
		RMove rMove = new RMove(rInitiative, new Vector2(initiativei - initiative, 0), speed);
		if (commonQueue) B.scaleAnimations.Add(rMove);
		else scaleAnimations.Add(rMove);
		initiative = initiativei;
	}

	public virtual void Kill() {}

	public virtual Color RelationshipColor { get { return Color.Blue; }	}
	public virtual void Run() {}

	protected void ContinueTurn(float time)
	{
		Log.Assert(time > 0, "time <= 0");
		SetInitiative(initiative - time, 2.0f / time, true);
		B.CheckForEvents();
	}

	protected virtual void PassTurn(float time)
	{
		ContinueTurn(time);

		LObject nextLObject = B.NextLObject;
		if (nextLObject != null) nextLObject.Run();
	}

	public bool IsAdjacentTo(LObject o) { return position.IsAdjacentTo(o.position); }

	public bool IsAdjacentTo(List<ZPoint> zone)
	{
		var query = from p in zone where p.IsAdjacentTo(position) select p;
		return query.Count() > 0;
	}

	public int Distance(LObject o) { return MyMath.ManhattanDistance(position, o.position); }
	public int Distance(ZPoint p) { return MyMath.ManhattanDistance(position, p); }
}

public class PureShape : NamedObject
{
	public Texture texture;
	public bool isWalkable, isFlat;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		isWalkable = MyXml.GetBool(xnode, "walkable");
		isFlat = MyXml.GetBool(xnode, "flat");

		texture = BigBase.Instance.textures.Get(name);
	}
}

public class PureLObject : LObject
{
	public PureShape data;

	public override bool IsWalkable { get { return data.isWalkable; } }
	public override string Name { get {	return data.texture.name; }	}

	public PureLObject(string name)
	{
		data = BigBase.Instance.pureShapes.Get(name);
		texture = data.texture.Random();
		Init();
	}
}
using System.Xml;
using System.Linq;
using System.Collections.Generic;
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
	public virtual bool IsFlat { get { return true; } }
	public virtual bool IsVisible { get { return true; } }
	protected ZPoint GraphicPosition { get { return new ZPoint(Battlefield.GC(rPosition)); } } // вот это нужно почаще применять!

	public virtual int Importance { get { return 4; } }

	public void SetPosition(ZPoint p, float gameTime, bool commonQueue)
	{
		RMove rMove = new RMove(rPosition, p - position, gameTime);
		if (commonQueue) B.combatAnimations.Add(rMove);
		else movementAnimations.Add(rMove);
		position = p;
	}

	public void SetInitiative(float initiativei, float gameTime, bool commonQueue)
	{
		RMove rMove = new RMove(rInitiative, new Vector2(initiativei - initiative, 0), gameTime);
		if (commonQueue) B.scaleAnimations.Add(rMove);
		else scaleAnimations.Add(rMove);
		initiative = initiativei;
	}

	protected void AddInitiative(float value, float gameTime, bool commonQueue, bool animate)
	{
		if (animate) B.combatAnimations.Add(new TextAnimation(value.ToString(), null, M.fonts.verdanaBold, Color.Blue, 
			Battlefield.GC(position), 1, true));
		SetInitiative(initiative + value, gameTime, commonQueue);
	}

	public virtual void Kill() {}

	public virtual Color RelationshipColor { get { return Color.Blue; }	}
	public virtual void Run() {}

	protected void ContinueTurn(float time)
	{
		Log.Assert(time > 0, "time <= 0");
		SetInitiative(initiative - time, time, true);
	}

	protected virtual void PassTurn(float time)
	{
		ContinueTurn(time);

		LObject nextLObject = B.NextLObject;
		B.spotlightObject = nextLObject;
		if (nextLObject != null) nextLObject.Run();
	}

	public bool IsAdjacentTo(LObject o) { return position.IsAdjacentTo(o.position); }
	public bool IsAdjacentTo(List<ZPoint> zone)	{ return zone.Where(p => p.IsAdjacentTo(position)).Count() > 0; }
	public bool IsReachableFrom(List<ZPoint> zone) { return (zone.Where(p => B.IsReachable(p, position, B.CurrentLCreature.Range))).Count() > 0; }

	public int Distance(LObject o) { return MyMath.ManhattanDistance(position, o.position); }
	public int Distance(ZPoint p) { return MyMath.ManhattanDistance(position, p); }

	public virtual void Draw() { B.Draw(texture, rPosition, scaling, IsVisible ? Color.White : new Color(0.5f, 0.5f, 0.5f, 0.5f)); }
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
	public override bool IsFlat { get { return data.isFlat; } }
	public override string Name { get {	return data.texture.name; }	}

	public PureLObject() { Log.Error("this PureLObject constructor should not be called"); }
	public PureLObject(string name)
	{
		data = BigBase.Instance.pureShapes.Get(name);
		texture = data.texture.Random();
		//Init();
	}
}

public class LContainer : PureLObject
{
	public List<Item> items;

	public LContainer(Inventory inventory) : base("Chest")
	{
		items = inventory.Items;
	}
}

public class LItem : LObject
{
	public Item data;

	public override string Name { get { return data.data.name; } }

	public LItem(Item item)
	{
		data = item;
		texture = data.data.texture;
	}

	public LItem(ItemShape shape) : this(new Item(shape)) { }
	public LItem(string name) : this(ItemShape.Get(name)) { }
}
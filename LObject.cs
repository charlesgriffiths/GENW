using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class LObject
{
	public ZPoint position = new ZPoint();
	
	public RPoint rInitiative = new RPoint();
	public RPoint rPosition = new RPoint();

	public AnimationQueue movementAnimations = new AnimationQueue();
	public AnimationQueue scaleAnimations = new AnimationQueue();

	//public bool isActive;
	public float initiative;
	public Texture2D texture;

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }

	public LObject() {}
	public LObject(string namei)
	{
		//isActive = false;
		texture = BigBase.Instance.textures.Get(namei).data;

		Init();
	}

	public virtual string Name { get { return ""; } }

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
}
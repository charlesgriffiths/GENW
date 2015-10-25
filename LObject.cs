using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class LObject
{
	public ZPoint position = new ZPoint();
	
	public RPoint rInitiative = new RPoint();
	public RPoint rPosition = new RPoint();
	public AnimationQueue movementAnimations = new AnimationQueue();

	public bool isActive;
	public float initiative;
	
	public Texture2D texture;
	public string name;

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }

	public LObject() {}
	public LObject(string namei)
	{
		name = namei;
		isActive = false;
		texture = BigBase.Instance.textures.Get(name).data;

		Init();
	}

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

	public void SetInitiative(float initiativei, float speed)
	{
		B.scaleAnimationsTest.Add(new RMove(rInitiative, new Vector2(initiativei - initiative, 0), speed));
		initiative = initiativei;
	}

	public virtual void Kill()
	{
		isActive = false;
	}

	public virtual Color RelationshipColor { get { return Color.Blue; }	}
	public virtual void Run() {}

	protected void ContinueTurn(float time)
	{
		Log.Assert(time > 0, "time <= 0");
		SetInitiative(initiative - time, 1.0f / time);
		B.CheckForEvents();
	}

	protected virtual void PassTurn(float time)
	{
		ContinueTurn(time);

		LObject nextLObject = B.NextLObject;
		if (nextLObject != null) nextLObject.Run();
	}
}
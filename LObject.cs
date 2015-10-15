using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class LObject
{
	public ZPoint position = new ZPoint();
	public RPoint rPosition = new RPoint();
	public RPoint rInitiative = new RPoint();

	public float initiative;
	public int HP;
	private int controlMovementCounter;

	public CreatureShape shape;
	public Texture2D texture;
	public bool isInParty, isAIControlled, isActive;

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }

	public LObject() {}
	public LObject(string shapei, bool isInPartyi, bool isAIControlledi)
	{
		shape = BigBase.Instance.creatureShapes.Get(shapei);
		isInParty = isInPartyi;
		isAIControlled = isAIControlledi;
		Init();
	}

	protected void Init()
	{
		texture = shape.texture;
		initiative = 0.0f;
		HP = shape.maxHP;
		controlMovementCounter = 0;
		isActive = true;

		M.rPoints.Add(rPosition);
		M.rPoints.Add(rInitiative);
	}

	public void SetPosition(ZPoint p, float speed, bool commonQueue)
	{
		if (commonQueue) rPosition.Add(position, p, speed, M.rMoves);
		else rPosition.Add(position, p, speed);
		position = p;
	}

	public void SetInitiative(float initiativei, float speed)
	{
		//rInitiative.Add(new ZPoint((int)(100 * initiative), 0), new ZPoint((int)(100 * initiativei), 0), speed, false);
		rInitiative.Add(new Vector2(initiative, 0), new Vector2(initiativei, 0), speed, B.scaleAnimations);
		initiative = initiativei;
	}

	public virtual void Kill()
	{
		isActive = false;
		texture = M.game.Content.Load<Texture2D>("oBlood");
	}

	private void AnimateFailedMovement(ZPoint.Direction d)
	{
		Vector2 v = 0.25f * (Vector2)(ZPoint.Zero.Shift(d));
		rPosition.Add(v, 0.5f, M.rMoves);
		rPosition.Add(-v, 0.5f, M.rMoves);
	}

	private void AnimateAttack(ZPoint p)
	{
		Vector2 v = p - position;
		v.Normalize();
		v *= 0.5f;

		rPosition.Add(v, 0.5f, M.rMoves);
		rPosition.Add(-v, 0.5f, M.rMoves);
	}

	public void Attack(LObject l)
	{
		l.HP -= shape.damage;
		if (l.HP <= 0) l.Kill();

		AnimateAttack(l.position);
		PassTurn(shape.attackSpeed);
	}

	public void TryToMove(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		LObject l = B.GetLObject(destination);

		if (l != null) Attack(l);
		else if (B.IsWalkable(destination)) Move(d, control);
	}

	public void Move(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		if (B.IsWalkable(destination)) SetPosition(destination, 1.0f, true);
		else AnimateFailedMovement(d);

		if (control == true && controlMovementCounter < 3)
		{
			controlMovementCounter++;
			ContinueTurn(shape.movementSpeed);
		}
		else
		{
			controlMovementCounter = 0;
			PassTurn(shape.movementSpeed);
		}
	}

	public void Run()
	{
		if (!isAIControlled)
		{
			B.currentLObject = B.NextLObject;
			return;
		}

		Move(ZPoint.GetDirection(World.Instance.random.Next(4)), false);
	}

	private void ContinueTurn(float time)
	{
		//float initiativeSpeed = 1.0f;
		//if (this == B.currentLObject) initiativeSpeed = 60.0f;

		SetInitiative(initiative - time, 1.0f);
		B.CheckForEvents();
	}

	private void PassTurn(float time)
	{
		ContinueTurn(time);
		//B.previousLObject = this;
		B.NextLObject.Run();
	}
}

class LPlayer : LObject
{
	public LPlayer()
	{
		shape = BigBase.Instance.creatureShapes.Get("Human");
		isInParty = true;
		isAIControlled = false;
		Init();
	}
    public override void Kill() { World.Instance.player.Kill(); }
}
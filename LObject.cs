using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class CreatureShape : NamedObject
{
	public Texture2D texture;
	public int maxHP, damage;
	public float movementSpeed, attackSpeed;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		maxHP = MyXml.GetInt(xnode, "maxHP");
		damage = MyXml.GetInt(xnode, "damage");
		movementSpeed = MyXml.GetFloat(xnode, "movementSpeed");
		attackSpeed = MyXml.GetFloat(xnode, "attackSpeed");
	}

	public static void LoadTextures()
	{
		foreach (CreatureShape s in BigBase.Instance.creatureShapes.data)
			s.texture = BigBase.Instance.game.Content.Load<Texture2D>("l" + s.name);
	}
}

class LObject
{
	public ZPoint position = new ZPoint();
	public RPoint rPosition = new RPoint();

	public float initiative;
	public int HP;
	private int controlMovementCounter;

	public CreatureShape shape;
	public Texture2D texture;
	public bool isInParty, isAIControlled, isActive;

	private Battlefield B { get { return World.Instance.battlefield; } }

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
	}

	public void SetPosition(ZPoint p, float speed)
	{
		rPosition.Add(position, p, speed);
		position = p;
	}

	public virtual void Kill()
	{
		isActive = false;
		texture = BigBase.Instance.game.Content.Load<Texture2D>("oBlood");
	}

	private void AnimateAttack(ZPoint p)
	{
		Vector2 v = p - position;
		v.Normalize();
		v *= 0.5f;

		rPosition.Add(position, position + v, 5.0f);
		rPosition.Add(position + v, position, 5.0f);
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
		if (B.IsWalkable(destination)) SetPosition(destination, 5.0f);

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

		TryToMove(ZPoint.GetDirection(World.Instance.random.Next(4)), false);
	}

	private void ContinueTurn(float time)
	{
		initiative -= time;
		B.CheckForEvents();
	}

	private void PassTurn(float time)
	{
		ContinueTurn(time);
		LObject nextLObject = B.NextLObject;
        if (nextLObject != null) nextLObject.Run();
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
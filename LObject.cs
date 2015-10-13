using System.Xml;
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
	public float initiative;
	public int HP;

	public CreatureShape shape;
	public Texture2D texture;
	public bool isInParty, isAIControlled, isActive;

	public LObject() {}
	public LObject(string shapei, bool isInPartyi, bool isAIControlledi)
	{
		shape = BigBase.Instance.creatureShapes.Get(shapei);
		isInParty = isInPartyi;
		isAIControlled = isAIControlledi;

		texture = shape.texture;
		initiative = 0.0f;
		HP = shape.maxHP;
		isActive = true;
	}

	public virtual void Kill()
	{
		isActive = false;
		texture = BigBase.Instance.game.Content.Load<Texture2D>("oBlood");
	}

	public void Attack(LObject l)
	{
		l.HP -= shape.damage;
		if (l.HP <= 0) l.Kill();
	}

	public void TryToMove(ZPoint.Direction d)
	{
		ZPoint destination = position.Shift(d);
		LObject l = World.Instance.battlefield.GetLObject(destination);

		if (l != null) Attack(l);
		else if (World.Instance.battlefield.IsWalkable(destination)) Move(d);
	}

	public void Move(ZPoint.Direction d)
	{
		ZPoint destination = position.Shift(d);
		if (World.Instance.battlefield.IsWalkable(destination)) position = destination;

		PassTurn(shape.movementSpeed);
	}

	public void Run()
	{
		if (!isAIControlled)
		{
			World.Instance.battlefield.currentLObject = World.Instance.battlefield.NextLObject;
			return;
		}
		TryToMove(ZPoint.GetDirection(World.Instance.random.Next(4)));
	}

	public void PassTurn(float time)
	{
		initiative -= time;
		World.Instance.battlefield.CheckForEvents();
		World.Instance.battlefield.NextLObject.Run();
	}
}

class LPlayer : LObject
{
	public LPlayer()
	{
		shape = BigBase.Instance.creatureShapes.Get("Human");
		isInParty = true;
		isAIControlled = false;
		texture = shape.texture;
		isActive = true;
		initiative = 0.1f;
		HP = shape.maxHP;
	}
    public override void Kill() { World.Instance.player.Kill(); }
}
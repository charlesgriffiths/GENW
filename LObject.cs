using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Texture : NamedObject
{
	public Texture2D data;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
	}

	public static void LoadTextures()
	{
		foreach (Texture t in BigBase.Instance.textures.data)
		{
			t.data = MainScreen.Instance.game.Content.Load<Texture2D>("local/" + t.name);
		}
	}
}

class LObject
{
	public ZPoint position = new ZPoint();
	public RPoint rPosition = new RPoint();
	public RPoint rInitiative = new RPoint();

	public bool isActive;
	public float initiative;
	
	public Texture2D texture;
	public string name;

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }

	public LObject() {}
	//public LObject(string shapei, bool isInPartyi, bool isAIControlledi)
	public LObject(string namei)
	{
		name = namei;
		//shape = BigBase.Instance.creatureShapes.Get(shapei);
		//isInParty = isInPartyi;
		//isAIControlled = isAIControlledi;
		Init();
	}

	protected virtual void Init()
	{
		//texture = shape.texture;
		initiative = 0.0f;
		//HP = shape.maxHP;
		//controlMovementCounter = 0;
		//isActive = true;
		isActive = false;

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
		rInitiative.Add(new Vector2(initiative, 0), new Vector2(initiativei, 0), speed, B.scaleAnimations);
		initiative = initiativei;
	}

	public virtual void Kill()
	{
		isActive = false;
		//texture = M.game.Content.Load<Texture2D>("local/blood");
	}

	public virtual Color RelationshipColor
	{
		get
		{
			return Color.Blue;
			//if (isInParty) return Color.Green;
			//else return Color.Red;
		}
	}

	public virtual void Run()
	{
	}

	protected void ContinueTurn(float time)
	{
		Log.Assert(time > 0, "time <= 0");
		SetInitiative(initiative - time, 1.0f / time);
		B.CheckForEvents();
	}

	protected void PassTurn(float time)
	{
		ContinueTurn(time);

		LObject nextLObject = B.NextLObject;
		if (nextLObject != null) nextLObject.Run();
	}
}
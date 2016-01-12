using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class GlobalObject
{
	protected GlobalShape shape;
	public List<LocalObject> party = new List<LocalObject>();
	public Inventory inventory = new Inventory(6, 5, "", false);

	public HexPoint position = new HexPoint();
	public RPoint rPosition = new RPoint();
	public AnimationQueue movementAnimations = new AnimationQueue();

	public string uniqueName;
	public float initiative;
	public Dialog dialog;

	protected static MyGame G { get { return MyGame.Instance; } }
	protected static World W { get { return World.Instance; } }
	protected static Player P { get { return World.Instance.player; } }
	protected static MainScreen M { get { return MainScreen.Instance; } }
	protected static Random R { get { return World.Instance.random; } }

	public string Name { get { return shape.name; } }
	public float Speed { get { return shape.speed; } }
	public bool IsActive { get { return shape.isActive; } }

	public virtual Texture2D Texture { get { return shape.texture; } }
	/*{
		get { return shape.texture; }
		set { shape.texture = value; }
	}*/

	public GlobalObject() {}
	public GlobalObject(GlobalShape shapei)
	{
		shape = shapei;
		dialog = shapei.dialog;

		foreach (KeyValuePair<string, int> pair in shape.partyShape)
			for (int i = 0; i < pair.Value; i++)
				party.Add(new LocalObject(LocalShape.Get(pair.Key)));

		initiative = -0.1f;
	}

	private bool IsVisible()
	{
		if (!MyGame.Instance.FOVEnabled) return true;
		else if (IsActive) return W.map.IsInView(W.player.position, position);
		else return W.player[position];
    }

	public void DrawAnnotation()
	{
		if (IsVisible() && uniqueName != "")
		{
			Vector2 offset = M.fonts.small.MeasureString(uniqueName);
			Vector2 v = M.GraphicCoordinates(rPosition) + new Vector2(24 - offset.X / 2, 48);
			M.DrawRectangle(new ZPoint(v), new ZPoint(offset), new Color(0.0f, 0.0f, 0.0f, 0.8f));
			M.DrawString(M.fonts.small, uniqueName, new ZPoint(v), Color.White);
		}
	}

	public virtual void Draw()
	{
		movementAnimations.Draw();
		if (IsVisible()) M.Draw(Texture, M.GraphicCoordinates(rPosition));
	}

	public virtual void Kill()
	{
		World.Instance.gObjects.Remove(this);
	}

	public virtual void ProcessCollisions(GlobalObject g)
	{
		//if (MyMath.SamePairs("Morlocks", "Wild Dogs", name, g.name)) { Kill(); g.Kill(); }
	}

	public static void ProcessCollisions(List<GlobalObject> c)
	{
		if (c.Count == 2) c[0].ProcessCollisions(c[1]);
		else if (c.Count > 2) Log.WriteLine("That is an interesting development!");
	}

	public static void CheckForEvents()
	{
		if (P.party.Where(pc => pc.uniqueName == "Boo-Boo").Count() == 0 && R.Next(300) == 0)
			M.dialogScreen.StartDialog("Boo-Boo Died");
	}

	public virtual void Move(HexPoint.HexDirection d)
	{
		ZPoint destination = position.Shift(d);
		float travelTime = Speed * W.map[destination].type.travelTime;
        if (W.map.IsWalkable(destination)) SetPosition(destination, travelTime);
		PassTurn(travelTime);
	}

	public virtual void Run()
	{
		Move(HexPoint.GetDirection(W.random.Next(6)));
	}

	public void PassTurn(float time)
	{
		CheckForEvents();
		ProcessCollisions(W[position]);

		initiative -= time;
		if (!MyGame.Instance.dialog) W.NextGObject.Run();
	}

	public void SetPosition(HexPoint p, float gameTime)
	{
		movementAnimations.Add(new RMove(rPosition, (ZPoint)p - position, gameTime));
		position = p;
	}

	public int Max(Skill skill) { return (from c in party where c.skills != null select c.skills[skill]).Max(); }
	public int Sum(Skill skill) { return (from c in party where c.skills != null select c.skills[skill]).Sum(); }

	public bool HasAbility(ClassAbility a) { return party.Where(c => c.abilities.Has(a)).Count() > 0; }
	public bool HasAbility(string abilityName) { return HasAbility(ClassAbility.Get(abilityName)); }

	public float WeightLimit { get { return (from o in party where o.skills != null select 40.0f * (1.0f + 0.25f * o.skills["Strength"])).Sum(); } }
}
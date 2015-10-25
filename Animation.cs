using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

abstract class Animation
{
	protected int time, maxTime;

	public virtual void Draw()
	{
		time++;
	}

	public bool IsFinished { get { return time >= maxTime; } }
}

class AnimationQueue
{
	private List<Animation> data = new List<Animation>();

	public void Add(Animation a)
	{
		data.Add(a);
	}
	
	public void Draw()
	{
		if (data.Count == 0) return;

		Animation a = data[0];
		if (a.IsFinished) data.Remove(a);
		else a.Draw();
	}

	public RPoint CurrentTarget
	{
		get
		{
			if (data.Count == 0) return null;
			else return (data[0] as RMove).Target;
		}
	}
}

class RMove : Animation
{
	private RPoint target;
	private Vector2 delta;

	public RPoint Target { get { return target; } }

	public RMove(RPoint targeti, Vector2 v, float speed)
	{
		Log.Assert(speed > 0, "speed is less then zero in RMove");
		target = targeti;

		maxTime = (int)(60.0f / speed);
		if (maxTime == 0) maxTime = 1;
		delta = v / maxTime;
		time = 0;
	}

	public override void Draw()
	{
		target.Add(delta);
		base.Draw();
	}
}

class DamageAnimation : Animation
{
	private Vector2 position;
	private Texture2D texture;
	private int damage;

	public DamageAnimation(int damagei, Vector2 positioni, float seconds)
	{
		damage = damagei;
		position = positioni;
		maxTime = (int)(seconds * 60.0f);
		time = 0;

		texture = BigBase.Instance.textures.Get("damage").data;
	}

	public override void Draw()
	{
		MainScreen m = MainScreen.Instance;
		m.Draw(texture, position);
		m.DrawString(m.smallFont, damage.ToString(), new ZPoint(position) + new ZPoint(10, 10 - (int)(time*0.35f)), Color.White);
		base.Draw();
	}
}

class RPoint
{
	public float x, y;

	public RPoint() { x = 0; y = 0; }
	public RPoint(float xi, float yi) { x = xi; y = yi; }

	public void Add(Vector2 v)
	{
		x += v.X;
		y += v.Y;
	}

	public override string ToString() {	return "(" + x + ", " + y + ")"; }
}

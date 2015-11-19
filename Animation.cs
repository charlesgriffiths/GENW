using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public abstract class Animation
{
	protected int time, maxTime;

	public virtual void Draw() { time++; }
	public bool IsFinished { get { return time >= maxTime; } }

	public virtual Vector2 Position { get { return Vector2.Zero; } }
}

public class AnimationQueue
{
	private List<Animation> data = new List<Animation>();

	public void Add(Animation a)
	{
		data.Add(a);
	}

	public bool IsEmpty { get { return data.Count == 0; } }

	public Animation Peek
	{
		get
		{
			if (!IsEmpty) return data[0];
			else return null;
		}
	}

	public void Draw()
	{
		Collection<Animation> animationsToDraw = new Collection<Animation>();

		if (data.Count >= 1) animationsToDraw.Add(data[0]);
		if (data.Count >= 2 && data[0] is RMove && !(data[1] is RMove)) animationsToDraw.Add(data[1]);

		foreach (Animation a in animationsToDraw)
		{
			if (a.IsFinished) data.Remove(a);
			else a.Draw();
		}
	}

	public RPoint CurrentTarget
	{
		get
		{
			if (data.Count == 0) return null;
			else if (data[0] is RMove) return (data[0] as RMove).Target;
			else return null;
		}
	}
}

class RMove : Animation
{
	private RPoint target;
	private Vector2 delta;

	public RPoint Target { get { return target; } }
	public override Vector2 Position { get { return World.Instance.battlefield.GraphicCoordinates(Target); } }

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

class ScalingAnimation : Animation
{
	private LObject target;
	private float delta;

	public ScalingAnimation(LObject targeti, float finalScaling, float speed)
	{
		target = targeti;
		maxTime = 2 * (int)(30.0f / speed);
		delta = (finalScaling - target.scaling) / maxTime;
		time = 0;
	}

	public override void Draw()
	{
		if (time * 2 < maxTime) target.scaling += delta;
		else target.scaling -= delta;
		base.Draw();
	}
}

class DamageAnimation : Animation
{
	private Vector2 position;
	private Texture2D texture;
	private int damage;

	public override Vector2 Position { get { return position; } }

	public DamageAnimation(int damagei, Vector2 positioni, float seconds, bool isPure)
	{
		damage = damagei;
		position = positioni;
		maxTime = (int)(seconds * 60.0f);
		time = 0;

		texture = BigBase.Instance.textures.Get(isPure ? "pureDamage" : "damage").Single();
	}

	public override void Draw()
	{
		MainScreen m = MainScreen.Instance;
		m.Draw(texture, position);
		m.DrawString(m.verdanaBoldFont, damage.ToString(), new ZPoint(position) + new ZPoint(10, 10 - (int)(time*0.35f)), Color.White);
		base.Draw();
	}
}

public class RPoint
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

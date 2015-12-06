using System.Collections.Generic;
using Microsoft.Xna.Framework;

public abstract class Animation
{
	protected int frameTime, maxFrameTime;

	protected static MainScreen M { get { return MainScreen.Instance; } }

	public virtual void Draw() { frameTime++; }
	public virtual bool SpendsTime { get { return false; } }
	public bool IsFinished { get { return frameTime >= maxFrameTime; } }
	public static int FramesInOneGameTime { get { return 20; } }
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
		List<Animation> animationsToDraw = new List<Animation>();

		if (data.Count >= 1) animationsToDraw.Add(data[0]);
		//if (data.Count >= 2 && data[0] is RMove && !(data[1] is RMove)) animationsToDraw.Add(data[1]);
		if (data.Count >= 2 && !data[1].SpendsTime) animationsToDraw.Add(data[1]);

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

public class RPoint
{
	public float x, y;

	public RPoint() { x = 0; y = 0; }
	public RPoint(float xi, float yi) { x = xi; y = yi; }
	public RPoint(Vector2 v) { x = v.X; y = v.Y; }

	public void Add(Vector2 v)
	{
		x += v.X;
		y += v.Y;
	}

	public override string ToString() {	return "(" + x + ", " + y + ")"; }

	public static implicit operator ZPoint(RPoint r) { return new ZPoint((int)r.x, (int)r.y); }
}

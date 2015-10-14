using System.Collections.Generic;
using Microsoft.Xna.Framework;

class RMove
{
	public Vector2 delta;
	public int n, numberOfSteps;

	public RMove(Vector2 v, float speed)
	{
		Log.Assert(speed > 0, "speed is less then zero in RMove");
		numberOfSteps = (int)(60.0f / speed);
		if (numberOfSteps == 0) numberOfSteps = 1;
		delta = v / numberOfSteps;
		n = 0;
	}
}

class RPoint
{
	private Vector2 data = new Vector2();
	private Queue<RMove> rMoves = new Queue<RMove>();

	public float x { get { return data.X; } }
	public float y { get { return data.Y; } }

	public RPoint() { data = new Vector2(); }
	public RPoint(float x, float y) { data = new Vector2(x, y); }

	public void Update()
	{
		if (rMoves.Count == 0) return;
		RMove rMove = rMoves.Peek();

		if (rMove.n < rMove.numberOfSteps)
		{
			data += rMove.delta;
			rMove.n++;
		}
		else rMoves.Dequeue();
	}

	public void Add(Vector2 start, Vector2 finish, float speed)
	{
		RMove rMove = new RMove(finish - start, speed);
		rMoves.Enqueue(rMove);
	}

	public static implicit operator Vector2(RPoint p) { return new Vector2(p.x, p.y); }
}

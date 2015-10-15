using System.Collections.Generic;
using Microsoft.Xna.Framework;

class RMove
{
	public Vector2 delta;
	public int n, numberOfSteps;
	public RPoint rPoint;

	public RMove(RPoint p, Vector2 v, float speed)
	{
		Log.Assert(speed > 0, "speed is less then zero in RMove");
		numberOfSteps = (int)(60.0f / speed);
		if (numberOfSteps == 0) numberOfSteps = 1;
		delta = v / numberOfSteps;
		n = 0;

		rPoint = p;
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

	public static void Update(Queue<RMove> q)
	{
		if (q.Count == 0) return;
		RMove rMove = q.Peek();

		if (rMove.n < rMove.numberOfSteps)
		{
			rMove.rPoint.data += rMove.delta;
			rMove.n++;
		}
		else q.Dequeue();
	}

	public void Update() { Update(rMoves); }

	public void Add(Vector2 v, float speed, Queue<RMove> q)
	{
		RMove rMove = new RMove(this, v, speed);
		q.Enqueue(rMove);
	}

	public void Add(Vector2 start, Vector2 finish, float speed, Queue<RMove> q)
	{ Add(finish - start, speed, q); }

	public void Add(Vector2 v, float speed) { Add(v, speed, rMoves); }
	public void Add(Vector2 start, Vector2 finish, float speed) { Add(finish - start, speed); }

	public static implicit operator Vector2(RPoint p) { return new Vector2(p.x, p.y); }
}

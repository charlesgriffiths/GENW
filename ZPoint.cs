using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

class ZPoint
{
	public int x, y;

	public ZPoint() { x = y = 0; }
	public ZPoint(int xi, int yi) { x = xi; y = yi; }
	public ZPoint(Vector2 v) { x = (int)v.X; y = (int)v.Y; }
	public static ZPoint Zero { get { return new ZPoint(0, 0); } }

	public enum Direction { Right, Up, Left, Down };

	public ZPoint Shift(Direction d)
	{
		if (d == Direction.Right) return new ZPoint(x + 1, y);
		else if (d == Direction.Up) return new ZPoint(x, y - 1);
		else if (d == Direction.Left) return new ZPoint(x - 1, y);
		else return new ZPoint(x, y + 1);
	}

	public static Direction GetDirection(int i)
	{
		Log.Assert(i >= 0 && i < 4, "wrong rectangular direction");

		if (i == 0) return Direction.Right;
		else if (i == 1) return Direction.Up;
		else if (i == 2) return Direction.Left;
		else return Direction.Down;
	}

	public static Collection<Direction> Directions
	{
		get
		{
			Collection<Direction> result = new Collection<Direction>();
			result.Add(Direction.Right);
			result.Add(Direction.Up);
			result.Add(Direction.Left);
			result.Add(Direction.Down);
			return result;
		}
	}

	public static ZPoint Min(ZPoint p1, ZPoint p2)
	{
		return new ZPoint(Math.Min(p1.x, p2.x), Math.Min(p1.y, p2.y));
	}

	public static ZPoint Max(ZPoint p1, ZPoint p2)
	{
		return new ZPoint(Math.Max(p1.x, p2.x), Math.Max(p1.y, p2.y));
	}

	public static ZPoint operator +(ZPoint p1, ZPoint p2)
	{
		return new ZPoint(p1.x + p2.x, p1.y + p2.y);
	}

	public static Vector2 operator +(ZPoint p, Vector2 v)
	{
		return new Vector2(p.x + v.X, p.y + v.Y);
	}

	public static ZPoint operator -(ZPoint p1, ZPoint p2)
	{
		return new ZPoint(p1.x - p2.x, p1.y - p2.y);
	}

	public static Vector2 operator /(ZPoint p, float f)
	{
		return new Vector2(p.x / f, p.y / f);
	}

	public static implicit operator HexPoint(ZPoint zPoint) { return new HexPoint(zPoint.x, zPoint.y); }
	public static implicit operator RPoint(ZPoint zPoint) {	return new RPoint(zPoint.x, zPoint.y); }
	public static implicit operator Vector2(ZPoint zPoint) { return new Vector2(zPoint.x, zPoint.y); }

	public override string ToString()
	{
		return "(" + x + "," + y + ")";
	}

	public bool TheSameAs(ZPoint p)
	{
		if (x == p.x && y == p.y) return true;
		else return false;
	}

	public ZPoint Boundaries(ZPoint p1, ZPoint p2)
	{
		if (p1.x > p2.x || p1.y > p2.y) return this;
		else return Min(p2, Max(p1, this));
	}

	public bool InBoundaries(ZPoint p1, ZPoint p2)
	{
		if (x < p1.x || y < p1.y || x > p2.x || y > p2.y) return false;
		else return true;
	}

	public bool IsAdjacent(ZPoint p) { return MyMath.ManhattanDistance(this, p) == 1; }
}
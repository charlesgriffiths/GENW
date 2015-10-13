using System;
using Microsoft.Xna.Framework;

class ZPoint
{
	public int x, y;

	public ZPoint() { x = y = 0; }
	public ZPoint(int xi, int yi) { x = xi; y = yi; }
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

	public static ZPoint operator -(ZPoint p1, ZPoint p2)
	{
		return new ZPoint(p1.x - p2.x, p1.y - p2.y);
	}

	public static implicit operator HexPoint(ZPoint zPoint)
	{
		return new HexPoint(zPoint.x, zPoint.y);
	}

	public static implicit operator Vector2(ZPoint zPoint)
	{
		return new Vector2(zPoint.x, zPoint.y);
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
}

/*
class ZRectangle
{
	public ZPoint p, size;

	public ZPoint q { get { return p + size; } }
}
*/
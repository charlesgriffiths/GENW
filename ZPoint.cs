using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

public class ZPoint
{
	public int x, y;

	public ZPoint() { x = y = 0; }
	public ZPoint(int xi, int yi) { x = xi; y = yi; }
	public ZPoint(Vector2 v) { x = (int)v.X; y = (int)v.Y; }

	public enum Direction { Right, Up, Left, Down };

	public ZPoint(Direction d)
	{
		if (d == Direction.Right) { x = 1; y = 0; }
		else if (d == Direction.Up) { x = 0; y = -1; }
		else if (d == Direction.Left) { x = -1; y = 0; }
		else { x = 0; y = 1; }
	}

	public Direction GetDirection()
	{
		Log.Assert(ManhattanNorm == 1, "wrong parameter in GetDirection");

		if (x == 0) return y == 1 ? Direction.Down : Direction.Up;
		else if (x == 1) return Direction.Right;
		else return Direction.Left;
	}

	public static string Name(Direction d)
	{
		if (d == Direction.Right) return "right";
		else if (d == Direction.Up) return "up";
		else if (d == Direction.Left) return "left";
		else return "down";
	}

	public static ZPoint Zero { get { return new ZPoint(0, 0); } }

	public int ManhattanNorm { get { return MyMath.ManhattanDistance(this, Zero); } }

	public static float Angle(Direction d)
	{
		if (d == Direction.Right) return 0.0f;
		else if (d == Direction.Up) return 1.5f * MyMath.PI;
		else if (d == Direction.Left) return MyMath.PI;
		else return 0.5f * MyMath.PI;
	}

	public ZPoint Shift(Direction d) { return this + new ZPoint(d); }
	public ZPoint Shift(Direction d, int n) { return this + n * new ZPoint(d); }

	public static Direction GetDirection(int i)
	{
		Log.Assert(i >= 0 && i < 4, "wrong rectangular direction");

		if (i == 0) return Direction.Right;
		else if (i == 1) return Direction.Up;
		else if (i == 2) return Direction.Left;
		else return Direction.Down;
	}

	public static Direction GetDirection(ZPoint p)
	{
		if (p.x == 1 && p.y == 0) return Direction.Right;
		else if (p.x == 0 && p.y == -1) return Direction.Up;
		else if (p.x == -1 && p.y == 0) return Direction.Left;
		else if (p.x == 0 && p.y == 1) return Direction.Down;
		else
		{
			Log.Error("unacceptable parameter in GetDirection(ZPoint p)");
			return Direction.Right;
		}
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

	public static Direction Opposite(Direction d)
	{
		if (d == Direction.Right) return Direction.Left;
		else if (d == Direction.Up) return Direction.Down;
		else if (d == Direction.Left) return Direction.Right;
		else return Direction.Up;
	}

	public static ZPoint Min(ZPoint p1, ZPoint p2) { return new ZPoint(Math.Min(p1.x, p2.x), Math.Min(p1.y, p2.y));	}
	public static ZPoint Max(ZPoint p1, ZPoint p2) { return new ZPoint(Math.Max(p1.x, p2.x), Math.Max(p1.y, p2.y));	}
	public static ZPoint operator +(ZPoint p1, ZPoint p2) {	return new ZPoint(p1.x + p2.x, p1.y + p2.y); }
	public static Vector2 operator +(ZPoint p, Vector2 v) { return new Vector2(p.x + v.X, p.y + v.Y); }
	public static ZPoint operator+(ZPoint p, Direction d) { return p.Shift(d); }
	public static ZPoint operator -(ZPoint p1, ZPoint p2) {	return new ZPoint(p1.x - p2.x, p1.y - p2.y); }
	public static Vector2 operator /(ZPoint p, float f) { return new Vector2(p.x / f, p.y / f);	}
	public static ZPoint operator *(int k, ZPoint p) { return new ZPoint(k * p.x, k * p.y); }
	public static Vector2 operator *(float f, ZPoint p) { return new Vector2(f * p.x, f * p.y); }

	public static implicit operator HexPoint(ZPoint zPoint) { return new HexPoint(zPoint.x, zPoint.y); }
	public static implicit operator RPoint(ZPoint zPoint) {	return new RPoint(zPoint.x, zPoint.y); }
	public static implicit operator Vector2(ZPoint zPoint) { return new Vector2(zPoint.x, zPoint.y); }
	public static implicit operator Point(ZPoint zPoint) { return new Point(zPoint.x, zPoint.y); }

	public override string ToString() {	return "(" + x + "," + y + ")"; }

	public ZPoint Boundaries(ZPoint p1, ZPoint p2)
	{
		if (p1.x > p2.x || p1.y > p2.y) return this;
		else return Min(p2, Max(p1, this));
	}

	public bool TheSameAs(ZPoint p) { return x == p.x && y == p.y; }
	public bool InBoundaries(ZPoint p1, ZPoint p2) { return x >= p1.x && y >= p1.y && x <= p2.x && y <= p2.y; }
	public bool IsAdjacentTo(ZPoint p) { return MyMath.ManhattanDistance(this, p) == 1; }

	public bool IsIn(List<ZPoint> list)	{ return (from p in list where TheSameAs(p) select p).Count() > 0; }
	public bool IsIn(List<FramedZPoint> list) {	return (from p in list where TheSameAs(p.data) select p).Count() > 0; }
	public bool IsIn(MouseTrigger t) { return x >= t.position.x && y >= t.position.y && x <= t.position.x + t.size.x && y <= t.position.y + t.size.y; }

	public Direction GetDirection(List<FramedZPoint> list) { return (from p in list where TheSameAs(p.data) select p.d).Single(); }
}
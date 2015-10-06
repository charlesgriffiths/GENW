using System;
using Microsoft.Xna.Framework;

class ZPoint
{
	public int x, y;

	public ZPoint() { x = y = 0; }
	public ZPoint(int xi, int yi) { x = xi; y = yi; }
	public static ZPoint Zero { get { return new ZPoint(0, 0); } }

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

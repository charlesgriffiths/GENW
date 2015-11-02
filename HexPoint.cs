using System;
using System.Collections.ObjectModel;

class HexPoint
{
	public int x, y;

	public HexPoint() { x = y = 0; }
	public HexPoint(int xi, int yi) { x = xi; y = yi; }

	public enum HexDirection { S, SE, NE, N, NW, SW };

	public static Collection<HexDirection> Directions
	{
		get
		{
			Collection<HexDirection> result = new Collection<HexDirection>();
			result.Add(HexDirection.S);
			result.Add(HexDirection.SE);
			result.Add(HexDirection.NE);
			result.Add(HexDirection.N);
			result.Add(HexDirection.NW);
			result.Add(HexDirection.SW);
			return result;
		}
	}

	public static HexDirection Opposite(HexDirection d)
	{
		if (d == HexDirection.N) return HexDirection.S;
		else if (d == HexDirection.S) return HexDirection.N;
		else if (d == HexDirection.SE) return HexDirection.NW;
		else if (d == HexDirection.NE) return HexDirection.SW;
		else if (d == HexDirection.SW) return HexDirection.NE;
		else return HexDirection.SE;
	}

	public HexPoint Shift(HexDirection d)
	{
		if (d == HexDirection.S) return new HexPoint(x, y + 1);
		else if (d == HexDirection.SE) return new HexPoint(x + 1, y + MyMath.IsOdd(x));
		else if (d == HexDirection.NE) return new HexPoint(x + 1, y - MyMath.IsEven(x));
		else if (d == HexDirection.N) return new HexPoint(x, y - 1);
		else if (d == HexDirection.NW) return new HexPoint(x - 1, y - MyMath.IsEven(x));
		else return new HexPoint(x - 1, y + MyMath.IsOdd(x));
	}

	public HexPoint S { get { return Shift(HexDirection.S); } }
	public HexPoint SE { get { return Shift(HexDirection.SE); } }
	public HexPoint NE { get { return Shift(HexDirection.NE); } }
	public HexPoint N { get { return Shift(HexDirection.N); } }
	public HexPoint NW { get { return Shift(HexDirection.NW); } }
	public HexPoint SW { get { return Shift(HexDirection.SW); } }

	public static HexDirection GetDirection(int i)
	{
		Log.Assert(i >= 0 && i < 6, "code for HexDirection is out of range");

		if (i == 0) return HexDirection.S;
		else if (i == 1) return HexDirection.SE;
		else if (i == 2) return HexDirection.NE;
		else if (i == 3) return HexDirection.N;
		else if (i == 4) return HexDirection.NW;
		else return HexDirection.SW;
	}

	public void Change(HexDirection d)
	{
		HexPoint p = Shift(d);
		x = p.x;
		y = p.y;
	}

	public static implicit operator ZPoint(HexPoint hexPoint) {	return new ZPoint(hexPoint.x, hexPoint.y); }
	public static implicit operator RPoint(HexPoint hexPoint) { return new RPoint(hexPoint.x, hexPoint.y); }

	public static implicit operator string (HexPoint hexPoint)
	{
		return "(" + hexPoint.x + ", " + hexPoint.y + ")";
	}

	public bool TheSameAs(HexPoint p)
	{
		if (x == p.x && y == p.y) return true;
		else return false;
	}

	public struct CubePoint { public int x, y, z; }
	public CubePoint CubeCoordinates
	{
		get
		{
			CubePoint result = new CubePoint();
			result.x = x;
			result.z = y - (x - MyMath.IsOdd(x)) / 2;
			result.y = -result.x - result.z;
			return result;
		}
	}

	public static int Distance(HexPoint p1, HexPoint p2)
	{
		CubePoint c1 = p1.CubeCoordinates;
		CubePoint c2 = p2.CubeCoordinates;
		return MyMath.Max(Math.Abs(c1.x - c2.x), Math.Abs(c1.y - c2.y), Math.Abs(c1.z - c2.z));
	}
}
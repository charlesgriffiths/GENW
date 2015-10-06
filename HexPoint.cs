class HexPoint
{
	public int x, y;

	public HexPoint() { x = y = 0; }
	public HexPoint(int xi, int yi) { x = xi; y = yi; }

	public enum HexDirection { S, SE, NE, N, NW, SW };

	public HexPoint Shift(HexDirection d)
	{
		if (d == HexDirection.S) return new HexPoint(x, y + 1);
		else if (d == HexDirection.SE) return new HexPoint(x + 1, y + MyMath.IsOdd(x));
		else if (d == HexDirection.NE) return new HexPoint(x + 1, y - MyMath.IsEven(x));
		else if (d == HexDirection.N) return new HexPoint(x, y - 1);
		else if (d == HexDirection.NW) return new HexPoint(x - 1, y - MyMath.IsEven(x));
		else return new HexPoint(x - 1, y + MyMath.IsOdd(x));
	}

	public void Change(HexDirection d)
	{
		HexPoint p = Shift(d);
		x = p.x;
		y = p.y;
	}

	public static implicit operator ZPoint(HexPoint hexPoint)
	{
		return new ZPoint(hexPoint.x, hexPoint.y);
	}

	public static implicit operator string (HexPoint hexPoint)
	{
		return "(" + hexPoint.x + ", " + hexPoint.y + ")";
	}
}
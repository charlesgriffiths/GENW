using System;
using Microsoft.Xna.Framework;

static class MyMath
{
	public static int IsOdd(int n) { return Math.Abs(n) % 2; }
	public static int IsEven(int n) { return 1 - IsOdd(n); }

	public static float SawFunction(float x)
	{
		if ((int)x % 2 == 0) return x - (int)x;
		else return 1 + (int)x - x;
	}

	public static int Max(int i1, int i2, int i3) { return Math.Max(i1, Math.Max(i2, i3)); }

	public static bool SamePairs(string s1, string s2, string t1, string t2)
	{
		if ((s1 == t1 && s2 == t2) || (s1 == t2 && s2 == t1)) return true;
		else return false;
	}

	public static float ManhattanDistance(Vector2 v1, Vector2 v2)
	{
		return Math.Abs(v1.X - v2.X) + Math.Abs(v1.Y - v2.Y);
	}

	public static int ManhattanDistance(ZPoint p1, ZPoint p2)
	{
		return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
	}
}
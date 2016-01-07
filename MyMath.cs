using System;
using Microsoft.Xna.Framework;

static class MyMath
{
	public static float PI { get { return 3.14159265f; } }
	public static int IsOdd(int n) { return Math.Abs(n) % 2; }
	public static int IsEven(int n) { return 1 - IsOdd(n); }
	public static float SawFunction(float x) { return (int)x % 2 == 0 ? x - (int)x : 1 + (int)x - x; }
	public static int Max(int i1, int i2, int i3) { return Math.Max(i1, Math.Max(i2, i3)); }
	public static int ManhattanDistance(ZPoint p1, ZPoint p2) {	return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y); }
	public static float ManhattanDistance(Vector2 v1, Vector2 v2) {	return Math.Abs(v1.X - v2.X) + Math.Abs(v1.Y - v2.Y); }
}
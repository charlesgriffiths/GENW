using System;

static class MyMath
{
	public static int IsOdd(int n) { return Math.Abs(n) % 2; }
	public static int IsEven(int n) { return 1 - IsOdd(n); }

	public static int Max(int i1, int i2, int i3) { return Math.Max(i1, Math.Max(i2, i3)); }
}
using System;

static class MyMath
{
	public static int IsOdd(int n) { return Math.Abs(n) % 2; }
	public static int IsEven(int n) { return 1 - IsOdd(n); }
}
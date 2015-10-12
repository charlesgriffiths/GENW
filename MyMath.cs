using System;
using System.Collections.ObjectModel;

static class MyMath
{
	public static int IsOdd(int n) { return Math.Abs(n) % 2; }
	public static int IsEven(int n) { return 1 - IsOdd(n); }

	public static int Max(int i1, int i2, int i3) { return Math.Max(i1, Math.Max(i2, i3)); }

	public static bool SamePairs(string s1, string s2, string t1, string t2)
	{
		if ((s1 == t1 && s2 == t2) || (s1 == t2 && s2 == t1)) return true;
		else return false;
	}

	public static string Split(string s, int length)
	{
		string result = s;
		int line = 1, k = 0;

		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == ' ') k = i + 2*(line-1);
			if (i > length * line)
			{
				//result.Remove(k);
				result = result.Insert(k+1, "\n\r");
				line++;
			}
		}

		return result;
	}
}
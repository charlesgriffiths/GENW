public static class Log
{
	public static void Write(string message)
	{
		System.Diagnostics.Debug.Write(message);
	}

	public static void WriteLine(string message)
	{
		Write(message);
		Write(System.Environment.NewLine);
	}

	public static void Error(string message)
	{
		Write(System.Environment.NewLine);
		Write("ERROR: ");
		Write(message);
		WriteLine("!");
	}

	public static void Assert(bool proposition, string message)
	{
		if (!proposition) Error(message);
	}
}

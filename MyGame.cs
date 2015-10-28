class MyGame
{
	public bool input, dialog, battle, editor, debug;
	public Console console;

	private static readonly MyGame instance = new MyGame();
	public static MyGame Instance { get { return instance; } }

	private MyGame()
	{
		console = new Console();

		input = false;
		dialog = false;
		battle = false;
		editor = false;
		debug = false;
	}
}

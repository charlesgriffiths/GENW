using Microsoft.Xna.Framework.Input;

class MyGame
{
	public bool input, dialog, battle, editor, debug;
	public Console console;

	public KeyboardState keyboardState, previousKeyboardState;
	public MouseState mouseState, previousMouseState;

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

	public bool LeftMouseButtonClicked { get { return mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released; } }
	public bool RightMouseButtonClicked { get { return mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released; } }
}

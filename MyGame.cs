using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

public class MyGame
{
	public bool input, dialog, battle, editor, debug, combatLog, FOVEnabled;
	public Console console;

	public KeyboardState keyboardState, previousKeyboardState;
	public MouseState mouseState, previousMouseState;

	public List<MouseTriggerKeyword> mouseTriggerKeywords = new List<MouseTriggerKeyword>();
	public List<MouseTriggerLCreature> mouseTriggerLCreatures = new List<MouseTriggerLCreature>();
	public List<MouseTriggerCreature> mouseTriggerCreatures = new List<MouseTriggerCreature>();
	public List<MouseTriggerInventory> mouseTriggerInventories = new List<MouseTriggerInventory>();

	public Item dndItem = null;
	public Inventory inventory = null;
	public int cell = 0;

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
		combatLog = false;

		FOVEnabled = true;
    }

	public ZPoint Mouse { get { return new ZPoint(mouseState.Position.ToVector2()); } }

	public bool LeftMouseButtonClicked { get { return mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released; } }
	public bool RightMouseButtonClicked { get { return mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released; } }
	public bool LeftMouseButtonReleased { get { return mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed; } }
}

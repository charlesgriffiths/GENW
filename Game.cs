using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MyGame : Game
{
	GraphicsDeviceManager graphics;
	SpriteBatch spriteBatch;

	KeyboardState previousKeyboardState;
	MouseState previousMouseState;

	public MyGame()
	{
		graphics = new GraphicsDeviceManager(this);
		graphics.PreferredBackBufferWidth = MainScreen.Instance.size.x;
		graphics.PreferredBackBufferHeight = MainScreen.Instance.size.y;
		Window.Position = new Point(MainScreen.Instance.position.x, MainScreen.Instance.position.y);
		graphics.ApplyChanges();

		Content.RootDirectory = "Content";
	}

	protected override void Initialize()
	{
		base.Initialize();
		IsMouseVisible = true;

		previousKeyboardState = Keyboard.GetState();
		previousMouseState = Mouse.GetState();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);

		BigBase.Instance.Load(this);
		World.Instance.Load();

		MainScreen.Instance.LoadTextures(this);
        GTile.LoadTextures(this);
		LTile.LoadTextures(this);
		CreatureShape.LoadTextures();
		World.Instance.LoadTextures(this);
	}

	protected override void UnloadContent()
	{
		Content.Unload();
	}

	private bool KeyPressed(KeyboardState currentState, KeyboardState previousState, Keys key)
	{
		if (currentState.IsKeyDown(key) && !previousState.IsKeyDown(key)) return true;
		else return false;
	}

	private bool LeftButtonPressed(MouseState currentState, MouseState previousState)
	{
		if (currentState.LeftButton == ButtonState.Pressed && !(previousState.LeftButton == ButtonState.Pressed)) return true;
		else return false;
	}

	//тут много тяжелого текста, можно все убрать на самом деле!
	protected override void Update(GameTime gameTime)
	{
		KeyboardState keyboardState = Keyboard.GetState();
		MouseState mouseState = Mouse.GetState();
		MainScreen m = MainScreen.Instance;

		if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

		if (m.gameState == MainScreen.GameState.Dialog && keyboardState != previousKeyboardState) m.dialogScreen.Press(keyboardState);
		else if (m.gameState == MainScreen.GameState.Local)
		{
			if (KeyPressed(keyboardState, previousKeyboardState, Keys.Right)) World.Instance.battlefield.currentLObject.TryToMove(ZPoint.Direction.Right);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Up)) World.Instance.battlefield.currentLObject.TryToMove(ZPoint.Direction.Up);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Left)) World.Instance.battlefield.currentLObject.TryToMove(ZPoint.Direction.Left);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Down)) World.Instance.battlefield.currentLObject.TryToMove(ZPoint.Direction.Down);
		}
		else
		{
			if (KeyPressed(keyboardState, previousKeyboardState, Keys.Home)) World.Instance.player.Move(HexPoint.HexDirection.N);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.End)) World.Instance.player.Move(HexPoint.HexDirection.S);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Insert)) World.Instance.player.Move(HexPoint.HexDirection.NW);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Delete)) World.Instance.player.Move(HexPoint.HexDirection.SW);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.PageUp)) World.Instance.player.Move(HexPoint.HexDirection.NE);
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.PageDown)) World.Instance.player.Move(HexPoint.HexDirection.SE);

			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Right)) m.editor.GoRight();
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.Left)) m.editor.GoLeft();
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.S)) World.Instance.map.Save();
			else if (KeyPressed(keyboardState, previousKeyboardState, Keys.F)) World.Instance.player.FOVEnabled = !World.Instance.player.FOVEnabled;

			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				HexPoint p = m.HexCoordinates(mouseState.Position.ToVector2());
				World.Instance.map[p] = m.editor.Brush;
			}
		}

		previousKeyboardState = keyboardState;
		previousMouseState = mouseState;
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();

		MainScreen.Instance.Draw(spriteBatch, previousMouseState.Position.ToVector2());

		spriteBatch.End();
		base.Draw(gameTime);
	}
}

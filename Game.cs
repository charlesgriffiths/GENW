using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MyGame : Game
{
	GraphicsDeviceManager graphics;
	SpriteBatch spriteBatch;
	MainScreen mainScreen;

	KeyboardState previousKeyboardState;
	MouseState previousMouseState;

	public MyGame()
	{
		graphics = new GraphicsDeviceManager(this);
		mainScreen = new MainScreen();
		graphics.PreferredBackBufferWidth = mainScreen.size.x;
		graphics.PreferredBackBufferHeight = mainScreen.size.y;
		Window.Position = new Point(mainScreen.position.x, mainScreen.position.y);
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

		BigBase.Instance.Load(GraphicsDevice);
		World.Instance.Load();

		mainScreen.LoadTextures(this);
		GlobalTile.LoadTextures(this);
		World.Instance.player.LoadTexture(this);
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

	protected override void Update(GameTime gameTime)
	{
		KeyboardState keyboardState = Keyboard.GetState();
		MouseState mouseState = Mouse.GetState();

		if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

		if (KeyPressed(keyboardState, previousKeyboardState, Keys.Home)) World.Instance.player.Move(HexPoint.HexDirection.N);
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.End)) World.Instance.player.Move(HexPoint.HexDirection.S);
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.Insert)) World.Instance.player.Move(HexPoint.HexDirection.NW);
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.Delete)) World.Instance.player.Move(HexPoint.HexDirection.SW);
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.PageUp)) World.Instance.player.Move(HexPoint.HexDirection.NE);
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.PageDown)) World.Instance.player.Move(HexPoint.HexDirection.SE);

		if (KeyPressed(keyboardState, previousKeyboardState, Keys.Right)) mainScreen.editor.GoRight();
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.Left)) mainScreen.editor.GoLeft();
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.S)) World.Instance.map.Save();
		if (KeyPressed(keyboardState, previousKeyboardState, Keys.F)) World.Instance.player.FOVEnabled = !World.Instance.player.FOVEnabled;

		if (mouseState.LeftButton == ButtonState.Pressed)
		{
			HexPoint p = mainScreen.HexCoordinates(mouseState.Position.ToVector2());
			World.Instance.map[p] = mainScreen.editor.Brush;
		}

		previousKeyboardState = keyboardState;
		previousMouseState = mouseState;
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();

		mainScreen.Draw(spriteBatch, previousMouseState.Position.ToVector2());

		spriteBatch.End();
		base.Draw(gameTime);
	}
}

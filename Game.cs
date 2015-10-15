using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MyGame : Game
{
	GraphicsDeviceManager graphics;
	//SpriteBatch spriteBatch;
	KeyboardState keyboardState, previousKeyboardState;
	MouseState mouseState, previousMouseState;

	private World W { get { return World.Instance; } }
	private MainScreen M { get { return MainScreen.Instance; } }
	private Battlefield B { get { return World.Instance.battlefield; } }

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
		//spriteBatch = new SpriteBatch(GraphicsDevice);

		MainScreen.Instance.Init(this);
		BigBase.Instance.Load();
		World.Instance.Load();
		
		MainScreen.Instance.LoadTextures();
        GTile.LoadTextures();
		LTile.LoadTextures();
		CreatureShape.LoadTextures();
		World.Instance.LoadTextures();
	}

	protected override void UnloadContent()
	{
		Content.Unload();
	}

	private bool KeyPressed(Keys key)
	{
		if (keyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key)) return true;
		else return false;
	}

	private bool LeftButtonPressed()
	{
		if (mouseState.LeftButton == ButtonState.Pressed && !(previousMouseState.LeftButton == ButtonState.Pressed)) return true;
		else return false;
	}

	protected override void Update(GameTime gameTime)
	{
		keyboardState = Keyboard.GetState();
		mouseState = Mouse.GetState();

		if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

		if (M.gameState == MainScreen.GameState.Dialog && keyboardState != previousKeyboardState) M.dialogScreen.Press(keyboardState);
		else if (M.gameState == MainScreen.GameState.Local)
		{
			if (KeyPressed(Keys.Right)) B.currentLObject.TryToMove(ZPoint.Direction.Right, keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Up)) B.currentLObject.TryToMove(ZPoint.Direction.Up, keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Left)) B.currentLObject.TryToMove(ZPoint.Direction.Left, keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Down)) B.currentLObject.TryToMove(ZPoint.Direction.Down, keyboardState.IsKeyDown(Keys.LeftControl));
		}
		else
		{
			if (KeyPressed(Keys.Home)) W.player.Move(HexPoint.HexDirection.N);
			else if (KeyPressed(Keys.End)) W.player.Move(HexPoint.HexDirection.S);
			else if (KeyPressed(Keys.Insert)) W.player.Move(HexPoint.HexDirection.NW);
			else if (KeyPressed(Keys.Delete)) W.player.Move(HexPoint.HexDirection.SW);
			else if (KeyPressed(Keys.PageUp)) W.player.Move(HexPoint.HexDirection.NE);
			else if (KeyPressed(Keys.PageDown)) W.player.Move(HexPoint.HexDirection.SE);

			else if (KeyPressed(Keys.Right)) M.editor.GoRight();
			else if (KeyPressed(Keys.Left)) M.editor.GoLeft();
			else if (KeyPressed(Keys.S)) W.map.Save();
			else if (KeyPressed(Keys.F)) W.player.FOVEnabled = !W.player.FOVEnabled;

			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				HexPoint p = M.HexCoordinates(mouseState.Position.ToVector2());
				W.map[p] = M.editor.Brush;
			}
		}

		previousKeyboardState = keyboardState;
		previousMouseState = mouseState;
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		M.spriteBatch.Begin();
		MainScreen.Instance.Draw(previousMouseState.Position.ToVector2());
		M.spriteBatch.End();
		base.Draw(gameTime);
	}
}

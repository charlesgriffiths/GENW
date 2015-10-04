using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MyGame : Game
{
	GraphicsDeviceManager graphics;
	SpriteBatch spriteBatch;
	KeyboardState previousState;

	public MyGame()
	{
		graphics = new GraphicsDeviceManager(this);
		graphics.PreferredBackBufferWidth = Screen.Instance.size.x;
		graphics.PreferredBackBufferHeight = Screen.Instance.size.y;
		Window.Position = new Point(Screen.Instance.position.x, Screen.Instance.position.y);
		graphics.ApplyChanges();

		Content.RootDirectory = "Content";
	}

	protected override void Initialize()
	{
		base.Initialize();
		IsMouseVisible = true;
		previousState = Keyboard.GetState();
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);

		BigBase.Instance.Load();
		World.Instance.Init();

		GlobalTile.LoadTextures(this);
		World.Instance.player.LoadTexture(this);
	}

	protected override void UnloadContent()
	{
		Content.Unload();
	}

	protected override void Update(GameTime gameTime)
	{
		KeyboardState state = Keyboard.GetState();

		if (state.IsKeyDown(Keys.Escape)) Exit();

		if (state.IsKeyDown(Keys.Home) && !previousState.IsKeyDown(Keys.Home)) World.Instance.player.position.Change(HexPoint.HexDirection.N);
		if (state.IsKeyDown(Keys.End) && !previousState.IsKeyDown(Keys.End)) World.Instance.player.position.Change(HexPoint.HexDirection.S);
		if (state.IsKeyDown(Keys.Insert) && !previousState.IsKeyDown(Keys.Insert)) World.Instance.player.position.Change(HexPoint.HexDirection.NW);
		if (state.IsKeyDown(Keys.Delete) && !previousState.IsKeyDown(Keys.Delete)) World.Instance.player.position.Change(HexPoint.HexDirection.SW);
		if (state.IsKeyDown(Keys.PageUp) && !previousState.IsKeyDown(Keys.PageUp)) World.Instance.player.position.Change(HexPoint.HexDirection.NE);
		if (state.IsKeyDown(Keys.PageDown) && !previousState.IsKeyDown(Keys.PageDown)) World.Instance.player.position.Change(HexPoint.HexDirection.SE);

		previousState = state;
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();

		Screen.Instance.Draw(spriteBatch);

		spriteBatch.End();
		base.Draw(gameTime);
	}
}

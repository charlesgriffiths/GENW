using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MyGame : Game
{
	GraphicsDeviceManager graphics;
	SpriteBatch spriteBatch;

	public MyGame()
	{
		graphics = new GraphicsDeviceManager(this);
		graphics.PreferredBackBufferWidth = 640;
		graphics.PreferredBackBufferHeight = 480;
		graphics.ApplyChanges();

		Content.RootDirectory = "Content";
	}

	protected override void Initialize()
	{
		base.Initialize();
		IsMouseVisible = true;
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);

		BigBase.Instance.Load();
		World.Instance.Init();
		GlobalTile.LoadTextures(this);
	}

	protected override void UnloadContent()
	{
		Content.Unload();
	}

	protected override void Update(GameTime gameTime)
	{
		KeyboardState state = Keyboard.GetState();

		if (state.IsKeyDown(Keys.Escape)) Exit();

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin();

		for (int i = 0; i < World.Instance.globalMap.Width; i++)
			for (int j = 0; j < World.Instance.globalMap.Height; j++)
			{
				Vector2 position = new Vector2 ();
				if (j % 2 == 0) position.X = i * 32;
				else position.X = i * 32 + 16;
				position.Y = j * 32;
				spriteBatch.Draw(World.Instance.globalMap[i,j].texture, position);
            }

		spriteBatch.End();
		base.Draw(gameTime);
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Xml;

public class MyGame : Game
{
	GraphicsDeviceManager graphics;
	SpriteBatch spriteBatch;
	Texture2D[] texture;

	int[,] map;
	int width, height;

	public MyGame()
	{
		graphics = new GraphicsDeviceManager(this);
		graphics.PreferredBackBufferWidth = 256;
		graphics.PreferredBackBufferHeight = 192;
		graphics.ApplyChanges();

		Content.RootDirectory = "Content";
		texture = new Texture2D[3];
	}

	protected override void Initialize()
	{
		base.Initialize();
		IsMouseVisible = true;

		XmlDocument xdoc = new XmlDocument();
		xdoc.Load("map.xml");
		XmlNode xnode = xdoc.FirstChild;

		XmlElement xl = (XmlElement)xnode;
		width = int.Parse(xl.GetAttribute("width"));
		height = int.Parse(xl.GetAttribute("height"));
		map = new int[width, height];

		xnode = xnode.FirstChild;
		string text = xnode.InnerText;
		char[] delimiters = new char[] { '\r', '\n', ' ' };
		string[] dataLines = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

		for (int j = 0; j < height; j++)
		{
			string line = dataLines[j];
			for (int i = 0; i < width; i++) map[i, j] = (int)Char.GetNumericValue(line[i]);
		}
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);
		texture[0] = Content.Load<Texture2D>("water");
		texture[1] = Content.Load<Texture2D>("land");
		texture[2] = Content.Load<Texture2D>("forest");
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

		for (int i = 0; i < 8; i++)
			for (int j = 0; j < 6; j++)
			{
				Vector2 position = new Vector2(i * 32, j * 32);
				spriteBatch.Draw(texture[map[i, j]], position);
			}

		spriteBatch.End();
		base.Draw(gameTime);
	}
}

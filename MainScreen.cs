using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class MainScreen : Screen
{
	private static readonly MainScreen instance = new MainScreen();
	public static MainScreen Instance { get { return instance; } }

	public ZPoint windowPosition;
	public SpriteBatch spriteBatch;
	public Game game;

	public Editor editor;
	public DialogScreen dialogScreen;

	public Texture2D universalTexture;
	private Texture2D hexSelectionTexture;
	private SpriteFont ambientFont;

	public Collection<RPoint> rPoints = new Collection<RPoint>();
	public Queue<RMove> rMoves = new Queue<RMove>();

	private MainScreen()
	{
		windowPosition = new ZPoint(200, 150);
		position = new ZPoint(0, 0);
		size = new ZPoint(1280, 720);
	}

	public void Init(Game g)
	{
		game = g;
		spriteBatch = new SpriteBatch(g.GraphicsDevice);

		universalTexture = new Texture2D(g.GraphicsDevice, 1, 1);
		Color[] colorArray = new Color[1];
		colorArray[0] = Color.White;
		universalTexture.SetData(colorArray);
	}

	public void LoadTextures()
	{
		editor = new Editor(size - new ZPoint(66, 62), new ZPoint(56, 52));
		dialogScreen = new DialogScreen(new ZPoint(200, 200), new ZPoint(600, 400));

		hexSelectionTexture = game.Content.Load<Texture2D>("other/hexSelection");
		ambientFont = game.Content.Load<SpriteFont>("fAmbient");
		dialogScreen.dialogFont = game.Content.Load<SpriteFont>("fDialog");
	}

	public void Draw(Vector2 mouse)
	{
		RPoint.Update(rMoves);
		foreach (RPoint p in rPoints) p.Update();

		World.Instance.Draw(mouse);

		HexPoint hexMouse = HexCoordinates(mouse);
        if (MyGame.Instance.gameState == MyGame.GameState.Global) spriteBatch.Draw(hexSelectionTexture, GraphicCoordinates(hexMouse));
		spriteBatch.DrawString(ambientFont, "Mouse: " + hexMouse, new Vector2(10, 10), Color.Red);
		spriteBatch.DrawString(ambientFont, "Party size: " + World.Instance.player.party.Count, new Vector2(10, 30), Color.Yellow);

		editor.Draw();
		dialogScreen.Draw();
	}

	public Vector2 ZeroGraphicCoordinates { get { return new Vector2(size.x * 0.5f, size.y * 0.5f); } }

	public Vector2 GraphicCoordinates(RPoint p)
	{
		Vector2 result = new Vector2();

		result.X = (p.x - World.Instance.camera.x) * 40;
		//result.Y = (p.y - World.Instance.camera.y) * 48 /*+ MyMath.IsOdd(p.x) * 24*/ - MyMath.IsOdd(World.Instance.camera.x) * 24;
		result.Y = (p.y - World.Instance.camera.y) * 48 + MyMath.SawFunction(p.x) * 24 - MyMath.IsOdd(World.Instance.camera.x) * 24;
		result = result + ZeroGraphicCoordinates - new Vector2(26, 24);

		return result;
	}

	public HexPoint HexCoordinates(Vector2 mouse)
	{
		HexPoint p = new HexPoint();
		p.x = (int)(0.025f * (mouse.X - ZeroGraphicCoordinates.X + 26.0f) + World.Instance.camera.x);
		p.y = (int)(0.0208333f * (mouse.Y - ZeroGraphicCoordinates.Y + 24.0f) + World.Instance.camera.y);

		float[] distances = new float[7];
		Vector2 shift = new Vector2(26, 24);
		distances[0] = (mouse - shift - GraphicCoordinates(p)).Length();
		distances[1] = (mouse - shift - GraphicCoordinates(p.Shift(HexPoint.HexDirection.N))).Length();
		distances[2] = (mouse - shift - GraphicCoordinates(p.Shift(HexPoint.HexDirection.NW))).Length();
		distances[3] = (mouse - shift - GraphicCoordinates(p.Shift(HexPoint.HexDirection.SW))).Length();
		distances[4] = (mouse - shift - GraphicCoordinates(p.Shift(HexPoint.HexDirection.S))).Length();
		distances[5] = (mouse - shift - GraphicCoordinates(p.Shift(HexPoint.HexDirection.SE))).Length();
		distances[6] = (mouse - shift - GraphicCoordinates(p.Shift(HexPoint.HexDirection.NE))).Length();

		int k = 0;
		float value = 100.0f;

		for (int i = 0; i < 7; i++)
		{
			if (distances[i] < value)
			{
				k = i;
				value = distances[i];
			}
		}

		if (k == 0) return p;
		else if (k == 1) return p.Shift(HexPoint.HexDirection.N);
		else if (k == 2) return p.Shift(HexPoint.HexDirection.NW);
		else if (k == 3) return p.Shift(HexPoint.HexDirection.SW);
		else if (k == 4) return p.Shift(HexPoint.HexDirection.S);
		else if (k == 5) return p.Shift(HexPoint.HexDirection.SE);
		else return p.Shift(HexPoint.HexDirection.NE);
	}
}

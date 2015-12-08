using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class MainScreen : Screen
{
	private static readonly MainScreen instance = new MainScreen();
	public static MainScreen Instance { get { return instance; } }

	public ZPoint windowPosition;
	public SpriteBatch spriteBatch;
	public Game game;

	public Editor editor;
	public DialogScreen dialogScreen;

	public Texture2D universalTexture, zSelectionTexture;
	private Texture2D hexSelectionTexture;
	
	public struct Fonts { public SpriteFont ambient, small, verySmall, superSmall, verdana, verdanaBold; }
	public Fonts fonts;

	private MyGame G { get { return MyGame.Instance; } }
	private World W { get { return World.Instance; } }

	public HexPoint Mouse { get { return HexCoordinates(G.mouseState.Position.ToVector2()); } }

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
		ZPoint dialogSize = new ZPoint(400, 200);
        dialogScreen = new DialogScreen(new ZPoint((int)(0.5f * (size.x - dialogSize.x)), size.y - 8 - dialogSize.y), dialogSize);

		hexSelectionTexture = game.Content.Load<Texture2D>("other/hexSelection");
		zSelectionTexture = game.Content.Load<Texture2D>("other/zSelection");

		fonts.ambient = game.Content.Load<SpriteFont>("fonts/ambient");
		fonts.small = game.Content.Load<SpriteFont>("fonts/small");
		fonts.verySmall = game.Content.Load<SpriteFont>("fonts/verySmall");
		fonts.superSmall = game.Content.Load<SpriteFont>("fonts/superSmall");
		fonts.verdana = game.Content.Load<SpriteFont>("fonts/verdana");
		fonts.verdanaBold = game.Content.Load<SpriteFont>("fonts/verdanaBold");
	}

	public void Draw()
	{
		W.Draw();

        if (!G.dialog && !G.battle && (!G.FOVEnabled || W.player[Mouse])) Draw(hexSelectionTexture, GraphicCoordinates(Mouse));
		if (G.debug) DrawString(fonts.ambient, "Mouse: " + Mouse, new ZPoint(10, 10), Color.Red);
		if (G.dndItem != null) Draw(G.dndItem.data.texture, G.Mouse - new ZPoint(16, 16));

		editor.Draw();
		dialogScreen.Draw();
		G.console.Draw(this);
	}

	public Vector2 ZeroGraphicCoordinates { get { return new Vector2(size.x * 0.5f, size.y * 0.5f); } }

	public Vector2 GraphicCoordinates(RPoint p)
	{
		Vector2 result = new Vector2();

		result.X = (p.x - World.Instance.camera.x) * 40;
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

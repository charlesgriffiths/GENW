using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class MainScreen : Screen
{
	private static readonly MainScreen instance = new MainScreen();
	public static MainScreen Instance { get { return instance; } }

	public Editor editor;
	public DialogScreen dialogScreen;

	private Texture2D hexSelectionTexture;
	private SpriteFont ambientFont;

	public enum GameState { Global, Local, Dialog };
	public GameState gameState;

	public MainScreen()
	{
		position = new ZPoint(200, 150);
		size = new ZPoint(1280, 720);
		gameState = GameState.Global;
	}

	public void LoadTextures(Game game)
	{
		editor = new Editor(size - new ZPoint(66, 62), new ZPoint(56, 52));
		dialogScreen = new DialogScreen(new ZPoint(200, 200), new ZPoint(600, 400));

		hexSelectionTexture = game.Content.Load<Texture2D>("hexSelection");
		ambientFont = game.Content.Load<SpriteFont>("fAmbient");
		dialogScreen.dialogFont = game.Content.Load<SpriteFont>("fDialog");
	}

	public void Draw(SpriteBatch sb, Vector2 mouse)
	{
		World.Instance.Draw(this, sb);

		HexPoint hexMouse = HexCoordinates(mouse);
        if (gameState == GameState.Global) sb.Draw(hexSelectionTexture, GraphicCoordinates(hexMouse));
		sb.DrawString(ambientFont, "Mouse: " + hexMouse, new Vector2(10, 10), Color.Red);
		sb.DrawString(ambientFont, "Party size: " + World.Instance.player.partySize, new Vector2(10, 30), Color.Yellow);

		editor.Draw(sb);
		dialogScreen.Draw(sb);
	}

	public Vector2 ZeroGraphicCoordinates { get { return new Vector2(size.x * 0.5f, size.y * 0.5f); } }

	public Vector2 GraphicCoordinates(HexPoint hexPoint)
	{
		Vector2 result = new Vector2();
		ZPoint camera = World.Instance.Camera;

		result.X = (hexPoint.x - camera.x) * 40;
		result.Y = (hexPoint.y - camera.y) * 48 + MyMath.IsOdd(hexPoint.x) * 24 - MyMath.IsOdd(camera.x) * 24;
		result = result + ZeroGraphicCoordinates - new Vector2(26, 24);

		return result;
	}

	public HexPoint HexCoordinates(Vector2 mouse)
	{
		HexPoint p = new HexPoint();
		p.x = (int)(0.025f * (mouse.X - ZeroGraphicCoordinates.X + 26.0f) + World.Instance.Camera.x);
		p.y = (int)(0.0208333f * (mouse.Y - ZeroGraphicCoordinates.Y + 24.0f) + World.Instance.Camera.y);

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

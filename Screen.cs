using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Screen
{
	private static readonly Screen instance = new Screen();
	public static Screen Instance { get { return instance; } }

	public ZPoint position = new ZPoint(200, 150);
	public ZPoint size = new ZPoint(1280, 720);
	public ZPoint viewRadius = new ZPoint(16, 8);

	public void Draw(SpriteBatch spriteBatch)
	{
		World.Instance.Draw(spriteBatch);
	}

	public Vector2 ZeroGraphicCoordinates { get { return new Vector2(size.x * 0.5f, size.y * 0.5f); } }

	public Vector2 GraphicCoordinates(HexPoint hexPoint, ZPoint camera)
	{
		Vector2 result = new Vector2();

		result.X = (hexPoint.x - camera.x) * 40;
		result.Y = (hexPoint.y - camera.y) * 48 + (hexPoint.x % 2) * 24 - (camera.x % 2) * 24;
		result = result + ZeroGraphicCoordinates - new Vector2(26, 24);

		return result;
	}
}

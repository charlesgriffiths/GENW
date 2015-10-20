using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Screen
{
	public ZPoint position, size;

	private MainScreen M { get { return MainScreen.Instance; } }

	public Screen()
	{
		ZPoint position = new ZPoint(10, 10);
		ZPoint size = new ZPoint(200, 100);
	}
	
	public Screen(ZPoint positioni, ZPoint sizei)
	{
		position = positioni;
		size = sizei;
	}

	public void DrawRectangle(ZPoint p, ZPoint s, Color color)
	{
		M.spriteBatch.Draw(M.universalTexture, new Rectangle(position.x + p.x, position.y + p.y, s.x, s.y), color);
	}

	public void Fill(Color color)
	{
		DrawRectangle(ZPoint.Zero, size, color);
	}

	public void Draw(Texture2D texture, ZPoint p, Color color)
	{
		MainScreen.Instance.spriteBatch.Draw(texture, position + p, color);
	}

	public void Draw(Texture2D texture, ZPoint p)
	{
		Draw(texture, p, Color.White);
	}

	public void Draw(Texture2D texture, Vector2 v) { Draw(texture, new ZPoint(v)); }

	public void DrawString(SpriteFont font, string text, ZPoint p, Color color)
	{
		MainScreen.Instance.spriteBatch.DrawString(font, text, position + p, color);
	}

	public void DrawString(SpriteFont font, string text, ZPoint p, Color color, int length)
	{
		MainScreen.Instance.spriteBatch.DrawString(font, MyMath.Split(text, length), position + p, color);
	}
}

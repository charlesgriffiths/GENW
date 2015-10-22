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
		ZPoint realPosition = p;
		ZPoint realSize = s;

		if (s.x < 0)
		{
			realPosition.x = p.x + s.x;
			realSize.x = -s.x;
		}
		if (s.y < 0)
		{
			realPosition.y = p.y + s.y;
			realSize.y = -s.y;
		}

		Rectangle rectangle = new Rectangle(position.x + realPosition.x, position.y + realPosition.y, realSize.x, realSize.y);
        M.spriteBatch.Draw(M.universalTexture, rectangle, color);
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Screen
{
	private Texture2D background;
	public ZPoint position, size;

	public Screen()
	{
		ZPoint position = new ZPoint(10, 10);
		ZPoint size = new ZPoint(200, 100);
	}
	
	public Screen(ZPoint positioni, ZPoint sizei, Color color)
	{
		position = positioni;
		size = sizei;

		//background = new Texture2D(BigBase.Instance.graphicsDevice, 1, 1);
		background = new Texture2D(BigBase.Instance.game.GraphicsDevice, 1, 1);
		Color[] c = new Color[1];
		c[0] = color;
		background.SetData(c);
	}

	public void Fill(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(background, destinationRectangle: new Rectangle(position.x, position.y, size.x, size.y));
	}

	public void Draw(Texture2D texture, ZPoint p, SpriteBatch spriteBatch, Color color)
	{
		spriteBatch.Draw(texture, position + p, color);
	}

	public void Draw(Texture2D texture, ZPoint p, SpriteBatch spriteBatch)
	{
		Draw(texture, p, spriteBatch, Color.White);
	}

	public void DrawString(SpriteFont font, string text, ZPoint p, Color color, SpriteBatch spriteBatch)
	{
		spriteBatch.DrawString(font, text, position + p, color);
	}

	public void DrawString(SpriteFont font, string text, ZPoint p, Color color, int length, SpriteBatch spriteBatch)
	{
		spriteBatch.DrawString(font, MyMath.Split(text, length), position + p, color);
	}
}

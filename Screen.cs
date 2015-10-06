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
	
	public Screen(ZPoint positioni, ZPoint sizei)
	{
		position = positioni;
		size = sizei;

		background = new Texture2D(BigBase.Instance.graphicsDevice, 1, 1);
		Color[] c = new Color[1];
		c[0] = Color.Black;
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
}

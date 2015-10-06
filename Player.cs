using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Player
{
	public HexPoint position = new HexPoint();
	public Texture2D texture;

	public void LoadTexture(Game game)
	{
		texture = game.Content.Load<Texture2D>("player");
	}

	public void Draw(MainScreen mainScreen, SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(texture, mainScreen.GraphicCoordinates(position) 
			- new Vector2(0.5f * texture.Width, 0.5f * texture.Height) + new Vector2(26, 24));
	}

	public void Move(HexPoint.HexDirection d)
	{
		ZPoint destination = position.Shift(d);
		if (World.Instance.map.IsWalkable(destination)) position = destination;
	}
}

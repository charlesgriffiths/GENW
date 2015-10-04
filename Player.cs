using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Player
{
	public HexPoint position;
	public Texture2D texture;

	public Player() { position = new HexPoint(20, 15); }

	public void LoadTexture(Game game)
	{
		texture = game.Content.Load<Texture2D>("player");
	}

	public void Draw(SpriteBatch spriteBatch, ZPoint camera)
	{
		//		spriteBatch.Draw(texture, Screen.Instance.ZeroGraphicCoordinates - new Vector2(0.5f * texture.Width, 0.5f * texture.Height));
		spriteBatch.Draw(texture, Screen.Instance.GraphicCoordinates(position, camera) - new Vector2(0.5f * texture.Width, 0.5f * texture.Height) + new Vector2(26, 24));
	}
}

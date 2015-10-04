using Microsoft.Xna.Framework.Graphics;

class World
{
	private static readonly World instance = new World();
	public static World Instance { get { return instance; } }
	
	public GlobalMap globalMap = new GlobalMap();
	public Player player = new Player();

	public void Init()
	{
		globalMap.Load();
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		ZPoint camera = player.position;
		ZPoint min = Screen.Instance.viewRadius;
		ZPoint max = globalMap.Size - Screen.Instance.viewRadius - new ZPoint(1, 0);
        camera = camera.Boundaries(min, max);

		globalMap.Draw(spriteBatch, camera);
		player.Draw(spriteBatch, camera);
	}
}

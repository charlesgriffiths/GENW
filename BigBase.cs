using Microsoft.Xna.Framework.Graphics;

class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }

	public GeneralBase<GlobalTile> globalTileBase = new GeneralBase<GlobalTile>();

	public GraphicsDevice graphicsDevice;

	public void Load(GraphicsDevice g)
	{
		GlobalTile.LoadBase();
		graphicsDevice = g;
	}
}

using Microsoft.Xna.Framework.Graphics;

class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }

	public GeneralBase<GTile> gTileBase = new GeneralBase<GTile>();
	public GeneralBase<Dialog> dialogBase = new GeneralBase<Dialog>();

	public GraphicsDevice graphicsDevice;

	public void Load(GraphicsDevice g)
	{
		//GTile.LoadBase();
		//Dialog.LoadBase();
		gTileBase.Load("gTiles.xml");
		dialogBase.Load("dialogs.xml");
		graphicsDevice = g;
	}
}

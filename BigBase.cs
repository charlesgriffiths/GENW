class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }

	public GeneralBase<GlobalTile> globalTileBase = new GeneralBase<GlobalTile>();

	public void Load()
	{
		GlobalTile.LoadBase();
	}
}

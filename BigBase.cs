class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }
	private BigBase() {}

	public GeneralBase<GTileType> gTileTypes = new GeneralBase<GTileType>();
	public GeneralBase<LTileType> lTileTypes = new GeneralBase<LTileType>();
	public GeneralBase<GTile> gTiles = new GeneralBase<GTile>();
	public GeneralBase<LTile> lTiles = new GeneralBase<LTile>();
	public GeneralBase<Palette> palettes = new GeneralBase<Palette>();
	public GeneralBase<Dialog> dialogs = new GeneralBase<Dialog>();
	public GeneralBase<CreatureShape> creatureShapes = new GeneralBase<CreatureShape>();

	public void Load()
	{
		Log.Write("loading all databases... ");

		gTileTypes.Load("gTileTypes.xml");
		lTileTypes.Load("lTileTypes.xml");
		gTiles.Load("gTiles.xml");
		lTiles.Load("lTiles.xml");
		palettes.Load("palettes.xml");
		dialogs.Load("dialogs.xml");
		creatureShapes.Load("creatureShapes.xml");

		Log.WriteLine("OK");
	}
}

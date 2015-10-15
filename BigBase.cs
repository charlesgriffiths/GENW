class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }

	public GeneralBase<GTile> gTiles = new GeneralBase<GTile>();
	public GeneralBase<LTile> lTiles = new GeneralBase<LTile>();
	public GeneralBase<Dialog> dialogs = new GeneralBase<Dialog>();
	public GeneralBase<CreatureShape> creatureShapes = new GeneralBase<CreatureShape>();

	//public Game game;

	public void Load()
	{
		gTiles.Load("gTiles.xml");
		lTiles.Load("lTiles.xml");
		dialogs.Load("dialogs.xml");
		creatureShapes.Load("creatureShapes.xml");
		//game = g;
	}
}

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

	public GeneralBase<GObjectShape> gShapes = new GeneralBase<GObjectShape>();
	public GeneralBase<Dialog> dialogs = new GeneralBase<Dialog>();

	public GeneralBase<Texture> textures = new GeneralBase<Texture>();
	public GeneralBase<CreepShape> creepShapes = new GeneralBase<CreepShape>();
	public GeneralBase<CharacterClass> classes = new GeneralBase<CharacterClass>();
	public GeneralBase<CharacterRace> races = new GeneralBase<CharacterRace>();

	public void Load()
	{
		Log.Write("loading all databases... ");

		gTileTypes.Load("gTileTypes.xml");
		lTileTypes.Load("lTileTypes.xml");
		gTiles.Load("gTiles.xml");
		lTiles.Load("lTiles.xml");
		palettes.Load("palettes.xml");

		gShapes.Load("gObjects.xml");
		dialogs.Load("dialogs.xml");

		textures.Load("textures.xml");
		creepShapes.Load("creeps.xml");
		classes.Load("classes.xml");
		races.Load("races.xml");

		Log.WriteLine("OK");
	}
}

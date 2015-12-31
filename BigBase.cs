public class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }
	private BigBase() {}

	public GeneralBase<GTileType> gTileTypes = new GeneralBase<GTileType>();
	public GeneralBase<LTileType> lTileTypes = new GeneralBase<LTileType>();
	public GeneralBase<GTile> gTiles = new GeneralBase<GTile>();
	public GeneralBase<LTile> lTiles = new GeneralBase<LTile>();
	public GeneralBase<Palette> palettes = new GeneralBase<Palette>();
	public GeneralBase<CComponent> ccomponents = new GeneralBase<CComponent>();

	public GeneralBase<Skill> skills = new GeneralBase<Skill>();
	public GeneralBase<EffectShape> effects = new GeneralBase<EffectShape>();
	public GeneralBase<CAbility> abilities = new GeneralBase<CAbility>();
	public GeneralBase<Ability> iAbilityTypes = new GeneralBase<Ability>();
	public GeneralBase<Origin> origins = new GeneralBase<Origin>();
	public GeneralBase<Background> backgrounds = new GeneralBase<Background>();

	public GeneralBase<GlobalShape> gShapes = new GeneralBase<GlobalShape>();
	public GeneralBase<Dialog> dialogs = new GeneralBase<Dialog>();

	public GeneralBase<Texture> textures = new GeneralBase<Texture>();
	public GeneralBase<LocalType> types = new GeneralBase<LocalType>();
	public GeneralBase<CreatureType> creatureTypes = new GeneralBase<CreatureType>();
	public GeneralBase<LocalShape> shapes = new GeneralBase<LocalShape>();

	public GeneralBase<Race> races = new GeneralBase<Race>();
	public GeneralBase<CClass> classes = new GeneralBase<CClass>();
	public GeneralBase<ItemShape> items = new GeneralBase<ItemShape>();
	
	public void Load()
	{
		Log.Write("loading all databases... ");

		gTileTypes.Load("gTileTypes.xml");
		lTileTypes.Load("lTileTypes.xml");
		gTiles.Load("gTiles.xml");
		lTiles.Load("lTiles.xml");
		palettes.Load("palettes.xml");
		ccomponents.Load("ccomponents.xml");

		skills.Load("skills.xml");
		effects.Load("effects.xml");
		abilities.Load("abilities.xml");
		iAbilityTypes.Load("iAbilityTypes.xml");
		origins.Load("origins.xml");
		backgrounds.Load("backgrounds.xml");

		dialogs.Load("dialogs.xml");
		gShapes.Load("gObjects.xml");

		textures.Load("textures.xml");
		types.Load("types.xml");
		creatureTypes.Load("creatureTypes.xml");
		shapes.Load("localShapes.xml");

		races.Load("races.xml");
		classes.Load("classes.xml");
		items.Load("items.xml");
		
		Log.WriteLine("OK");
	}
}

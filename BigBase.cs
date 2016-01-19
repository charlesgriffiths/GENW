public class BigBase
{
	private static readonly BigBase instance = new BigBase();
	public static BigBase Instance { get { return instance; } }
	private BigBase() {}

	public GeneralBase<NamedTexture> namedTextures = new GeneralBase<NamedTexture>();
	public GeneralBase<GlobalTileType> globalTileTypes = new GeneralBase<GlobalTileType>();
	public GeneralBase<LocalTileType> localTileTypes = new GeneralBase<LocalTileType>();

	public GeneralBase<Skill> skills = new GeneralBase<Skill>();
	public GeneralBase<EffectShape> effects = new GeneralBase<EffectShape>();
	public GeneralBase<ClassAbility> abilities = new GeneralBase<ClassAbility>();
	public GeneralBase<Ability> iAbilityTypes = new GeneralBase<Ability>();
	public GeneralBase<Origin> origins = new GeneralBase<Origin>();
	public GeneralBase<Background> backgrounds = new GeneralBase<Background>();

	public GeneralBase<GlobalShape> gShapes = new GeneralBase<GlobalShape>();
	public GeneralBase<Dialog> dialogs = new GeneralBase<Dialog>();

	public GeneralBase<LocalType> types = new GeneralBase<LocalType>();
	public GeneralBase<CreatureType> creatureTypes = new GeneralBase<CreatureType>();
	public GeneralBase<Race> races = new GeneralBase<Race>();
	public GeneralBase<CharacterClass> classes = new GeneralBase<CharacterClass>();
	public GeneralBase<CraftingComponent> components = new GeneralBase<CraftingComponent>();
	public GeneralBase<ItemShape> items = new GeneralBase<ItemShape>();
	public GeneralBase<LocalShape> shapes = new GeneralBase<LocalShape>();
	public GeneralBase<LocalTile> localTiles = new GeneralBase<LocalTile>();
	public GeneralBase<Palette> palettes = new GeneralBase<Palette>();
	public GeneralBase<GlobalTile> globalTiles = new GeneralBase<GlobalTile>();

	public void Load()
	{
		Log.Write("loading all databases... ");

		namedTextures.Load("textures.xml");
		globalTileTypes.Load("globalTileTypes.xml");
		localTileTypes.Load("localTileTypes.xml");

		skills.Load("skills.xml");
		effects.Load("effects.xml");
		abilities.Load("abilities.xml");
		iAbilityTypes.Load("iAbilityTypes.xml");
		origins.Load("origins.xml");
		backgrounds.Load("backgrounds.xml");

		dialogs.Load("dialogs.xml");
		gShapes.Load("gObjects.xml");

		types.Load("types.xml");
		creatureTypes.Load("creatureTypes.xml");
		races.Load("races.xml");
		classes.Load("classes.xml");
		components.Load("ccomponents.xml");
		items.Load("items.xml");
		shapes.Load("localShapes.xml");
		localTiles.Load("localTiles.xml");
		palettes.Load("palettes.xml");
		globalTiles.Load("globalTiles.xml");

		Log.WriteLine("OK");
	}
}

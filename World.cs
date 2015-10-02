class World
{
	private static readonly World instance = new World();
	public static World Instance { get { return instance; } }
	
	public GlobalMap globalMap = new GlobalMap();

	public void Init()
	{
		globalMap.Load();
	}
}

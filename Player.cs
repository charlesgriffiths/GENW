using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public partial class Player : GlobalObject
{
	public Inventory crafting = new Inventory(6, 1, "CRAFTING", true);
	public Inventory ground = new Inventory(6, 2, "GROUND", true);
	private Inventory toSell = new Inventory(12, 1, "SELL", true);
	private Inventory toBuy = new Inventory(12, 1, "BUY", false);
	public GlobalObject barter = null;

	public Dictionary<ItemShape, int> craftableShapes = new Dictionary<ItemShape, int>();

	public bool[,] visitedLocations;
	private List<GlobalObject> visibleObjects = new List<GlobalObject>();

	public Player()
	{
		shape = new GlobalShape();
		shape.name = "Karl";
		shape.speed = 1.0f;
		shape.isActive = true;
		uniqueName = shape.name;

		inventory.isInParty = true;
		inventory.globalOwner = this;

		LocalObject c1 = new LocalObject(shape.name, Race.Get("Human"), CharacterClass.Get("Fighter"), 
			Background.Get("Peasant"), Origin.Get("Eden"), 0);
		c1.inventory.Add("Staff");
		party.Add(c1);

		//LocalObject c2 = new LocalObject("Nicolas", Race.Get("Floran"), CharacterClass.Get("Seer"), Background.Get("Hunter"), Origin.Get("Eden"), 140);
		//party.Add(c2);

		party.Add(new LocalObject(LocalShape.Get("Krokar"), "Boo-Boo"));

		inventory.Add("Inperium Coins", 4);
		inventory.Add("Alliance Coins", 10);
		inventory.Add("Bottle of Water", 2);
		inventory.Add("King Bolete", 3);
		inventory.Add("Potato", 5);
	}

	public override Texture2D Texture { get	{ return NamedTexture.Get("other/player"); } }

	public override void Kill()
	{
		Log.WriteLine("Game Over!");
		MainScreen.Instance.game.Exit();
	}

	public override void ProcessCollisions(GlobalObject g)
	{
		if (g.dialog != null) M.dialogScreen.StartDialog(g.dialog, g);
	}

	public override void Move(HexPoint.HexDirection d)
	{
		base.Move(d);

		foreach (LocalObject c in party) c.fatigue.Add(W.map[position.Shift(d)].type.travelTime * c.hp.Max);

		ground.Clear();
		crafting.Clear();

		foreach (ItemShape s in W.map[position].items) if (W.random.Next(50) <= Sum(Skill.Get("Survival")))	ground.Add(s);

		if (barter != null)
		{
			foreach (Item item in toBuy.Items) if (barter.inventory.CanAdd(item)) barter.inventory.Add(item);
			foreach (Item item in toSell.Items) if (inventory.CanAdd(item)) inventory.Add(item);

			toBuy.Clear();
			toSell.Clear();
			barter = null;
		}

		UpdateVisitedLocations();
	}

	private bool NewObjectsVisible()
	{
		List<GlobalObject> newVisibleObjects = (from o in W.gObjects where W.map.IsInView(position, o.position) select o).ToList();
		List<GlobalObject> query = (from o in newVisibleObjects where !visibleObjects.Contains(o) select o).ToList();
		visibleObjects = newVisibleObjects;
		return query.Count > 0;
	}

	public override void Run(){}

	public bool this[ZPoint p]
	{
		get	{ return W.map.InRange(p) ? visitedLocations[p.x, p.y] : false;	}
		set	{ if (W.map.InRange(p)) visitedLocations[p.x, p.y] = value;	}
	}

	public void UpdateVisitedLocations()
	{
		if (visitedLocations == null)
		{
			visitedLocations = new bool[W.map.Size.x, W.map.Size.y];
			for (int j = 0; j < W.map.Size.y; j++)
				for (int i = 0; i < W.map.Size.x; i++)
					visitedLocations[i, j] = false;

			//OpenMap();
		}

		for (int j = -2; j <= 2; j++) for (int i = -2; i <=2; i++)
		{
				ZPoint p = position + new ZPoint(i, j);
				if (W.map.IsInView(position, p)) this[p] = true;
		}

		ZPoint pvr = new ZPoint(3, 3);
		if (!((ZPoint)position).InBoundaries(W.camera - W.viewRadius + pvr + new ZPoint(5, 0), W.camera + W.viewRadius - pvr))
			W.camera = position;
	}

	public void UpdateCrafting()
	{
		craftableShapes.Clear();
		var all = BigBase.Instance.items.data;

		Func<ItemShape, bool> hasAbilities = s => {
			foreach (CraftingComponent cc in s.MultilessComponents) if (!HasAbility(cc.requirement)) return false;
			return true; };

		foreach (ItemShape itemShape in all.Where(s => s.IsComposable(crafting.CComponents) && !crafting.Contains(s) && 
		s.CraftingComplexity <= Max(Skill.Get("Crafting")) && s.isCraftable && hasAbilities(s)))
			craftableShapes.Add(itemShape, 0);
	}

	public void OpenMap() {	for (int j = 0; j < W.map.Size.y; j++) for (int i = 0; i < W.map.Size.x; i++) visitedLocations[i, j] = true; }
}

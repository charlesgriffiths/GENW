﻿using System.Linq;
using System.Collections.Generic;

public partial class Player : GObject
{
	public Inventory crafting = new Inventory(6, 1, null, "CRAFTING", true);
	public Inventory ground = new Inventory(6, 2, null, "GROUND", true);
	private Inventory toSell = new Inventory(12, 1, null, "SELL", true);
	private Inventory toBuy = new Inventory(12, 1, null, "BUY", false);
	public GObject barter = null;

	public Dictionary<ItemShape, int> craftableShapes = new Dictionary<ItemShape, int>();

	public bool[,] visitedLocations;
	private List<GObject> visibleObjects = new List<GObject>();

	public Player()
	{
		shape = new GObjectShape();
		shape.name = "Nicolas";
		shape.speed = 1.0f;
		shape.isActive = true;
		uniqueName = shape.name;
		inventory.isInParty = true;

		LocalObject c1 = new LocalObject(shape.name, Race.Get("Human"), CClass.Get("Alchemist"), Background.Get("Merchant"), Origin.Get("Iron Alliance"));
		c1.inventory.Add("Club");
		c1.inventory.Add("Leather Armor");
		party.Add(c1);

		LocalObject c2 = new LocalObject("Karl", Race.Get("Floran"), CClass.Get("Ranger"), Background.Get("Hunter"), Origin.Get("Eden"));
		c2.inventory.Add("Iron Dagger");
		party.Add(c2);

		//party.Add(new Creep("Krokar"));
		party.Add(new LocalObject(LocalShape.Get("Krokar")));

		inventory.Add("Net");
		inventory.Add("Rope");
		inventory.Add("Inperium Coins", 5);
		inventory.Add("Alliance Coins", 7);
		inventory.Add("Longbow");
		inventory.Add("Pickaxe", 3);
		inventory.Add("Buckler");
		inventory.Add("Banana", 4);
		inventory.Add("Meat", 2);
		inventory.Add("Deformed Banana", 4);
		inventory.Add("Empty Bottle");
		inventory.Add("Bottle of Water");
		inventory.Add("Paralyzing Poison");
		inventory.Add("Flashbang");
		inventory.Add("Nourishing Mix");
		inventory.Add("Vatra Mushroom", 2);
	}

	public override void Kill()
	{
		Log.WriteLine("Game Over!");
		MainScreen.Instance.game.Exit();
	}

	public override void ProcessCollisions(GObject g)
	{
		if (g.dialog != null) M.dialogScreen.StartDialog(g.dialog, g);
	}

	public override void Move(HexPoint.HexDirection d)
	{
		base.Move(d);

		foreach (LocalObject c in party) c.fatigue.Add(W.map[position.Shift(d)].type.travelTime * c.hp.Max);

		ground.Clear();
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
		List<GObject> newVisibleObjects = (from o in W.gObjects where W.map.IsInView(position, o.position) select o).Cast<GObject>().ToList();
		List<GObject> query = (from o in newVisibleObjects where !visibleObjects.Contains(o) select o).Cast<GObject>().ToList();
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

		foreach (ItemShape itemShape in all.Where(s => s.IsComposable(crafting.CComponents) && !crafting.Contains(s) && 
		(s.craftLevel <= Max(Skill.Get("Crafting")) || (s.craftLevel == 101 && HasAbility("More Recipes")))))
			craftableShapes.Add(itemShape, 0);
	}

	public void OpenMap() {	for (int j = 0; j < W.map.Size.y; j++) for (int i = 0; i < W.map.Size.x; i++) visitedLocations[i, j] = true; }
}

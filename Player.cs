using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public partial class Player : GObject
{
	private Inventory crafting = new Inventory(6, null, "Crafting");
	private Inventory ground = new Inventory(12, null, "Ground");

	public bool[,] visitedLocations;
	private List<GObject> visibleObjects = new List<GObject>();

	public Player()
	{
		shape = new GObjectShape();
		shape.name = "Karl";
		shape.speed = 1.0f;
		shape.isActive = true;
		uniqueName = shape.name;

		Character playerCharacter = new Character(shape.name, "Floran", "Assassin", "Eden", "Soldier");
		playerCharacter.inventory.Add("Club");
		playerCharacter.inventory.Add("Leather Armor");
		party.Add(playerCharacter);

		Character c2 = new Character("Bob", "Vorcha", "Alchemist", "The Scorch", "Pitfighter");
		c2.inventory.Add("Dagger");
		c2.inventory.Add("Force Staff");
		party.Add(c2);

		//party.Add(new Creep("Krokar"));

		inventory.Add("Net");
		inventory.Add("Rope");
		inventory.Add("Inperium Coins", 5);
		inventory.Add("Alliance Coins", 7);
		inventory.Add("Longbow", 2);
		inventory.Add("Pickaxe", 3);
		inventory.Add("Buckler");
		inventory.Add("Chainmail");
		inventory.Add("Banana", 4);
		inventory.Add("Corpse Meat", 2);
	}

	public void LoadTextures()
	{
		Texture = M.game.Content.Load<Texture2D>("other/player");
	}

	public void DrawParty(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(1, 1));
		int vOffset = 0;

		foreach (Creature member in party)
		{
			//int i = party.IndexOf(member);
			Screen icon = new Screen(position + new ZPoint(0, vOffset), new ZPoint(32, 32));

			icon.Draw(member.texture, ZPoint.Zero);
			icon.DrawString(M.fonts.verySmall, member.stamina.ToString() + "/" + member.hp + "/" + member.MaxHP, 32, Color.White);

			float hpMissing = 1 - (float)member.hp / member.MaxHP;
			float staminaMissing = 1 - (float)member.stamina / member.MaxHP;

			icon.DrawRectangle(new ZPoint(0, 32), new ZPoint(32, -(int)(staminaMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));
			icon.DrawRectangle(new ZPoint(0, 32), new ZPoint(32, -(int)(hpMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));

			vOffset += 32 + 16;
		}

		var characters = from c in party where c is Character select c;
		foreach (Character c in characters) c.inventory.Draw(position + new ZPoint(40, party.IndexOf(c) * 40));

		vOffset = (from c in party where c is Character select c).Count() * 40;
		inventory.Draw(position + new ZPoint(40, vOffset));
		crafting.Draw(position + new ZPoint(40, vOffset + 256));
		ground.Draw(position + new ZPoint(40, vOffset + 256 + 32 + 16));
	}

	public override void Draw()
	{
		movementAnimations.Draw();
		M.spriteBatch.Draw(Texture, M.GraphicCoordinates(rPosition));
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

		foreach (Creature c in party) c.AddFatigue(W.map[position.Shift(d)].type.travelTime);
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
		get
		{
			if (W.map.InRange(p)) return visitedLocations[p.x, p.y];
			else return false;
		}
		set
		{
			if (W.map.InRange(p)) visitedLocations[p.x, p.y] = value;
		}
	}

	public void UpdateVisitedLocations()
	{
		if (visitedLocations == null)
		{
			visitedLocations = new bool[W.map.Size.x, W.map.Size.y];
			for (int j = 0; j < W.map.Size.y; j++)
				for (int i = 0; i < W.map.Size.x; i++)
					visitedLocations[i, j] = false;
		}

		for (int j=-2; j <= 2; j++) for (int i=-2; i <=2; i++)
		{
				ZPoint p = position + new ZPoint(i, j);
				if (W.map.IsInView(position, p)) this[p] = true;
		}

		ZPoint pvr = new ZPoint(3, 3);
		if (!((ZPoint)position).InBoundaries(W.camera - W.viewRadius + pvr, W.camera + W.viewRadius - pvr))	W.camera = position;
	}
}

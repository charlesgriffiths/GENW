using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public abstract class LocalComponent
{
	protected LocalObject t;

	protected static Battlefield B { get { return World.Instance.battlefield; } }
	protected static MainScreen M { get { return MainScreen.Instance; } }
	protected static Random R { get { return World.Instance.random; } }

	public LocalComponent(LocalObject o) { t = o; }
}

public class LocalObject
{
	public LocalDrawing drawing;
	public LocalPosition p;

	public Initiative initiative;
	public Team team;
	public Effects effects;

	public Movement movement;
	public HPComponent hp;
	public Defence defence;
	public Attack attack;
	public Abilities abilities;
	public Fatigue fatigue;
	public Eating eating;

	public ShapeComponent shape;
	public Item item;

	public Inventory inventory;
	public Race race;
	public CharacterClass cclass;
	public Origin origin;
	public Background background;
	public Skills skills;
	public Experience xp;

	public string uniqueName = "";

	public string TypeName
	{
		get
		{
			if (cclass != null) return race.name + " " + cclass.name + ", " + xp.Level;
			else if (item != null) return item.data.name;
			else return shape.data.name;
		}
	}

	public string FullName { get { return uniqueName != "" ? uniqueName + ", " + TypeName : TypeName; } }
	public string CommonName { get { return uniqueName != "" ? uniqueName : TypeName; } }

	public Color LogColor {	get	{ return team != null ? team.isInParty ? Color.White : Color.Orange : Color.DarkGray; } }
	public Color RelationshipColor { get { return team != null ? team.isInParty ? Color.Green : Color.Red : Color.DodgerBlue; } }

	public int Importance
	{
		get
		{
			if (uniqueName == World.Instance.player.uniqueName) return 1;
			else if (GetCreatureType != null && hp != null) return cclass != null ? 2 : 3;
			else if (item != null) return 4;
			else if (inventory != null) return 5;
			else return 6;
		}
	}

	public bool HasEffect(params string[] names) { return effects != null ? effects.HasOne(names) : false; }
	public bool HasAbility(string name) { return abilities != null ? abilities.Has(name) : false; }
	public void RemoveEffect(params string[] names) { if (effects != null) effects.RemoveAll(names); }

	public Texture2D GetTexture
	{
		get
		{
			if (item != null) return item.data.texture;
			else if (shape != null) return shape.GetTexture;
			else return cclass.textures[race];
		}
	}

	public void DrawInfo(ZPoint position)
	{
		MainScreen M = MainScreen.Instance;
		Screen screen = new Screen(position, new ZPoint(192, 1));
		screen.DrawString(M.fonts.verdanaBold, FullName, ZPoint.Zero, Color.White);

		if (cclass != null)
		{
			screen.DrawString(M.fonts.small, "(" + background.name + ", " + origin.name + ")", new ZPoint(0, screen.offset), Color.White);
			screen.offset += 8;

			foreach (Skill skill in BigBase.Instance.skills.data)
			{
				int previousOffset = screen.offset;
				screen.DrawString(M.fonts.small, skill.name, new ZPoint(0, screen.offset), Color.White);
				int trueOffset = screen.offset;
				screen.DrawString(M.fonts.small, skills[skill].ToString(), new ZPoint(100, previousOffset), Color.White);
				screen.offset = trueOffset;
			}
		}
	}

	public CreatureType GetCreatureType
	{
		get
		{
			if (shape != null) return shape.data.creatureType;
			else if (cclass != null) return BigBase.Instance.creatureTypes.Get("Sentient");
			else return null;
		}
	}

	public LocalObject(Item _item)
	{
		item = _item;
	}

	public LocalObject(LocalShape localShape, string _uniqueName = "", Inventory _inventory = null)
	{
		uniqueName = _uniqueName;
		shape = new ShapeComponent(localShape, this);

		if (localShape.type == LocalType.Get("Destructible") || localShape.type == LocalType.Get("Container"))
		{
			hp = new HPComponent(this);
		}
		else if (localShape.type == LocalType.Get("Creature"))
		{
			hp = new HPComponent(this);
			movement = new Movement(this);
			defence = new Defence(this);
			attack = new Attack(this);
			abilities = new Abilities(this);
			fatigue = new Fatigue(this);
			eating = new Eating(this);
		}

		if (_inventory != null)
		{
			inventory = new Inventory(6, 1, "", false, null, this);
			_inventory.MoveTo(inventory);
		}
	}

	public LocalObject(string _uniqueName, Race _race, CharacterClass _cclass, Background _background, Origin _origin, int experience)
	{
		uniqueName = _uniqueName;
		race = _race;
		cclass = _cclass;
		background = _background;
		origin = _origin;

		skills = new Skills(this);
		inventory = new Inventory(6, 1, "", true, null, this);

		hp = new HPComponent(this);
		movement = new Movement(this);
		defence = new Defence(this);
		attack = new Attack(this);
		abilities = new Abilities(this);
		fatigue = new Fatigue(this);
		eating = new Eating(this);

		xp = new Experience(experience, this);
	}
}
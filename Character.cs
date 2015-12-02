using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Character : Creature
{
	public Inventory inventory;

	public Race race;
	public CClass cClass;
	public Origin origin;
	public Background background;

	public int xp;

	public override string Name { get { return race.name + " " + cClass.name; } }
	public override CreepType creepType { get {	return BigBase.Instance.creepTypes.Get("Sentient");	} }

	private int InventorySum(Func<Bonus, int> func)	{ return (from i in inventory.Items select func(i.data.bonus)).Sum(); }

	public override int MaxHP { get { return 10 + 4 * this["Endurance"] + InventorySum(b => b.hp); } }
	public override int Damage { get { return 1 + this["Strength"] + InventorySum(b => b.damage); } }
	public override int Attack { get { return this["Agility"] + InventorySum(b => b.attack); } }
	public override int Defence { get { return this["Agility"] + InventorySum(b => b.defence); } }
	public override int Armor { get { return InventorySum(b => b.armor); } }

	public int this[string skillName]
	{
		get
		{
			Skill skill = BigBase.Instance.skills.Get(skillName);
			return (int)(race.bonus.skills[skill] + origin.bonus.skills[skill] + background.bonus.skills[skill]);
		}
	}

	public override List<Ability> Abilities
	{
		get
		{
			List<Ability> result = new List<Ability>();
			result.Add(race.ability);
			foreach (Ability a in cClass.abilities) result.Add(a);
			return result;
		}
	}

	public override int Importance { get { return IsAlive ? uniqueName == World.Instance.player.uniqueName ? 1 : 2 : 4; } }

	public Character(string uniqueNamei, string raceName, string className, string originName, string backgroundName)
	{
		BigBase b = BigBase.Instance;

		uniqueName = uniqueNamei;
		inventory = new Inventory(6, this);

		race = b.races.Get(raceName);
		cClass = b.classes.Get(className);
		origin = b.origins.Get(originName);
		background = b.backgrounds.Get(backgroundName);

		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + Name);

		hp = MaxHP;
		stamina = MaxHP;
	}
}
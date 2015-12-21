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

	//public int xp;

	public override string Name { get { return race.name + " " + cClass.name; } }
	public override CreepType creepType { get {	return BigBase.Instance.creepTypes.Get("Sentient");	} }

	private int InventorySum(Func<Bonus, int> func)	{ return (from i in inventory.Items select func(i.data.bonus)).Sum(); }
	
	public override int MaxHP { get { return 10 + 4 * this[Skill.Get("Endurance")] + InventorySum(b => b.hp); } }
	public override int Damage { get { return 1 + this[Skill.Get("Strength")] + InventorySum(b => b.damage); } }
	public override int Attack { get { return this[Skill.Get("Agility")] + InventorySum(b => b.attack); } }
	public override int Defence { get { return this[Skill.Get("Agility")] + InventorySum(b => b.defence); } }
	public override int Armor { get { return InventorySum(b => b.armor); } }

	public int this[Skill skill] { get {
		return (int)(race.bonus.skills[skill] + origin.bonus.skills[skill] + background.bonus.skills[skill]); } }
	public int this[string skillName] { get { return this[Skill.Get(skillName)]; } }

	public override List<CAbility> Abilities
	{
		get
		{
			List<CAbility> result = new List<CAbility>();
			result.Add(race.ability);
			foreach (CAbility a in cClass.abilities) result.Add(a);
			return result;
		}
	}

	public override int Importance { get { return IsAlive ? uniqueName == World.Instance.player.uniqueName ? 1 : 2 : 4; } }

	public Character(string uniqueNamei, string raceName, string className, string originName, string backgroundName)
	{
		BigBase b = BigBase.Instance;

		uniqueName = uniqueNamei;
		inventory = new Inventory(6, 1, this, "");

		race = b.races.Get(raceName);
		cClass = b.classes.Get(className);
		origin = b.origins.Get(originName);
		background = b.backgrounds.Get(backgroundName);

		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + Name);

		hp = MaxHP;
		stamina = MaxHP;
	}

	public override void AddFatigue(float value)
	{
		fatigue += value * 0.1f * Math.Max(10 - this["Survival"], 0);
		UpdateFatigue();
	}
}
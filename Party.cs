using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public abstract class PartyCreature
{
	public Texture2D texture;
	public string uniqueName;
	public int endurance, hp;

	public virtual string Name { get { return ""; } }

	public virtual int MaxHP { get { return 10; } }
	public virtual int Damage { get { return 1; } }
	public virtual int Attack { get { return 0; } }
	public virtual int Defence { get { return 0; } }

	public virtual List<Ability> Abilities { get { return new List<Ability>(); } }
}

class PartyCreep : PartyCreature
{
	public CreepShape shape;

	public PartyCreep(string namei, string uniqueNamei = "")
	{
		uniqueName = uniqueNamei;
		shape = BigBase.Instance.creepShapes.Get(namei);
		hp = MaxHP;
		endurance = MaxHP;
		texture = shape.texture;
	}

	public override string Name { get { return shape.name; } }

	public override int MaxHP {	get	{ return shape.maxHP; }	}

	public override List<Ability> Abilities {	get	{ return shape.abilities; } }
}

class PartyCharacter : PartyCreature
{
	public Gift gift;
	public Race race;
	public CClass cClass;
	public Origin origin;
	public Background background;

	public override string Name { get { return race.name + " " + cClass.name; } }

	public override int MaxHP { get { return 10 + gift.bonus.hp + race.bonus.hp; } }
	public override int Damage { get { return 1 + gift.bonus.damage + race.bonus.damage; } }
	public override int Attack { get { return gift.bonus.attack + race.bonus.attack; } }
	public override int Defence { get {	return gift.bonus.defence + race.bonus.defence; } }

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

	public PartyCharacter(string uniqueNamei, string giftName, string raceName, string className, string originName, string backgroundName)
	{
		BigBase b = BigBase.Instance;

		uniqueName = uniqueNamei;
		//name = raceName + " " + className;

		gift = b.gifts.Get(giftName);
		race = b.races.Get(raceName);
		cClass = b.classes.Get(className);
		origin = b.origins.Get(originName);
		background = b.backgrounds.Get(backgroundName);

		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + Name);

		hp = MaxHP;
		endurance = MaxHP;
    }
}

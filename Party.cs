using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

abstract class PartyCreature
{
	public Texture2D texture;
	public string name, uniqueName;
	public int hp;

	public virtual int MaxHP { get { return 10; } }
	public virtual int Damage { get { return 1; } }
	public virtual int Attack { get { return 0; } }
	public virtual int Defence { get { return 0; } }

	public virtual Collection<Ability> Abilities { get { return new Collection<Ability>(); } }
}

class PartyCreep : PartyCreature
{
	public CreepShape shape;

	public PartyCreep(string namei, string uniqueNamei = "")
	{
		name = namei;
		uniqueName = uniqueNamei;
		shape = BigBase.Instance.creepShapes.Get(name);
		hp = MaxHP;
		texture = shape.texture;
	}

	public override int MaxHP {	get	{ return shape.maxHP; }	}
}

class PartyCharacter : PartyCreature
{
	public Gift gift;
	public Race race;
	public CClass cClass;
	public Origin origin;
	public Background background;

	public override int MaxHP { get { return 10 + gift.bonus.hp + race.bonus.hp; } }
	public override int Damage { get { return 1 + gift.bonus.damage + race.bonus.damage; } }
	public override int Attack { get { return gift.bonus.attack + race.bonus.attack; } }
	public override int Defence { get {	return gift.bonus.defence + race.bonus.defence; } }

	public override Collection<Ability> Abilities
	{
		get
		{
			Collection<Ability> result = new Collection<Ability>();
			result.Add(race.ability);
			foreach (Ability a in cClass.abilities) result.Add(a);
			return result;
		}
	}

	public PartyCharacter(string uniqueNamei, string giftName, string raceName, string className, string originName, string backgroundName)
	{
		BigBase b = BigBase.Instance;

		uniqueName = uniqueNamei;
		name = raceName + " " + className;

		gift = b.gifts.Get(giftName);
		race = b.races.Get(raceName);
		cClass = b.classes.Get(className);
		origin = b.origins.Get(originName);
		background = b.backgrounds.Get(backgroundName);

		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + name);
		hp = MaxHP;
    }
}

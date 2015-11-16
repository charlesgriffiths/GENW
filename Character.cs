using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class Character : Creature
{
	public Inventory inventory;

	public Gift gift;
	public Race race;
	public CClass cClass;
	public Origin origin;
	public Background background;

	public override string Name { get { return race.name + " " + cClass.name; } }

	public override int MaxHP { get { return 10 + gift.bonus.hp + race.bonus.hp /*+ (from i in inventory.Items select i.data.bonus.hp).Sum()*/; } }
	public override int Damage { get { return 1 + gift.bonus.damage + race.bonus.damage /*+ (from i in inventory.Items select i.data.bonus.damage).Sum()*/; } }
	public override int Attack { get { return gift.bonus.attack + race.bonus.attack /*+ (from i in inventory.Items select i.data.bonus.attack).Sum()*/; } }
	public override int Defence { get { return gift.bonus.defence + race.bonus.defence /*+ (from i in inventory.Items select i.data.bonus.defence).Sum()*/; } }

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

	public Character(string uniqueNamei, string giftName, string raceName, string className, string originName, string backgroundName)
	{
		BigBase b = BigBase.Instance;

		uniqueName = uniqueNamei;

		gift = b.gifts.Get(giftName);
		race = b.races.Get(raceName);
		cClass = b.classes.Get(className);
		origin = b.origins.Get(originName);
		background = b.backgrounds.Get(backgroundName);

		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + Name);

		hp = MaxHP;
		endurance = MaxHP;

		inventory = new Inventory(6, this);
	}
}

class LCharacter : LCreature
{
	public override float MovementSpeed { get { return 1.0f; } }
	public override float AttackSpeed { get { return 3.0f; } }

	public override int Damage { get { return Data.Damage; } }
	public override int MaxHP { get { return Data.MaxHP; } }

	public override int Importance { get { return 2; } }

	public Character Data { get { return data as Character; } }

	public LCharacter() {}
	public LCharacter(Character character, bool isInPartyi, bool isAIControlledi)
	{
		data = character;
		isInParty = isInPartyi;
		isAIControlled = isAIControlledi;
		texture = character.texture;
		Init();
	}

	protected override void Init()
	{
		base.Init();
		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" +
			Data.race.name + " " + Data.cClass.name);
	}
}

class LPlayer : LCharacter
{
	public override int Importance { get { return 1; } }

	public LPlayer(Character character)
	{
		data = character;
		isInParty = true;
		isAIControlled = false;
		texture = character.texture;
		Init();
	}

	public override void Kill()
	{
		World.Instance.player.Kill();
		base.Kill();
	}
}
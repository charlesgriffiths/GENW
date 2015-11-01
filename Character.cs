using Microsoft.Xna.Framework.Graphics;

class Character : Creature
{
	public override float MovementSpeed { get { return 1.0f; } }
	public override float AttackSpeed { get { return 3.0f; } }

	public override int Damage { get { return Member.Damage; } }
	public override int MaxHP { get { return Member.MaxHP; } }

	public override int Importance { get { return 2; } }

	public PartyCharacter Member { get { return partyCreature as PartyCharacter; } }

	public Character() {}
	public Character(PartyCharacter pc, bool isInPartyi, bool isAIControlledi)
	{
		partyCreature = pc;
		//name = pc.uniqueName;
		isInParty = isInPartyi;
		isAIControlled = isAIControlledi;
		texture = pc.texture;
		Init();
	}

	protected override void Init()
	{
		base.Init();
		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" +
			Member.race.name + " " + Member.cClass.name);
	}
}

class LPlayer : Character
{
	public override int Importance { get { return 1; } }

	public LPlayer(PartyCharacter pc)
	{
		partyCreature = pc;
		//name = pc.uniqueName;
		isInParty = true;
		isAIControlled = false;
		texture = pc.texture;
		Init();
	}

	public override void Kill()
	{
		World.Instance.player.Kill();
		base.Kill();
	}
}
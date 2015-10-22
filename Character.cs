using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class CharacterRace : NamedObject
{
	public CharacterClass defaultClass;
	//public Ability raceAbility;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		defaultClass = BigBase.Instance.classes.Get(MyXml.GetString(xnode, "defaultClass"));
	}
}

class CharacterClass : NamedObject
{
	//public Ability classAbility;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
	}
}

class Character : Creature
{
	//protected PartyCharacter partyCharacter;

	public override float MovementSpeed { get { return 0.2f; } }
	public override float AttackSpeed { get { return 0.2f; } }
	public override int Damage { get { return 10; } }
	public override int MaxHP { get { return Member.MaxHP; } }

	public override int Importance { get { return 2; } }

	public PartyCharacter Member { get { return partyCreature as PartyCharacter; } }

	public Character() {}
	public Character(PartyCharacter pc, bool isInPartyi, bool isAIControlledi)
	{
		//partyCharacter = pc;
		partyCreature = pc;
		name = pc.uniqueName;
		isInParty = isInPartyi;
		isAIControlled = isAIControlledi;
		texture = pc.texture;
		Init();
	}

	protected override void Init()
	{
		base.Init();
		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" +
			Member.characterRace.name + " " + Member.characterClass.name);
	}
}

class LPlayer : Character
{
	public override int Importance { get { return 1; } }

	public LPlayer(PartyCharacter pc)
	{
		//partyCharacter = pc;
		partyCreature = pc;
		name = pc.uniqueName;
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
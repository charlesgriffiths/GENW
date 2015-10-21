using System.Xml;
using Microsoft.Xna.Framework.Graphics;

class CharacterRace : NamedObject
{
	public CharacterClass defaultClass;
	public Ability raceAbility;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		defaultClass = BigBase.Instance.classes.Get(MyXml.GetString(xnode, "defaultClass"));
	}
}

class CharacterClass : NamedObject
{
	public Ability classAbility;

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
	}
}

class Character : Creature
{
	public CharacterRace characterRace;
	public CharacterClass characterClass;

	private int level = 1;

	protected override void Init()
	{
		base.Init();
		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + characterRace.name + " " + characterClass.name);
	}

	public override float MovementSpeed { get { return 0.2f; } }
	public override float AttackSpeed { get { return 0.2f; } }
	public override int Damage { get { return 10; } }
	public override int MaxHP { get { return 100; } }
}

class LPlayer : Character
{
	public LPlayer()
	{
		characterRace = BigBase.Instance.races.Get("Dark Eloi");
		characterClass = BigBase.Instance.classes.Get("Psionic");
		isInParty = true;
		isAIControlled = false;
		base.Init();
	}

	public override void Kill() { World.Instance.player.Kill(); }
}

using Microsoft.Xna.Framework.Graphics;

abstract class PartyCreature
{
	public Texture2D texture;
	public string name, uniqueName;
	public int hp;

	public virtual int MaxHP { get { return 10; } }
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
	public CharacterRace characterRace;
	public CharacterClass characterClass;

	//public int level = 1;
	public override int MaxHP { get { return 10; } }

	public PartyCharacter(string uniqueNamei, string raceName, string className)
	{
		uniqueName = uniqueNamei;
		name = raceName + " " + className;
		characterRace = BigBase.Instance.races.Get(raceName);
		characterClass = BigBase.Instance.classes.Get(className);
		texture = MainScreen.Instance.game.Content.Load<Texture2D>("characters/" + name);
		hp = MaxHP;
    }
}

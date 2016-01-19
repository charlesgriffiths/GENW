using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Experience : LocalComponent
{
	public int value;
	public List<ClassAbility> learned;

	private Numerator<LocalShape> creaturesKilled = new Numerator<LocalShape>();
	private Numerator<ClassAbility> abilitiesUsed = new Numerator<ClassAbility>();
	private Numerator<ItemShape> itemsUsed = new Numerator<ItemShape>();
	private Numerator<ItemShape> itemsEaten = new Numerator<ItemShape>();

	private static Player P { get { return World.Instance.player; } }

	public Experience(int _value, LocalObject o) : base(o)
	{
		value = _value;

		learned = new List<ClassAbility>();
		learned.Add(t.race.ability);
	}

	public int Max { get { return 100; } }
	public int Level { get { return 1 + (int)((float)value/(float)Max); }	}
	public int AbilityPoints { get { return Level - learned.Count; } }

	public void Add(int n, bool global = false)
	{
		value += n;

		var a = new TextAnimation("+" + n + "xp", null, M.fonts.verdanaBold, Color.Yellow, global ? P.GC : t.p.GC, 1, true);

		if (global) P.movementAnimations.Add(a);
		else B.combatAnimations.Add(a);
	}

	public void Reward(LocalShape shape)
	{
		creaturesKilled.Add(shape);
		if (creaturesKilled[shape] <= 5) Add(20);
	}

	public void Reward(ClassAbility a)
	{
		abilitiesUsed.Add(a);
		if (abilitiesUsed[a] <= 3) Add(10);
	}

	public void RewardUsing(ItemShape shape)
	{
		itemsUsed.Add(shape);
		if (itemsUsed[shape] <= 3) Add(10);
	}

	public void RewardEating(ItemShape shape)
	{
		itemsEaten.Add(shape);
		if (itemsEaten[shape] <= 3) Add(10, true);
	}
}

public class GlobalExperience
{
	private static List<LocalObject> party { get { return World.Instance.player.party; } }
}

public class Numerator<T>
{
	private Dictionary<T, int> data = new Dictionary<T, int>();

	public int this[T t] { get { return data[t]; } }

	public void Add(T t)
	{
		if (data.ContainsKey(t)) data[t]++;
		else data.Add(t, 1);
	}
}
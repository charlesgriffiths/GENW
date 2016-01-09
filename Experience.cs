using System.Collections.Generic;

public class Experience : LocalComponent
{
	public int value;
	public List<ClassAbility> learned;

	public Experience(int _value, LocalObject o) : base(o)
	{
		value = _value;

		learned = new List<ClassAbility>();
		learned.Add(t.race.ability);
	}

	public int Max { get { return 100; } }
	public int Level { get { return 1 + (int)((float)value/(float)Max); }	}
	public int AbilityPoints { get { return Level - learned.Count; } }
}

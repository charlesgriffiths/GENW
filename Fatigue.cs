public class Fatigue : LocalComponent
{
	private float value;

	public Fatigue(LocalObject o) : base(o)
	{
		value = 0;
	}

	protected void Update()
	{
		int threshold = 150;
		if (value >= threshold)
		{
			t.hp.AddStamina(t.abilities.Has("Overgrowth") ? 1 : -1);
			value -= threshold;
		}
	}

	public void Add(float f)
	{
		//value += t.skills != null ? f * 0.1f * Math.Max(10 - t.skills["Survival"], 0) : f;
		value += f;
		Update();
	}
}

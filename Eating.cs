public class Eating : LocalComponent
{
	public Eating(LocalObject o) : base(o) { }

	private bool CanEat(CraftingComponent cc)
	{
		if (cc.NameIs("Iron", "Gold", "Copper", "Bottle")) return false;
		else if (!t.HasAbility("Omnivore") && cc.NameIs("Ironwood")) return false;
		else if (t.HasAbility("Overgrowth") && !cc.NameIs("Water", "Glowing Goo")) return false;
		else return true;
	}

	public bool CanEat(Item i)
	{
		foreach (CraftingComponent cc in i.data.MultilessComponents) if (!CanEat(cc)) return false;
		return true;
	}

	private void Eat(CraftingComponent cc)
	{
		int nutritionalValue = 0;

		if (cc.NameIs("Squiraug", "Squiraid", "Auglum")) nutritionalValue = 1;
		else if (cc.NameIs("Fiber", "Wood", "Leather", "Animal Parts", "Ironwood") && t.HasAbility("Omnivore")) nutritionalValue = 1;
		else if (cc.NameIs("Glowing Goo", "Dukuris")) nutritionalValue = -1;

		if (t.HasAbility("Substitutional Metabolism")) t.hp.Add(nutritionalValue);
		t.hp.AddStamina(nutritionalValue);
	}

	public void Eat(Item item)
	{
		for (int n = 0; n < item.numberOfStacks; n++)
			foreach (var t in item.data.components)
				for (int k = 0; k < t.Item2; k++) Eat(t.Item1);
	}
}

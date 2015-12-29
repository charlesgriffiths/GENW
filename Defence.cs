using System;
using System.Linq;

public class Defence : LocalComponent
{
	public Defence(LocalObject o) : base(o) { }

	public int Value
	{
		get
		{
			int result = 0;
			if (t.shape != null) result += t.shape.defence;
			if (t.skills != null) result += t.skills["Agility"];
			if (t.inventory != null) result += t.inventory.Sum(b => b.defence);

			if (t.effects != null && t.abilities != null)
			{
				result += t.effects.BothAD;

				int enemiesNearby = (from c in t.team.Enemies where c.p.IsAdjacentTo(t) select c).Count();
				result -= Math.Max(enemiesNearby - 1, 0);

				result += (from c in t.team.Friends where c.abilities.Has("Defender") && 
					t.p.Distance(c) <= CAbility.Get("Defender").range select c).Count();

				if (t.effects.Has("Marked Prey")) result -= 3;
			}

			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class HPComponent : LocalComponent
{
	public int value, stamina;
	public Dictionary<LocalObject, int> damageDealt;

	public HPComponent(LocalObject o) : base(o)
	{
		damageDealt = new Dictionary<LocalObject, int>();

		value = Max;
		stamina = Max;
	}

	public int Max
	{
		get
		{
			int result = t.shape != null ? t.shape.data.maxHP : 10;
			if (t.inventory != null) result += t.inventory.Sum(b => b.hp);
			if (t.skills != null) result += t.skills["Endurance"];
			return result;
		}
	}

	public int Armor
	{
		get
		{
			int result = 0;
			if (t.shape != null) result += t.shape.data.armor;
			if (t.inventory != null) result += t.inventory.Sum(b => b.armor);
			return result;
		}
	}

	public void AddStamina(int n, bool animate = false)
	{
		int difference = Math.Max(0, Math.Min(value, stamina + n)) - stamina;
		if (animate && difference != 0) B.combatAnimations.Add(new TextAnimation(Stuff.ShowSgn(difference), null, M.fonts.verdanaBold,
			difference > 0 ? Color.Gray : Color.Red, t.p.GC, 1, difference > 0));

		stamina += difference;
		if (stamina == 0 && t.effects != null) t.effects.Add("Sleeping", 3);
	}

	public void Add(int n, bool animate = false)
	{
		int difference = Math.Max(0, Math.Min(Max, value + n)) - value;
		if (animate && difference > 0) B.combatAnimations.Add(new TextAnimation("+" + difference, null,
			M.fonts.verdanaBold, Color.Green, t.p.GC, 1, true));

		value += difference;
		if (stamina > value) stamina = value;
		if (value == 0 && t.p != null) t.p.Kill();
	}

	public int DamageDealtBy(LocalObject u) { return damageDealt.ContainsKey(u) ? damageDealt[u] : 0; }
	public void RememberDamage(LocalObject u, int damage)
	{
		if (damageDealt.ContainsKey(u)) damageDealt[u] += damage;
		else damageDealt.Add(u, damage);
	}
}

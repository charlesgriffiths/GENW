using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Attack : LocalComponent
{
	public Attack(LocalObject o) : base(o) { }

	public int Value
	{
		get
		{
			int result = 0;
			if (t.shape != null) result += t.shape.data.attack;
			if (t.skills != null) result += t.skills["Agility"];
			if (t.inventory != null) result += t.inventory.Sum(b => b.attack);

			var e = t.effects;
			if (e != null && t.abilities != null)
			{
				result += e.BothAD;

				if (e.Has("Bravery")) result += Math.Max((from c in t.team.Enemies where t.p.Distance(c) 
					<= CAbility.Get("Bravery").range select c).Count() - 1, 0);

				if (t.abilities.Has("Swarm")) result += (from c in t.team.Friends where c.TypeName == t.TypeName && 
					c != t && t.p.Distance(c) <= CAbility.Get("Swarm").range select c).Count();

				if (e.Has("True Strike")) result += 100;
				if (e.Has("Melded")) result += 100;
			}

			return result;
		}
	}

	public float Time
	{
		get
		{
			float result = t.shape != null ? t.shape.data.attackTime : 3;
			if (t.inventory != null) result *= t.inventory.Prod(b => b.atm);
			return result;
		}
	}

	public int Damage
	{
		get
		{
			int result = t.shape != null ? t.shape.data.damage : 1;
			if (t.skills != null) result += t.skills["Strength"];
			if (t.inventory != null) result += t.inventory.Sum(b => b.damage);
			return result;
		}
	}

	public void Animate(ZPoint p, float gameTime)
	{
		if (t.p.Range == 1)
		{
			Vector2 v = p - t.p.value;
			v.Normalize();
			v *= 0.5f;

			B.combatAnimations.Add(new RMove(t.p.r, v, 0.5f * gameTime));
			B.combatAnimations.Add(new RMove(t.p.r, -v, 0.5f * gameTime));
		}
		else B.combatAnimations.Add(new TextureAnimation("arrow", t.p.GC, Battlefield.GC(p), gameTime));
	}

	public int HitChance(LocalObject u) {
		return u.defence != null ? (int)(100.0f * (Math.Max(0.0f, Math.Min(4.0f + Value - u.defence.Value, 8.0f)) / 8.0f)) : 100; }

	public void Execute(LocalObject u)
	{
		Animate(u.p.value, Time);
		B.log.AddLine(t.CommonName, t.LogColor);
		B.log.Add(" attacks " + u.CommonName, Color.Pink);

		int hitChance = HitChance(u);
		int damage = Damage;

		ZPoint.Direction direction = (u.p.value - t.p.value).GetDirection();

		if (u.HasEffect("Sleeping")) hitChance = 100;
		if (t.HasAbility("Backstab") && t.p.Distance(u) == 1)
		{
			LocalObject behind = B.Get(t.p.value.Shift(direction, 2));
			if (behind != null && behind.initiative != null && behind.team.IsEnemyTo(u))
			{
				hitChance = 100;
				damage += 3;
				B.log.Add(" from behind");
			}
		}

		List<ZPoint.Direction> availableDirections = ZPoint.Directions.Where(d => B.IsWalkable(u.p.value.Shift(d))).ToList();
		int n = availableDirections.Count;

		if (u.HasAbility("Heightened Grace") && n > 0)
		{
			u.p.Set(u.p.value.Shift(availableDirections[R.Next(n)]), u.movement.Time, false);
			B.log.Add(" but " + u.CommonName + " was ready for that!");
		}
		else if (World.Instance.random.Next(100) < hitChance)
		{
			if (t.HasEffect("Power Strike")) { damage *= 3; t.RemoveEffect("Power Strike"); }
			if (t.HasEffect("Melded")) damage *= 2;

			t.p.DoDamage(u, damage, false);

			if (t.inventory != null && t.inventory.HasAbility("Cleave") && t.p.Distance(u) == 1)
			{
				ZPoint[] secondaryTargets = new ZPoint[2];
				secondaryTargets[0] = u.p.value + ZPoint.Next(direction);
				secondaryTargets[1] = u.p.value + ZPoint.Previous(direction);
				foreach (ZPoint p in secondaryTargets)
				{
					LocalObject secondaryTarget = B.Get(p);
					if (secondaryTarget != null && secondaryTarget.hp != null)
						t.p.DoDamage(secondaryTarget, damage, false);
				}
			}

			if (t.HasEffect("Poisoned Weapon") && u.effects != null)
			{
				Effect e = t.effects.Get("Poisoned Weapon");
				if (e.parameter as string == "Paralyzing Poison") u.effects.Add("Paralyzed", 3);
			}

			t.RemoveEffect("Destined to Succeed");

			B.log.Add(" and deals " + Math.Max(0, damage - u.hp.Armor) + " damage.");
		}
		else B.log.Add(" and misses.");

		if (t.HasAbility("Annoy") && u.effects != null) u.effects.Add("Annoyed", 5);

		t.RemoveEffect("True Strike", "Hidden", "Melded", "Fake Death");
		u.RemoveEffect("Sleeping");

		t.initiative.PassTurn(Time);
	}

	public void MoveOrAttack(ZPoint.Direction d, bool control)
	{
		Log.Assert(t.movement != null, "kAttack.MoveOrAttack");
		LocalObject u = B.Get(t.p.value.Shift(d));

		if (u != null && u.hp != null) Execute(u);
		else t.movement.Move(d, control);
	}
}

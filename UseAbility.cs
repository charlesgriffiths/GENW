using System;
using System.Linq;
using System.Collections.Generic;

public partial class Abilities : LocalComponent
{
	private void AnimateByDefault(float gameTime) { t.p.Set(t.p.value, gameTime, true); }

	public void Use(Ability ability)
	{
		Ability.TargetType tt = ability.targetType;

		if (tt == Ability.TargetType.Direction || tt == Ability.TargetType.Point || tt == Ability.TargetType.Creature)
			B.ability = ability;

		else if (tt == Ability.TargetType.None)
		{
			PayAbilityCost(ability);

			if (ability is ClassAbility)
			{
				ClassAbility ca = ability as ClassAbility;
				Action<string> log = s => B.log.Add(" " + s, ca.color);

				if (ca.NameIs("Meld"))
				{
					t.effects.Add("Melded", 20);

					AnimateByDefault(ability.castTime);
					log("melds with surroundings.");
				}
				else if (ca.NameIs("Hide in Shadows"))
				{
					t.effects.Add("Hidden", 20);

					AnimateByDefault(ability.castTime);
					log("hides in shadows.");
				}
			}

			else if (ability is ItemAbility)
			{
				ItemAbility ia = ability as ItemAbility;

				if (ability.NameIs("Drink"))
				{
					if (ia.itemShape == ItemShape.Get("Nourishing Mix")) t.hp.AddStamina(100, true);
					else if (ia.itemShape == ItemShape.Get("Bottled Blood"))
					{
						int amount = t.HasAbility("Omnivore") ? 2 : -2;
						t.hp.Add(amount, true);
						t.hp.AddStamina(amount, true);
					}

					t.inventory.Remove(ia.itemShape);
					t.inventory.Add("Empty Bottle");

					AnimateByDefault(ability.castTime);
				}
				else if (ability.NameIs("Apply to Weapon"))
				{
					t.effects.Add("Poisoned Weapon", 20, ia.itemShape.name);

					t.inventory.Remove(ia.itemShape);
					t.inventory.Add("Empty Bottle");

					AnimateByDefault(ability.castTime);
				}
				else if (ability.NameIs("Dip"))
				{
					t.inventory.Remove(ia.itemShape);

					var query = from o in B.GetAll(t.p.value) select o.CommonName;
					if (query.Contains("Blood")) t.inventory.Add(ItemShape.Get("Bottled Blood"));
				}
			}

			t.initiative.PassTurn(ability.castTime);
		}
	}

	public void Use(Ability ability, ZPoint target)
	{
		PayAbilityCost(ability);

		if (ability.targetType == Ability.TargetType.Creature) Use(ability, B.Get(target));
		else if (ability.targetType == Ability.TargetType.Direction) Use(ability, ZPoint.GetDirection(target - t.p.value));
		else
		{
			if (ability is ClassAbility)
			{
				ClassAbility ca = ability as ClassAbility;
				Action<string> log = s => B.log.Add(" " + s, ca.color);

				if (ca.NameIs("Overgrowth"))
				{
					LocalObject o = B.Get(target);

					if (o == null)
					{
						if (B.IsWalkable(target))
						{
							B.Add(new LocalObject(LocalShape.Get("Tree")), target);
							log("grows a tree.");
						}
					}
					else if (o.TypeName == "Tree" || o.TypeName == "Swamp Tree")
					{
						B.Remove(o);
						B.Add(new LocalObject(LocalShape.Get(o.TypeName == "Tree" ? "Treant" : "Swamp Treant")), target, true, false);
						log("transforms a tree into a treant!");
					}
					else if (o.TypeName == "Dead Tree")
					{
						B.Remove(o);
						B.Add(new LocalObject(LocalShape.Get("Tree")), target);
					}
					else if (o.TypeName == "Poisoned Tree")
					{
						B.Remove(o);
						B.Add(new LocalObject(LocalShape.Get("Dead Tree")), target);
					}
					else if (o == t)
					{
						t.hp.Add(1, true);
						log("regenerates.");
					}
					else if (o.effects != null)
					{
						B.log.RemoveLastLine();
						o.effects.Add("Roots", 10);
					}

					AnimateByDefault(ability.castTime);
				}
				else if (ca.NameIs("Leap"))
				{
					t.p.Set(target, ability.castTime, true);
					log("leaps to a different location.");
				}
			}
			else if (ability is ItemAbility)
			{
				ItemAbility ia = ability as ItemAbility;
				if (ia.name == "Destroy Wall")
				{
					var list = (from pair in B.palette.data where pair.Value.type.name == "ground" select pair.Key).ToList();
					int r = World.Instance.random.Next(list.Count);
					B.SetTile(target, list[r]);
				}
				else if (ia.name == "Hurl")
				{
					if (ia.itemShape.name == "Flashbang")
						foreach (LocalObject c in B.ActiveObjects.Where(c => c.p.Distance(target) <= 3)) c.effects.Add("Blind", 6);

					B.combatAnimations.Add(new TextureAnimation(ia.itemShape.texture, t.p.GC, Battlefield.GC(target), ia.castTime));
				}
			}
		}

		LocalObject lc = B.Get(target);
		if (lc != null) lc.RemoveEffect("Sleeping");

		B.ability = null;
		t.initiative.PassTurn(ability.castTime);
	}

	private void Kick(ZPoint p, ZPoint.Direction d, int distance, bool self, float gameTime)
	{
		List<LocalObject> train = new List<LocalObject>();
		int i = self ? 0 : 1;
		while (true)
		{
			ZPoint shifted = p.Shift(d, i);
			LocalObject lc = B.Get(shifted);
			if (lc == null) break;
			else train.Add(lc);
			i++;
		}

		if (train.Count > 0)
		{
			ZPoint last = train.Last().p.value;
			i = 1;
			while (B.IsWalkable(last.Shift(d, i))) i++;
			int shift = Math.Min(i - 1, distance);
			foreach (LocalObject o in train) o.p.Set(o.p.value.Shift(d, shift), gameTime, o == train.First());
		}
		else AnimateByDefault(gameTime);
	}

	public void Use(Ability ability, ZPoint.Direction direction)
	{
		if (ability is ClassAbility)
		{
			ClassAbility ca = ability as ClassAbility;
			Action<string> log = s => B.log.Add(" " + s, ca.color);

			if (ca.NameIs("Bull Rush"))
			{
				Kick(t.p.value, direction, 2, true, ability.castTime);
				log("bull-rushes somebody.");
			}
			else if (ca.NameIs("Kick"))
			{
				Kick(t.p.value, direction, 2, false, ability.castTime);
				log("kicks somebody.");
			}
			else if (ca.NameIs("Power Strike"))
			{
				t.effects.Add("Power Strike", 10, direction);

				AnimateByDefault(ability.castTime);
				LocalObject lc = B.Get(t.p.value + direction);
				log("prepares to do a powerful attack" + (lc != null ? " on " + lc.CommonName : "") + ".");
			}
			else if (ca.NameIs("Hurl Rock"))
			{
				ZPoint p = t.p.value + direction;
				while (B.IsFlat(p) && t.p.Distance(p) <= ability.range) p = p + direction;
				LocalObject lc = B.Get(p);

				B.combatAnimations.Add(new TextureAnimation(NamedTexture.Get("local/stone"), t.p.GC, Battlefield.GC(p), 0.5f * ability.castTime));
				log("throws a rock and hits");

				if (lc != null)
				{
					Kick(p, direction, 1, true, 0.5f * ability.castTime);
					t.p.DoDamage(lc, 2, false);

					log(lc.CommonName + ".");
				}
				else
				{
					AnimateByDefault(0.5f * ability.castTime);
					log("nobody.");
				}
			}
		}
		else if (ability is ItemAbility)
		{
			ItemAbility ia = ability as ItemAbility;
			if (ability.NameIs("Throw"))
			{
				ZPoint p = t.p.value + direction;
				while (B.IsFlat(p) && t.p.Distance(p) <= ability.range) p = p + direction;
				LocalObject lc = B.Get(p);
				
				B.combatAnimations.Add(new TextureAnimation(ia.itemShape.texture, t.p.GC, Battlefield.GC(p), ability.castTime));

				if (ia.itemShape.name == "Net")
				{
					if (lc != null) lc.effects.Add("Net", 6);
					else B.Add(new LocalObject(new Item(ia.itemShape)), p);
				}
				else
				{
					if (lc != null) t.p.DoDamage(lc, t.attack.Damage * 2, false);
					B.Add(new LocalObject(new Item(ia.itemShape)), p);
				}

				t.inventory.Remove(ia.itemShape);
			}
			else if (ability.NameIs("Push"))
			{
				ZPoint p = t.p.value + direction;
				while (B.IsFlat(p) && t.p.Distance(p) <= ability.range) p = p + direction;
				LocalObject lc = B.Get(p);

				if (lc != null) Kick(lc.p.value, direction, 2, true, ability.castTime);
			}
			else if (ability.NameIs("Power Shot"))
			{
				var ray = B.Ray(t.p.value, direction, ability.range, true);
				B.combatAnimations.Add(new TextureAnimation(NamedTexture.Get("local/arrow"), 
					Battlefield.GC(ray.First()), Battlefield.GC(ray.Last()), ability.castTime));
				foreach (LocalObject lc in B.ActiveObjects.Where(c => c.p.value.IsIn(ray) && c != t))
					t.p.DoDamage(lc, ia.itemShape.bonus.damage + 1, false);
			}
		}
	}

	public void Use(Ability ability, LocalObject target)
	{
		if (ability is ClassAbility)
		{
			ClassAbility ca = ability as ClassAbility;
			Action<string> log = s => B.log.Add(" " + s, ca.color);
			Action<LocalObject, string> logn = (lc, s) => { B.log.AddLine(lc.CommonName, lc.LogColor); log(s); };

			if (ca.NameIs("Leadership"))
			{
				if (target.GetCreatureType.name == "Sentient")
				{
					target.team.isInParty = true;
					target.initiative.isAIControlled = false;

					log("persuades " + target.CommonName + " to join the party!");
				}
				else log("tries to persuade " + target.CommonName + "to join the party, but " + target.CommonName + " being "
				   + target.GetCreatureType.name + " is deaf to the arguments.");

				AnimateByDefault(ability.castTime);
			}
			else if (ca.NameIs("Animal Friend"))
			{
				if (target.GetCreatureType.name == "Animal")
				{
					target.team.isInParty = true;
					target.initiative.isAIControlled = false;

					log("pets " + target.CommonName + ".");
				}
				else log("tries to pet " + target.CommonName + ", but " + target.CommonName + " being " + target.GetCreatureType.name
				   + " is unresponsive and looks skeptical");

				AnimateByDefault(ability.castTime);
			}
			else if (ca.NameIs("Pommel Strike"))
			{
				t.p.DoDamage(target, 1, false);
				target.initiative.Add(-6, ability.castTime, false, true);

				AnimateByDefault(ability.castTime);
				log("strikes " + target.CommonName + " unexpectedly.");
			}
			else if (ca.NameIs("Decapitate"))
			{
				t.attack.Animate(target.p.value, ability.castTime);

				if (target.hp.stamina <= 5)
				{
					t.p.DoDamage(target, 5, false);
					log("decapitates " + target.CommonName + "!");
				}
				else
				{
					t.p.DoDamage(target, 1, false);
					log("tries to decapitate " + target.CommonName + ", but " + target.CommonName + " still has enough stamina to dodge that.");
				}
			}
			else if (ca.NameIs("Psionic Blast"))
			{
				t.p.DoDamage(target, 4, true);

				AnimateByDefault(ability.castTime);
				log("makes " + target.CommonName + " feel pain.");
			}
			else if (ca.NameIs("True Strike"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.effects.Add("True Strike", 10);
			}
			else if (ca.NameIs("Marked Prey"))
			{
				target.effects.Add("Marked Prey", 7);

				AnimateByDefault(ability.castTime);
				log("marks " + target.CommonName + ".");
			}
			else if (ca.NameIs("Dirty Fighting"))
			{
				AnimateByDefault(ability.castTime);
				log("throws sand into " + target.CommonName + "'s face!");

				target.effects.Add("Blind", 6);
			}
			else if (ca.NameIs("Fake Death"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.effects.Add("Fake Death", 20);
			}
			else if (ca.NameIs("Mind Bond"))
			{
				AnimateByDefault(ability.castTime);
				log("holds " + target.CommonName + "'s mind.");

				target.effects.Add("Unconscious", 6);
				t.effects.Add("Sleeping", 6);

				B.log.RemoveLastLine();
			}
			else if (ca.NameIs("Blindsight"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.effects.Add("Blindsight", 6, this);
			}
			else if (ca.NameIs("Sleep"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();
				if (target.HasEffect("Sleeping")) logn(target, "falls asleep within a dream.");

				if (!t.team.IsEnemyTo(target)) target.hp.AddStamina(1, true);
				target.effects.Add("Sleeping", 10);
			}
			else if (ca.NameIs("Mind Trick"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.effects.Add("Mind Tricked", 10, this);
			}
			else if (ca.NameIs("Mind Control"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.effects.Add("Mind Controlled", 10, this);
				t.effects.Add("Sleeping", 10);

				B.log.RemoveLastLine();
			}
			else if (ca.NameIs("Attention"))
			{
				target.effects.Add("Attention", 10, this);

				AnimateByDefault(ability.castTime);
				log("is now the main object of " + target.CommonName + "'s thoughts.");
			}
			else if (ca.NameIs("Prediction"))
			{
				target.effects.Add(t.team.IsEnemyTo(target) ? "Destined to Die" : "Destined to Succeed", 10, this);

				AnimateByDefault(ability.castTime);
				log("predicts that " + target.CommonName + " will " + (t.team.IsEnemyTo(target) ? "die" : "succeed") + " tonight.");
			}
			else if (ca.NameIs("Grimoire Slam"))
			{
				Kick(t.p.value, (target.p.value - t.p.value).GetDirection(), 1, false, ability.castTime);
				target.initiative.Add(-2, ability.castTime, false, true);

				log("slams " + target.CommonName + " with a book!");
			}
			else if (ca.NameIs("First Aid"))
			{
				target.hp.Add(1, true);

				AnimateByDefault(ability.castTime);
				log("heals " + target.CommonName + " a little.");
			}
		}
		else if (ability is ItemAbility)
		{
			ItemAbility ia = ability as ItemAbility;
			if (ia.name == "Bash")
			{
				t.p.DoDamage(target, 1, false);
				target.initiative.Add(-6, ability.castTime, false, true);

				AnimateByDefault(ability.castTime);
			}
		}
	}

	private void PayAbilityCost(Ability a)
	{
		t.RemoveEffect("Melded", "Hidden", "Fake Death");

		t.hp.AddStamina(-a.cost, false);
		if (a is ClassAbility) cooldowns[a as ClassAbility] += a.cooldownTime;

		B.log.AddLine(t.CommonName, t.LogColor);
	}
}
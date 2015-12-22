using System;
using System.Linq;
using System.Collections.Generic;

public partial class LCreature : LObject
{
	private void AnimateByDefault(float gameTime) { SetPosition(position, gameTime, true); }

	public void UseAbility(Ability ability)
	{
		Ability.TargetType tt = ability.targetType;

		if (tt == Ability.TargetType.Direction || tt == Ability.TargetType.Point || tt == Ability.TargetType.Creature)
			B.ability = ability;

		else if (tt == Ability.TargetType.None)
		{
			PayAbilityCost(ability);

			if (ability is CAbility)
			{
				CAbility ca = ability as CAbility;
				Action<string> log = s => B.log.Add(" " + s, ca.color);

				if (ca.NameIs("Meld"))
				{
					AddEffect("Melded", 20);

					AnimateByDefault(ability.castTime);
					log("melds with surroundings.");
				}
				else if (ca.NameIs("Hide in Shadows"))
				{
					AddEffect("Hidden", 20);

					AnimateByDefault(ability.castTime);
					log("hides in shadows.");
				}
			}

			else if (ability is IAbility)
			{
				IAbility ia = ability as IAbility;

				if (ability.NameIs("Drink"))
				{
					if (ia.itemShape.name == "Nourishing Mix") data.AddStamina(100);

					Inventory inventory = (data as Character).inventory;
					inventory.Remove(ia.itemShape);
					inventory.Add("Empty Bottle");

					AnimateByDefault(ability.castTime);
				}
				else if (ability.NameIs("Apply to Weapon"))
				{
					AddEffect("Poisoned Weapon", 20, ia.itemShape.name);

					Inventory inventory = (data as Character).inventory;
					inventory.Remove(ia.itemShape);
					inventory.Add("Empty Bottle");

					AnimateByDefault(ability.castTime);
				}
			}

			PassTurn(ability.castTime);
		}
	}

	public void UseAbility(Ability ability, ZPoint target)
	{
		PayAbilityCost(ability);

		if (ability.targetType == Ability.TargetType.Creature) UseAbility(ability, B.GetLCreature(target));
		else if (ability.targetType == Ability.TargetType.Direction) UseAbility(ability, ZPoint.GetDirection(target - position));
		else
		{
			if (ability is CAbility)
			{
				CAbility ca = ability as CAbility;
				Action<string> log = s => B.log.Add(" " + s, ca.color);

				if (ca.NameIs("Overgrowth"))
				{
					LObject o = B.GetLObject(target);

					if (o == null && B.IsWalkable(target))
					{
						B.Add(new PureLObject("Tree"), target);
						log("grows a tree.");
					}
					else if (o is PureLObject && o.Name == "Tree")
					{
						B.Remove(o);
						B.Add(new LCreature(new Creep("Treant"), true, false), target);
						log("transforms a tree into a treant!");
					}
					else if (o == this)
					{
						data.AddHP(1);
						log("regenerates.");
					}
					else if (o is LCreature)
					{
						B.log.RemoveLastLine();
						(o as LCreature).AddEffect("Roots", 10);
					}

					AnimateByDefault(ability.castTime);
				}
				else if (ca.NameIs("Leap"))
				{
					SetPosition(target, ability.castTime, true);
					log("leaps to a different location.");
				}
			}
			else if (ability is IAbility)
			{
				IAbility ia = ability as IAbility;
				if (ia.name == "Destroy Wall")
				{
					var list = (from pair in B.palette.data where pair.Value.type.name == "ground" select pair.Key).ToList();
					int r = World.Instance.random.Next(list.Count);
					B.SetTile(target, list[r]);
				}
			}
		}

		LCreature lc = B.GetLCreature(target);
		if (lc != null) lc.RemoveEffect("Sleeping");

		B.ability = null;
		PassTurn(ability.castTime);
	}

	private void Kick(ZPoint p, ZPoint.Direction d, int distance, bool self, float gameTime)
	{
		List<LObject> train = new List<LObject>();
		int i = self ? 0 : 1;
		while (true)
		{
			ZPoint shifted = p.Shift(d, i);
			LCreature lc = B.GetLCreature(shifted);
			if (lc == null) break;
			else train.Add(lc);
			i++;
		}

		if (train.Count > 0)
		{
			ZPoint last = train.Last().position;
			i = 1;
			while (B.IsWalkable(last.Shift(d, i))) i++;
			int shift = Math.Min(i - 1, distance);
			foreach (LObject o in train) o.SetPosition(o.position.Shift(d, shift), gameTime, o == train.First());
		}
		else AnimateByDefault(gameTime);
	}

	public void UseAbility(Ability ability, ZPoint.Direction direction)
	{
		if (ability is CAbility)
		{
			CAbility ca = ability as CAbility;
			Action<string> log = s => B.log.Add(" " + s, ca.color);

			if (ca.NameIs("Bull Rush"))
			{
				Kick(position, direction, 2, true, ability.castTime);
				log("bull-rushes somebody.");
			}
			else if (ca.NameIs("Kick"))
			{
				Kick(position, direction, 2, false, ability.castTime);
				log("kicks somebody.");
			}
			else if (ca.NameIs("Power Strike"))
			{
				AddEffect("Power Strike", 10, direction);

				AnimateByDefault(ability.castTime);
				LCreature lc = B.GetLCreature(position + direction);
				log("prepares to do a powerful attack" + (lc != null ? " on " + lc.UniqueName : "") + ".");
			}
			else if (ca.NameIs("Hurl Rock"))
			{
				ZPoint p = position + direction;
				while (B.IsFlat(p) && Distance(p) <= ability.range) p = p + direction;
				LCreature lc = B.GetLCreature(p);

				B.combatAnimations.Add(new TextureAnimation("stone", Battlefield.GC(position), Battlefield.GC(p), 0.5f * ability.castTime));
				log("throws a rock and hits");

				if (lc != null)
				{
					Kick(p, direction, 1, true, 0.5f * ability.castTime);
					DoDamage(lc, 2, false);

					log(lc.UniqueName + ".");
				}
				else
				{
					AnimateByDefault(0.5f * ability.castTime);
					log("nobody.");
				}
			}
		}
		else if (ability is IAbility)
		{
			IAbility ia = ability as IAbility;
			if (ability.NameIs("Throw"))
			{
				ZPoint p = position + direction;
				while (B.IsFlat(p) && Distance(p) <= ability.range) p = p + direction;
				LCreature lc = B.GetLCreature(p);
				
				B.combatAnimations.Add(new TextureAnimation(ia.itemShape.texture, Battlefield.GC(position), Battlefield.GC(p), ability.castTime));

				if (ia.itemShape.name == "Net")
				{
					if (lc != null) lc.AddEffect("Net", 6);
					else B.Add(new LItem(ia.itemShape), p);
				}
				else if (ia.itemShape.name == "Flashbang")
				{
					foreach (LCreature c in B.AliveCreatures.Where(c => c.Distance(p) <= ability.range)) c.AddEffect("Blind", 6);
				}
				else
				{
					if (lc != null) DoDamage(lc, Damage * 2, false);
					B.Add(new LItem(ia.itemShape), p);
				}

				(data as Character).inventory.Remove(ia.itemShape);
			}
			else if (ability.NameIs("Push"))
			{
				ZPoint p = position + direction;
				while (B.IsFlat(p) && Distance(p) <= ability.range) p = p + direction;
				LCreature lc = B.GetLCreature(p);

				if (lc != null) Kick(lc.position, direction, 2, true, ability.castTime);
			}
			else if (ability.NameIs("Power Shot"))
			{
				var ray = B.Ray(position, direction, ability.range);
				foreach (LCreature lc in B.AliveCreatures.Where(c => c.position.IsIn(ray) && c != this))
					DoDamage(lc, ia.itemShape.bonus.damage + 1, false);

				B.combatAnimations.Add(new TextureAnimation("arrow", Battlefield.GC(ray.First()), Battlefield.GC(ray.Last()), ability.castTime));
			}
		}
	}

	public void UseAbility(Ability ability, LCreature target)
	{
		if (ability is CAbility)
		{
			CAbility ca = ability as CAbility;
			Action<string> log = s => B.log.Add(" " + s, ca.color);
			Action<LCreature, string> logn = (lc, s) => { B.log.AddLine(lc.UniqueName, lc.LogColor); log(s); };

			if (ca.NameIs("Leadership"))
			{
				if (target.data.creepType.name == "Sentient")
				{
					target.isInParty = true;
					target.isAIControlled = false;

					log("persuades " + target.UniqueName + " to join the party!");
				}
				else log("tries to persuade " + target.UniqueName + "to join the party, but " + target.UniqueName + " being "
				   + target.data.creepType.name + " is deaf to the arguments.");

				AnimateByDefault(ability.castTime);
			}
			else if (ca.NameIs("Animal Friend"))
			{
				if (target.data.creepType.name == "Animal")
				{
					target.isInParty = true;
					target.isAIControlled = false;

					log("pets " + target.UniqueName + ".");
				}
				else log("tries to pet " + target.UniqueName + ", but " + target.UniqueName + " being " + target.data.creepType.name
				   + " is unresponsive and looks skeptical");

				AnimateByDefault(ability.castTime);
			}
			else if (ca.NameIs("Pommel Strike"))
			{
				DoDamage(target, 1, false);
				target.SetInitiative(target.initiative - 6.0f, ability.castTime, false);

				AnimateByDefault(ability.castTime);
				log("strikes " + target.UniqueName + " unexpectedly.");
			}
			else if (ca.NameIs("Decapitate"))
			{
				AnimateAttack(target.position, ability.castTime);

				if (target.Stamina <= 5)
				{
					DoDamage(target, 5, false);
					log("decapitates " + target.UniqueName + "!");
				}
				else
				{
					DoDamage(target, 1, false);
					log("tries to decapitate " + target.UniqueName + ", but " + target.UniqueName + " still has enough stamina to dodge that.");
				}
			}
			else if (ca.NameIs("Psionic Blast"))
			{
				DoDamage(target, 4, true);

				AnimateByDefault(ability.castTime);
				log("makes " + target.UniqueName + " feel pain.");
			}
			else if (ca.NameIs("True Strike"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.AddEffect("True Strike", 10);
			}
			else if (ca.NameIs("Marked Prey"))
			{
				target.AddEffect("Marked Prey", 7);

				AnimateByDefault(ability.castTime);
				log("marks " + target.UniqueName + ".");
			}
			else if (ca.NameIs("Dirty Fighting"))
			{
				AnimateByDefault(ability.castTime);
				log("throws sand into " + target.UniqueName + "'s face!");

				target.AddEffect("Blind", 6);
			}
			else if (ca.NameIs("Fake Death"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.AddEffect("Fake Death", 20);
			}
			else if (ca.NameIs("Mind Bond"))
			{
				AnimateByDefault(ability.castTime);
				log("holds " + target.UniqueName + "'s mind.");

				target.AddEffect("Unconscious", 6);
				AddEffect("Sleeping", 6);

				B.log.RemoveLastLine();
			}
			else if (ca.NameIs("Blindsight"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.AddEffect("Blindsight", 6, this);
			}
			else if (ca.NameIs("Sleep"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();
				if (target.HasEffect("Sleeping")) logn(target, "falls asleep within a dream.");

				if (!IsEnemyTo(target)) target.data.AddStamina(1);
				target.AddEffect("Sleeping", 10);
			}
			else if (ca.NameIs("Mind Trick"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.AddEffect("Mind Tricked", 10, this);
			}
			else if (ca.NameIs("Mind Control"))
			{
				AnimateByDefault(ability.castTime);
				B.log.RemoveLastLine();

				target.AddEffect("Mind Controlled", 10, this);
				AddEffect("Sleeping", 10);

				B.log.RemoveLastLine();
			}
			else if (ca.NameIs("Attention"))
			{
				target.AddEffect("Attention", 10, this);

				AnimateByDefault(ability.castTime);
				log("is now the main object of " + target.UniqueName + "'s thoughts.");
			}
			else if (ca.NameIs("Prediction"))
			{
				target.AddEffect(IsEnemyTo(target) ? "Destined to Die" : "Destined to Succeed", 10, this);

				AnimateByDefault(ability.castTime);
				log("predicts that " + target.UniqueName + " will " + (IsEnemyTo(target) ? "die" : "succeed") + " tonight.");
			}
			else if (ca.NameIs("Grimoire Slam"))
			{
				target.AddInitiative(-2.0f, ability.castTime, false);
				Kick(position, (target.position - position).GetDirection(), 1, false, ability.castTime);

				log("slams " + target.UniqueName + " with a book!");
			}
			else if (ca.NameIs("First Aid"))
			{
				target.data.AddHP(1);

				AnimateByDefault(ability.castTime);
				log("heals " + target.UniqueName + " a little.");
			}
		}
		else if (ability is IAbility)
		{
			IAbility ia = ability as IAbility;
			if (ia.name == "Bash")
			{
				DoDamage(target, 1, false);
				target.AddInitiative(-6, ability.castTime, false);

				AnimateByDefault(ability.castTime);
			}
		}
	}

	private void PayAbilityCost(Ability ability)
	{
		RemoveEffects("Melded", "Hidden", "Fake Death");

		data.AddStamina(-ability.cost);

		B.log.AddLine(UniqueName, LogColor);
	}
}
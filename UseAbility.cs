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

			Action<string> log = s => B.log.Add(" " + s, ability.color);

			if (ability.NameIs("Meld"))
			{
				AddEffect("Melded", 20);

				AnimateByDefault(ability.castTime);
				log("melds with surroundings.");
			}
			else if (ability.NameIs("Hide in Shadows"))
			{
				AddEffect("Hidden", 20);

				AnimateByDefault(ability.castTime);
				log("hides in shadows.");
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
			Action<string> log = s => B.log.Add(" " + s, ability.color);

			if (ability.NameIs("Nature's Call"))
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
			else if (ability.NameIs("Leap"))
			{
				//Log.WriteLine(target.ToString());
				SetPosition(target, ability.castTime, true);
				log("leaps to a different location.");
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

		//Log.Write("Direction = " + ZPoint.Name(d) + "; ");
		//Log.Write("Train length = " + train.Count + "; ");

		if (train.Count > 0)
		{
			ZPoint last = train.Last().position;
			i = 1;
			while (B.IsWalkable(last.Shift(d, i))) i++;
			int shift = Math.Min(i - 1, distance);
			//Log.WriteLine("shift = " + shift);
			foreach (LObject o in train) o.SetPosition(o.position.Shift(d, shift), gameTime, o == train.First());
		}
		else AnimateByDefault(gameTime);
	}

	public void UseAbility(Ability ability, ZPoint.Direction direction)
	{
		Action<string> log = s => B.log.Add(" " + s, ability.color);

		if (ability.NameIs("Bull Rush"))
		{
			Kick(position, direction, 2, true, ability.castTime);
			log("bull-rushes somebody.");
		}
		else if (ability.NameIs("Kick"))
		{
			Kick(position, direction, 2, false, ability.castTime);
			log("kicks somebody.");
		}
		else if (ability.NameIs("Power Strike"))
		{
			AddEffect("Power Strike", 10, direction);

			AnimateByDefault(ability.castTime);
			LCreature lc = B.GetLCreature(position + direction);
			log("prepares to do a powerful attack" + (lc != null ? " on " + lc.UniqueName : "") + ".");
		}
		else if (ability.NameIs("Hurl Rock"))
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

	public void UseAbility(Ability ability, LCreature target)
	{
		Action<string> log = s => B.log.Add(" " + s, ability.color);
		Action<LCreature, string> logn = (lc, s) => { B.log.AddLine(lc.UniqueName, lc.LogColor); log(s); };

		if (ability.NameIs("Leadership"))
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
		else if (ability.NameIs("Animal Friend"))
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
		else if (ability.NameIs("Pommel Strike"))
		{
			DoDamage(target, 1, false);
			target.SetInitiative(target.initiative - 6.0f, ability.castTime, false);

			AnimateByDefault(ability.castTime);
			log("strikes " + target.UniqueName + " unexpectedly.");
        }
		else if (ability.NameIs("Decapitate"))
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
		else if (ability.NameIs("Psionic Blast"))
		{
			DoDamage(target, 4, true);

			AnimateByDefault(ability.castTime);
			log("makes " + target.UniqueName + " feel pain.");
		}
		else if (ability.NameIs("True Strike"))
		{
			AnimateByDefault(ability.castTime);
			B.log.RemoveLastLine();

			target.AddEffect("True Strike", 10);
		}
		else if (ability.NameIs("Marked Prey"))
		{
			target.AddEffect("Marked Prey", 7);

			AnimateByDefault(ability.castTime);
			log("marks " + target.UniqueName + ".");
		}
		else if (ability.NameIs("Dirty Fighting"))
		{
			AnimateByDefault(ability.castTime);
			log("throws sand into " + target.UniqueName + "'s face!");

			target.AddEffect("Blind", 6);
		}
		else if (ability.NameIs("Fake Death"))
		{
			AnimateByDefault(ability.castTime);
			B.log.RemoveLastLine();

			target.AddEffect("Fake Death", 20);
		}
		else if (ability.NameIs("Mind Bond"))
		{
			AnimateByDefault(ability.castTime);
			log("holds " + target.UniqueName + "'s mind.");

			target.AddEffect("Unconscious", 6);
			AddEffect("Sleeping", 6);

			B.log.RemoveLastLine();
		}
		else if (ability.NameIs("Blindsight"))
		{
			AnimateByDefault(ability.castTime);
			B.log.RemoveLastLine();

			target.AddEffect("Blindsight", 6, this);
		}
		else if (ability.NameIs("Sleep"))
		{
			AnimateByDefault(ability.castTime);
			B.log.RemoveLastLine();
			if (target.HasEffect("Sleeping")) logn(target, "falls asleep within a dream.");

			if (!IsEnemyTo(target)) target.data.AddStamina(1);
			target.AddEffect("Sleeping", 10);
		}
		else if (ability.NameIs("Mind Trick"))
		{
			AnimateByDefault(ability.castTime);
			B.log.RemoveLastLine();

			target.AddEffect("Mind Tricked", 10, this);
		}
		else if (ability.NameIs("Mind Control"))
		{
			AnimateByDefault(ability.castTime);
			B.log.RemoveLastLine();

			target.AddEffect("Mind Controlled", 10, this);
			AddEffect("Sleeping", 10);

			B.log.RemoveLastLine();
		}
		else if (ability.NameIs("Attention"))
		{
			target.AddEffect("Attention", 10, this);

			AnimateByDefault(ability.castTime);
			log("is now the main object of " + target.UniqueName + "'s thoughts.");
		}
		else if (ability.NameIs("Prediction"))
		{
			target.AddEffect(IsEnemyTo(target) ? "Destined to Die" : "Destined to Succeed", 10, this);

			AnimateByDefault(ability.castTime);
			log("predicts that " + target.UniqueName + " will " + (IsEnemyTo(target) ? "die" : "succeed") + " tonight.");
		}
		else if (ability.NameIs("Grimoire Slam"))
		{
			target.AddInitiative(-2.0f, ability.castTime, false);
			Kick(position, (target.position - position).GetDirection(), 1, false, ability.castTime);

			log("slams " + target.UniqueName + " with a book!");
		}
		else if (ability.NameIs("First Aid"))
		{
			target.data.AddHP(1);

			AnimateByDefault(ability.castTime);
			log("heals " + target.UniqueName + " a little.");
		}
	}

	private void PayAbilityCost(Ability ability)
	{
		RemoveEffects("Melded", "Hidden", "Fake Death");
		data.AddStamina(-ability.cost);

		B.log.AddLine(UniqueName, LogColor);
	}
}
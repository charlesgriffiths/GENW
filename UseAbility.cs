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

			if (ability.NameIs("Meld"))
			{
				AddEffect("Melded", 20);
				AnimateByDefault(ability.castTime);
			}
			else if (ability.NameIs("Hide in Shadows"))
			{
				AddEffect("Hidden", 20);
				AnimateByDefault(ability.castTime);
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
			if (ability.NameIs("Nature's Call"))
			{
				LObject o = B.GetLObject(target);

				if (o == null && B.IsWalkable(target)) B.Add(new PureLObject("Tree"), target);
				else if (o is PureLObject && o.Name == "Tree")
				{
					B.Remove(o);
					B.Add(new LCreature(new Creep("Treant"), true, false), target);
				}
				else if (o == this) data.AddHP(1);
				else if (o is LCreature) (o as LCreature).AddEffect("Roots", 10);

				AnimateByDefault(ability.castTime);
			}
			else if (ability.NameIs("Leap"))
			{
				Log.WriteLine(target.ToString());
				SetPosition(target, ability.castTime, true);
			}
		}

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
		if (ability.NameIs("Bull Rush")) Kick(position, direction, 2, true, ability.castTime);
		else if (ability.NameIs("Kick")) Kick(position, direction, 2, false, ability.castTime);
		else if (ability.NameIs("Power Strike"))
		{
			AddEffect("Power Strike", 10, direction);
			AnimateByDefault(ability.castTime);
			//colorful description with words in combat log
		}
		else if (ability.NameIs("Hurl Rock"))
		{
			ZPoint p = position + direction;
			while (B.IsFlat(p) && Distance(p) <= ability.range) p = p + direction;
			LCreature lc = B.GetLCreature(p);

			B.combatAnimations.Add(new TextureAnimation("stone", B.GC(position), B.GC(p), 0.5f * ability.castTime));

			if (lc != null)
			{
				Kick(p, direction, 1, true, 0.5f * ability.castTime);
				DoDamage(lc, 2, false);
			}
			else AnimateByDefault(0.5f * ability.castTime);
		}
	}

	public void UseAbility(Ability ability, LCreature target)
	{
		if (ability.NameIs("Leadership"))
		{
			if (target.data.creepType.name == "Sentient")
			{
				target.isInParty = true;
				target.isAIControlled = false;
			}

			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Animal Friend"))
		{
			if (target.data.creepType.name == "Animal")
			{
				target.isInParty = true;
				target.isAIControlled = false;
			}

			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Pommel Strike"))
		{
			DoDamage(target, 1, false);
			target.SetInitiative(target.initiative - 6.0f, ability.castTime, false);

			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Decapitate"))
		{
			AnimateAttack(target.position, ability.castTime);

			if (target.Endurance <= 5) DoDamage(target, 5, false);
			else DoDamage(target, 1, false);
		}
		else if (ability.NameIs("Psionic Blast"))
		{
			DoDamage(target, 4, true);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("True Strike"))
		{
			target.AddEffect("True Strike", 10);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Marked Prey"))
		{
			target.AddEffect("Marked Prey", 7);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Dirty Fighting"))
		{
			target.AddEffect("Blind", 6);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Fake Death"))
		{
			target.AddEffect("Fake Death", 20);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Mind Bond"))
		{
			target.AddInitiative(-ability.castTime, ability.castTime, false);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Blindsight"))
		{
			target.AddEffect("Blindsight", 6, this);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Sleep"))
		{
			if (!IsEnemyTo(target)) target.data.AddEndurance(1);
			target.AddEffect("Sleeping", 10);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Mind Trick"))
		{
			target.AddEffect("Mind Tricked", 10, this);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Mind Control"))
		{
			target.AddEffect("Mind Controlled", ability.castTime);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Attention"))
		{
			target.AddEffect("Attention", 10, this);
			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Prediction"))
		{
			if (IsEnemyTo(target)) target.AddEffect("Destined to Die", 10);
			else target.AddEffect("Destined to Succeed", 10);

			AnimateByDefault(ability.castTime);
		}
		else if (ability.NameIs("Grimoire Slam"))
		{
			target.AddInitiative(-2.0f, ability.castTime, false);
			Kick(position, (target.position - position).GetDirection(), 1, false, ability.castTime);
		}
		else if (ability.NameIs("First Aid"))
		{
			target.data.AddHP(1);
			AnimateByDefault(ability.castTime);
		}
	}

	private void PayAbilityCost(Ability ability)
	{
		RemoveEffects("Melded", "Hidden", "Fake Death");
		data.AddEndurance(-ability.cost);
	}
}
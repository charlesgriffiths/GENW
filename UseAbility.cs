using System;
using System.Linq;
using System.Collections.Generic;

public partial class LCreature : LObject
{
	public void UseAbility(Ability ability)
	{
		Ability.TargetType tt = ability.targetType;

		if (tt == Ability.TargetType.Direction || tt == Ability.TargetType.Point || tt == Ability.TargetType.Creature)
			B.ability = ability;

		else if (tt == Ability.TargetType.None)
		{
			PayAbilityCost(ability);

			if (ability.NameIs("Meld")) AddEffect("Melded", 20);
			else if (ability.NameIs("Hide in Shadows")) AddEffect("Hidden", 20);

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
			}
			else if (ability.NameIs("Leap"))
			{
				Log.WriteLine(target.ToString());
				SetPosition(target, 2.0f, true);
			}
		}

		B.ability = null;
		PassTurn(ability.castTime);
	}

	public void UseAbility(Ability ability, ZPoint.Direction direction)
	{
		if (ability.NameIs("Bull Rush") || ability.NameIs("Kick"))
		{
			List<LObject> train = new List<LObject>();
			int i = ability.NameIs("Bull Rush") ? 0 : 1;
			while (true)
			{
				ZPoint shifted = position.Shift(direction, i);
				LCreature lc = B.GetLCreature(shifted);
				if (lc == null) break;
				else train.Add(lc);
				i++;
			}

			if (train.Count > 0)
			{
				//Log.WriteLine("Train length = " + train.Count);
				ZPoint last = train.Last().position;
				i = 1;
				while (B.IsWalkable(last.Shift(direction, i))) i++;
				int shift = Math.Min(i - 1, 2);
				//Log.WriteLine("shift = " + shift);
				foreach (LObject o in train) o.SetPosition(o.position.Shift(direction, shift), 4.0f, false);
			}
		}
		else if (ability.NameIs("Power Strike"))
		{
			AddEffect("Power Strike", 10, direction);
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
		}
		else if (ability.NameIs("Animal Friend"))
		{
			if (target.data.creepType.name == "Animal")
			{
				target.isInParty = true;
				target.isAIControlled = false;
			}
		}
		else if (ability.NameIs("Pommel Strike"))
		{
			DoDamage(target, 1, false);
			target.SetInitiative(target.initiative - 2.0f, 2.0f, false);
		}
		else if (ability.NameIs("Decapitate"))
		{
			AnimateAttack(target.position);
			if (target.Endurance <= 5) DoDamage(target, 5, false);
			else DoDamage(target, 1, false);
		}
		else if (ability.NameIs("Psionic Blast"))
		{
			DoDamage(target, 4, true);
		}
		else if (ability.NameIs("True Strike"))
		{
			target.AddEffect("True Strike", 10);
			//B.combatAnimations.Add(new ScalingAnimation(target, 1.3f, 1.0f));
		}
		else if (ability.NameIs("Marked Prey")) target.AddEffect("Marked Prey", 7);
		else if (ability.NameIs("Dirty Fighting")) target.AddEffect("Blind", 6);
		else if (ability.NameIs("Fake Death")) target.AddEffect("Fake Death", 20);
		else if (ability.NameIs("Mind Bond")) target.AddInitiative(-ability.castTime);
		else if (ability.NameIs("Blindsight")) target.AddEffect("Blindsight", 6, this);
		else if (ability.NameIs("Sleep"))
		{
			if (!IsEnemyTo(target)) target.data.AddEndurance(1);
			target.AddEffect("Sleeping", 10);
		}
		else if (ability.NameIs("Mind Trick")) target.AddEffect("Mind Tricked", 10, this);
		else if (ability.NameIs("Mind Controll")) target.AddEffect("Mind Controlled", ability.castTime);
		else if (ability.NameIs("Attention")) target.AddEffect("Attention", 10, this);
		else if (ability.NameIs("Prediction"))
		{
			if (IsEnemyTo(target)) target.AddEffect("Destined to Die", 10);
			else target.AddEffect("Destined to Succeed", 10);
		}
	}

	private void PayAbilityCost(Ability ability)
	{
		RemoveEffects("Melded", "Hidden", "Fake Death");
		data.AddEndurance(-ability.cost);
	}
}
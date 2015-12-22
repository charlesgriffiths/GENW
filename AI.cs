using System.Linq;
using System.Collections.Generic;

partial class LCreature : LObject
{
	private float AITargetRank(LCreature lc)
	{
		float result = DamageDealtBy(lc) - Distance(lc);

		if (lc.HasEffect("Fake Death")) result -= 10;
		if (HasEffect("Annoyed") && lc.HasAbility("Annoy")) result += 5;
		if (HasEffect("Attention") && GetEffect("Attention").parameter == lc) result += 10;

		return result;
	} 

	private Action AI()
	{
		if (HasOneOfEffects("Sleeping", "Unconscious", "Paralyzed")) return new AWait(MovementTime);
        if (HasEffect("Power Strike"))
		{
			LCreature lc = B.GetLCreature(position + (ZPoint.Direction)GetEffect("Power Strike").parameter);

			if (lc != null) return new AAttack(lc);
			else
			{
				RemoveEffect("Power Strike");
				return new AWait(AttackTime);
			}
		}

		var viableTargets = from c in B.AliveCreatures where CanSee(c) && IsEnemyTo(c) orderby AITargetRank(c) select c;
		if (viableTargets.Count() == 0) return new AWait(MovementTime);

		LCreature target = viableTargets.Last() as LCreature;
		if (IsAdjacentTo(target)) return new AAttack(target);

		List<ZPoint.Direction> path = B.Path(position, target.position);
		if (path != null) return new AMove(path.First());
		else return new AWait(MovementTime);
    }
}

abstract class Action
{
	public virtual void Run(LCreature c) {}
}

class AWait : Action
{
	public float time;

	public AWait(float timei) { time = timei; }

	public override void Run(LCreature c)
	{
		c.Wait(time);
	}
}

class AMove : Action
{
	public ZPoint.Direction direction;

	public AMove(ZPoint.Direction d) { direction = d; }

	public override void Run(LCreature c)
	{
		c.Move(direction, false);
	}
}

class AAttack : Action
{
	public LCreature creature;

	public AAttack(LCreature c) { creature = c; }

	public override void Run(LCreature c)
	{
		c.DoAttack(creature);
	}
}
using System.Linq;
using System.Collections.Generic;

partial class LCreature : LObject
{
	private int AITargetRank(LCreature c)
	{
		return 10 - MyMath.ManhattanDistance(position, c.position);
	} 

	private Action AI()
	{
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

		var viableTargets = from c in B.AliveCreatures where c.IsVisible && IsEnemyTo(c) orderby AITargetRank(c) select c;
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
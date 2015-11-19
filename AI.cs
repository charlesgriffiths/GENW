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
		var viableTargets = from c in B.AliveCreatures where IsEnemyTo(c) orderby AITargetRank(c) select c;
		if (viableTargets.Count() == 0) return new AWait();

		LCreature target = viableTargets.Last() as LCreature;
		if (IsAdjacentTo(target)) return new AAttack(target);

		List<ZPoint.Direction> path = B.Path(position, target.position);
		if (path != null) return new AMove(path.First());
		else return new AWait();
    }
}

abstract class Action
{
	public virtual void Run(LCreature c) {}
}

class AWait : Action
{
	public AWait() {}

	public override void Run(LCreature c)
	{
		c.Wait();
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
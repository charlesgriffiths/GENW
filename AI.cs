using System.Linq;

abstract partial class Creature : LObject
{
	private int AITargetRank(Creature c)
	{
		return 10 - MyMath.ManhattanDistance(position, c.position);
	} 

	private Action AI()
	{
		var viableTargets = from c in B.AliveCreatures where IsEnemyTo(c) orderby AITargetRank(c) select c;
		if (viableTargets.Count() == 0) return new AWait();

		Creature target = viableTargets.Last() as Creature;
		if (position.IsAdjacent(target.position)) return new AAttack(target);

		var viableDirections = from d in ZPoint.Directions where B.IsWalkable(position.Shift(d)) orderby MyMath.ManhattanDistance(position.Shift(d), target.position) select d;
		if (viableDirections.Count() > 0) return new AMove(viableDirections.First());
		else return new AWait();
    }
}

abstract class Action
{
	public virtual void Run(Creature c) {}
}

class AWait : Action
{
	public AWait() {}

	public override void Run(Creature c)
	{
		c.Wait();
	}
}

class AMove : Action
{
	public ZPoint.Direction direction;

	public AMove(ZPoint.Direction d) { direction = d; }

	public override void Run(Creature c)
	{
		c.Move(direction, false);
	}
}

class AAttack : Action
{
	public Creature creature;

	public AAttack(Creature c) { creature = c; }

	public override void Run(Creature c)
	{
		c.DoAttack(creature);
	}
}
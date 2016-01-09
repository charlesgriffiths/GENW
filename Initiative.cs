using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class Initiative : LocalComponent
{
	public float value;
	public RPoint r;
	public AnimationQueue animations;
	public bool isAIControlled;

	public Initiative(bool isAIControlledi, LocalObject o) : base(o)
	{
		value = 0;
		r = new RPoint();
		animations = new AnimationQueue();
		isAIControlled = isAIControlledi;
	}

	public void Set(float valuei, float gameTime, bool commonQueue)
	{
		RMove rMove = new RMove(r, new Vector2(valuei - value, 0), gameTime);
		if (commonQueue) B.scaleAnimations.Add(rMove);
		else animations.Add(rMove);
		value = valuei;
	}

	public void Add(float amount, float gameTime, bool commonQueue, bool animate)
	{
		if (animate) B.combatAnimations.Add(new TextAnimation(amount.ToString(), null, M.fonts.verdanaBold, 
			Color.Blue, t.p.GC, 1, true));

		Set(value + amount, gameTime, commonQueue);
	}

	public bool IsAIControlled
	{
		get
		{
			if (t.HasEffect("Power Strike", "Sleeping", "Unconscious", "Paralyzed")) return true;
			else if (t.HasEffect("Mind Controlled")) return false;
			else return isAIControlled;
		}
	}

	public void Wait(float time = 1)
	{
		t.p.Set(t.p.value, time, true);
		PassTurn(time);
	}

	public void Run()
	{
		if (t.hp.stamina == 0) t.effects.Add("Sleeping", 1);

		if (!IsAIControlled)
		{
			B.current = B.NextObject;
			B.spotlight = B.current;
			return;
		}

		Action action = AI();
		action.Run(t);
	}

	public void ContinueTurn(float time)
	{
		Log.Assert(time > 0, "kInitiative.ContinueTurn");
		Set(value - time, time, true);

		if (t.abilities != null) t.abilities.UpdateCooldowns(time);
		if (t.inventory != null) t.inventory.UpdateCooldowns(time);
	}

	public void PassTurn(float time)
	{
		if (t.effects != null)
		{
			foreach (Effect e in t.effects.data) e.timeLeft -= time;
			foreach (Effect e in t.effects.data.Where(f => f.timeLeft <= 0).ToList()) t.effects.Remove(e.data.name);
		}

		if (t.movement != null) t.movement.counter = 3;

		ContinueTurn(time);

		LocalObject nextObject = B.NextObject;
		B.spotlight = nextObject;
		if (nextObject != null) nextObject.initiative.Run();
	}

	private int AITargetRank(LocalObject u)
	{
		int result = t.hp.DamageDealtBy(u) - t.p.Distance(u);

		if (u.HasEffect("Fake Death")) result -= 10;
		if (t.HasEffect("Annoyed") && u.HasAbility("Annoy")) result += 5;
		if (t.HasEffect("Attention") && t.effects.Get("Attention").parameter == u) result += 10;

		return result;
	}

	private Action AI()
	{
		if (t.HasEffect("Sleeping", "Unconscious", "Paralyzed")) return new AWait(t.movement.Time);
		if (t.HasEffect("Power Strike"))
		{
			LocalObject u = B.Get(t.p.value + (ZPoint.Direction)t.effects.Get("Power Strike").parameter);

			if (u != null && u.hp != null) return new AAttack(u);
			else
			{
				t.RemoveEffect("Power Strike");
				return new AWait(t.attack.Time);
			}
		}

		var viableTargets = from c in B.ActiveObjects where t.p.CanSee(c) && t.team.IsEnemyTo(c) orderby AITargetRank(c) select c;
		if (viableTargets.Count() == 0) return new AWait(t.movement.Time);

		LocalObject target = viableTargets.Last();
		if (t.p.IsAdjacentTo(target)) return new AAttack(target);

		List<ZPoint.Direction> path = B.Path(t.p.value, target.p.value);
		if (path != null) return new AMove(path.First());
		else return new AWait(t.movement.Time);
	}
}

abstract class Action { public virtual void Run(LocalObject o) { } }

class AWait : Action {
	public float time;
	public AWait(float timei) { time = timei; }
	public override void Run(LocalObject o) { o.initiative.Wait(time); }}

class AMove : Action {
	public ZPoint.Direction direction;
	public AMove(ZPoint.Direction d) { direction = d; }
	public override void Run(LocalObject o) { o.movement.Move(direction, false); }}

class AAttack : Action {
	public LocalObject defender;
	public AAttack(LocalObject defenderi) { defender = defenderi; }
	public override void Run(LocalObject o) { o.attack.Execute(defender); }}
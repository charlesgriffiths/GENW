using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public partial class LCreature : LObject
{
	public Creature data;
	private List<Effect> effects = new List<Effect>();
	public bool isInParty, isAIControlled;
	public int controlMovementCounter;

	public int HP { get { return data.hp; } }
	public int Endurance { get { return data.endurance; } }
	public bool IsAlive { get { return data.IsAlive; } }
	public List<Ability> Abilities { get { return data.Abilities; } }

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }
	private GeneralBase<Ability> A { get { return BigBase.Instance.abilities; }	}

	protected override void Init()
	{
		initiative = 0.0f;
		controlMovementCounter = 3;
		base.Init();
	}

	public override bool IsWalkable { get { return false; } }
	public override string Name { get { return data.Name; } }
	public string UniqueName { get { return data.uniqueName; } }

	public float MovementTime { get { return data.MovementTime; } }
	public float AttackTime { get { return data.AttackTime; } }
	public int Damage { get { return data.Damage; } }
	public int MaxHP { get { return data.MaxHP; } }

	public int Attack
	{
		get
		{
			int result = data.Attack;

			if (HasAbility("Bravery"))
				result += Math.Max((from c in Enemies where Distance(c) <= A.Get("Bravery").range select c).Count() - 1, 0);

			if (HasAbility("Swarm") && data is Creep)
				result += (from c in Friends where c.Name == Name && c != this && Distance(c) <= A.Get("Swarm").range select c).Count();

			if (HasAbility("Lone Warrior") && (from c in B.AliveCreatures where Distance(c) <= A.Get("Lone Warrior").range select c).Count() == 0)
				result += 2;

			if (HasEffect("True Strike")) result += 100;

			return result;
		}
	}

	public int Defence
	{
		get
		{
			int result = data.Defence;

			int enemiesNearby = (from c in Enemies where c.position.IsAdjacentTo(position) select c).Count();
			result -= Math.Max(enemiesNearby - 1, 0);

			result += (from c in Friends where c.HasAbility("Defender") && Distance(c) <= A.Get("Defender").range select c).Count();

			if (HasAbility("Lone Warrior") && (from c in B.AliveCreatures where Distance(c) <= A.Get("Lone Warrior").range select c).Count() == 0)
				result += 2;

			return result;
		}
	}

	public int Armor { get { return 0; } }

	public override int Importance { get { return data.Importance; } }

	public override void Kill()
	{
		texture = BigBase.Instance.textures.Get("blood").Random();
		base.Kill();
	}

	public override Color RelationshipColor
	{
		get
		{
			if (isInParty) return Color.Green;
			else return Color.Red;
		}
	}

	public bool IsEnemyTo(LCreature c) { return isInParty != c.isInParty; }
	public List<LCreature> Enemies { get { return (from c in B.AliveCreatures where c.IsEnemyTo(this) select c).Cast<LCreature>().ToList(); } }
	public List<LCreature> Friends { get { return (from c in B.AliveCreatures where !c.IsEnemyTo(this) select c).Cast<LCreature>().ToList(); } }

	public LCreature(Creature c, bool isInPartyi, bool isAIControlledi)
	{
		data = c;
		isInParty = isInPartyi;
		isAIControlled = isAIControlledi;
		texture = data.texture;
		Init();
	}

	private void AnimateFailedMovement(ZPoint.Direction d)
	{
		Vector2 v = 0.25f * (Vector2)(ZPoint.Zero.Shift(d));
		B.combatAnimations.Add(new RMove(rPosition, v, 4.0f / MovementTime));
		B.combatAnimations.Add(new RMove(rPosition, -v, 4.0f / MovementTime));
	}

	private void AnimateAttack(ZPoint p)
	{
		Vector2 v = p - position;
		v.Normalize();
		v *= 0.5f;

		B.combatAnimations.Add(new RMove(rPosition, v, 4.0f / AttackTime));
		B.combatAnimations.Add(new RMove(rPosition, -v, 4.0f / AttackTime));
	}

	public void DoDamage(int damage, bool pure)
	{
		int finalDamage = pure ? damage : Math.Max(damage - Armor, 0);
		data.AddEndurance(-finalDamage);
		if (data.endurance == 0) Kill();
		B.combatAnimations.Add(new DamageAnimation(finalDamage, B.GraphicCoordinates(position), 1.0f, pure));
	}

	public int HitChance(LCreature c) { return (int)(100.0f * (Math.Max(0.0f, Math.Min(4.0f + Attack - c.Defence, 8.0f)) / 8.0f)); }

	public void DoAttack(LCreature c)
	{
		AnimateAttack(c.position);

		if (World.Instance.random.Next(100) < HitChance(c)) c.DoDamage(Damage, false);

		RemoveEffect("True Strike");

		PassTurn(AttackTime);
	}

	public void TryToMove(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		LCreature c = B.GetLCreature(destination);

		if (c != null) DoAttack(c);
		else Move(d, control);
	}

	public void Move(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		if (B.IsWalkable(destination)) SetPosition(destination, 2.0f / MovementTime, true);
		else AnimateFailedMovement(d);

		if (control == true && controlMovementCounter > 0)
		{
			controlMovementCounter--;
			ContinueTurn(MovementTime);
		}
		else
		{
			PassTurn(MovementTime);
		}
	}

	public void Wait()
	{
		SetPosition(position, 2.0f / MovementTime, true);
		PassTurn(MovementTime);
	}

	public override void Run()
	{
		if (!isAIControlled)
		{
			B.currentObject = B.NextLObject;
			B.spotlightObject = B.currentObject;
			return;
		}

		Action action = AI();
		action.Run(this);
	}

	protected override void PassTurn(float time)
	{
		foreach (Effect e in effects) e.timeLeft -= time;
		foreach (Effect e in effects.Where(f => f.timeLeft <= 0).ToList()) effects.Remove(e);

		controlMovementCounter = 3;
		base.PassTurn(time);
	}

	private void AddEffect(string name, float time)
	{
		var query = effects.Where(e => e.data.name == name);
		if (query.Count() > 0)
		{
			Effect e = query.Single();
			if (e.timeLeft < time) e.timeLeft = time;
		}
		else effects.Add(new Effect(name, time));
	}

	private bool HasAbility(string name) { return data.HasAbility(name); }
	private bool HasEffect(string name) { return effects.Where(e => e.data.name == name).Count() > 0; }

	private void RemoveEffect(string name)
	{
		var query = effects.Where(e => e.data.name == name);
		if (query.Count() > 0)
		{
			Effect effect = query.Single();
			effects.Remove(effect);
		}
	}

	public void DrawEffects(ZPoint p)
	{
		int i = 0;
		foreach (Effect e in effects)
		{
			M.Draw(e.data.texture, p + new ZPoint(32 * i, 0), e.data.SgnColor);
			i++;
		}

		// еще нужно будет нарисовать описания эффектов
	}

	public void UseAbility(Ability ability)
	{
		Ability.TargetType tt = ability.targetType;

		if (tt == Ability.TargetType.Direction || tt == Ability.TargetType.Point || tt == Ability.TargetType.Creature)
			B.ability = ability;

		else if (tt == Ability.TargetType.None)
		{
		}
	}

	public void UseAbility(Ability ability, ZPoint target)
	{
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
		data.AddEndurance(-ability.cost);
		PassTurn(ability.castTime);
	}

	public void UseAbility(Ability ability, ZPoint.Direction direction)
	{
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
			target.DoDamage(1, false);
			target.SetInitiative(target.initiative - 2.0f, 2.0f, false);
		}
		else if (ability.NameIs("Decapitate"))
		{
			AnimateAttack(target.position);
			if (target.Endurance <= 5) target.DoDamage(5, false);
			else target.DoDamage(1, false);
		}
		else if (ability.NameIs("Psionic Blast"))
		{
			target.DoDamage(4, true);
		}
		else if (ability.NameIs("True Strike"))
		{
			target.AddEffect("True Strike", 10);
			B.combatAnimations.Add(new ScalingAnimation(target, 1.3f, 1.0f));
		}
    }
}
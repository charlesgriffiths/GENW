using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public partial class LCreature : LObject
{
	public Creature data;
	private List<Effect> effects = new List<Effect>();
	public bool isInParty;
	private bool isAIControlled;
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
			if (HasEffect("Melded")) result += 100;

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

			if (HasEffect("Marked Prey")) result -= 3;

			return result;
		}
	}

	public int Armor { get { return 0; } }

	public override bool IsVisible { get { return HasEffect("Melded") || HasEffect("Hidden") ? false : true; } }
	public bool IsAIControlled { get { return isAIControlled || HasEffect("Power Strike"); } }
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

		if (World.Instance.random.Next(100) < HitChance(c))
		{
			int damage = Damage;

			if (HasEffect("Power Strike")) { damage *= 3; RemoveEffect("Power Strike");	}
			if (HasEffect("Melded")) damage *= 2;

			c.DoDamage(damage, false);
		}

		RemoveEffect("True Strike");
		RemoveEffect("Hidden");
		RemoveEffect("Melded");

		PassTurn(AttackTime);
	}

	public void TryToMove(ZPoint.Direction d, bool control) // это исключительно для автоматической атаки при передвижении
	{
		ZPoint destination = position.Shift(d);
		LCreature c = B.GetLCreature(destination);

		if (c != null) DoAttack(c);
		else Move(d, control);
	}

	private bool CanMove { get { return !HasEffect("Roots") && !HasEffect("Net"); } }

	public void Move(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		if (CanMove && B.IsWalkable(destination)) SetPosition(destination, 2.0f / MovementTime, true);
		else AnimateFailedMovement(d);

		RemoveEffect("Melded");

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

	public void Wait(float time)
	{
		SetPosition(position, 2.0f / time, true);
		PassTurn(time);
	}

	public void Wait() { Wait(MovementTime); }

	public override void Run()
	{
		if (!IsAIControlled)
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

	private void AddEffect(string name, float time, object parameter)
	{
		var query = effects.Where(e => e.data.name == name);
		if (query.Count() > 0)
		{
			Effect e = query.Single();
			if (e.timeLeft < time) e.timeLeft = time;
		}
		else effects.Add(new Effect(name, time, parameter));
	}

	private void AddEffect(string name, float time) { AddEffect(name, time, null); }

	private bool HasAbility(string name) { return data.HasAbility(name); }
	private bool HasEffect(string name) { return effects.Where(e => e.data.name == name).Count() > 0; }
	private Effect GetEffect(string name) { return effects.Where(e => e.data.name == name).Single(); }

	private void RemoveEffect(string name)
	{
		var query = effects.Where(e => e.data.name == name);
		if (query.Count() > 0)
		{
			Effect effect = query.Single();
			effects.Remove(effect);
		}
	}

	public void DrawEffects(ZPoint p, ZPoint descriptionP)
	{
		Screen screen = new Screen(p, new ZPoint(1, 32));
		MouseTriggerKeyword.Clear("effect");

		int i = 0;
		foreach (Effect e in effects)
		{
			screen.Draw(e.data.texture, new ZPoint(32 * i, 0), e.data.SgnColor);
			screen.DrawStringWithShading(M.smallFont, ((int)e.timeLeft).ToString(), new ZPoint(32 * i + 26, 20), Color.White);

			MouseTriggerKeyword.Set("effect", i, p + new ZPoint(32 * i, 0), new ZPoint(32, 32));
			i++;
		}

		MouseTriggerKeyword t = MouseTriggerKeyword.GetUnderMouse("effect");
		if (t != null) effects[t.parameter].data.DrawDescription(descriptionP);
	}

	private void PayAbilityCost(Ability ability)
	{
		RemoveEffect("Melded");
		RemoveEffect("Hidden");

		data.AddEndurance(-ability.cost);
	}

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
			//B.combatAnimations.Add(new ScalingAnimation(target, 1.3f, 1.0f));
		}
		else if (ability.NameIs("Marked Prey")) target.AddEffect("Marked Prey", 7);
    }

	public override void Draw()
	{
		base.Draw();

		if (HasEffect("Roots")) M.DrawRectangle(GraphicPosition + new ZPoint(0, 28), new ZPoint(32, 5), Color.DarkGreen);
	}
}
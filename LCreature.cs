﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public partial class LCreature : LObject
{
	public Creature data;
	private List<Effect> effects = new List<Effect>();
	private Dictionary<LCreature, int> damageDealt = new Dictionary<LCreature, int>();

	public bool isInParty;
	private bool isAIControlled;
	public int controlMovementCounter;

	public int HP { get { return data.hp; } }
	public int Endurance { get { return data.endurance; } }
	public bool IsAlive { get { return data.IsAlive; } }
	public List<Ability> Abilities { get { return data.Abilities; } }

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }
	private Random R { get { return World.Instance.random; } }
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

	private int BothAD
	{
		get
		{
			int result = 0;

			if (HasAbility("Lone Warrior") && B.AliveCreatures.Where(c => Distance(c) <= A.Get("Lone Warrior").range).Count() == 0)
				result += 2;

			if (HasEffect("Attention") && IsFriendTo(GetEffect("Attention").parameter as LCreature)) result += 1;

			if (HasEffect("Destined to Die")) result -= HasEffect("Attention") ? 6 : 3;
			if (HasEffect("Success Prediction Failed")) result -= HasEffect("Attention") ? 4 : 2;
			if (HasEffect("Destined to Succeed")) result += HasEffect("Attention") ? 6 : 3;
			if (HasEffect("Death Prediction Failed")) result += HasEffect("Attention") ? 4 : 2;

			return result;
        }
	}

	public int Attack
	{
		get
		{
			int result = data.Attack + BothAD;

			if (HasAbility("Bravery"))
				result += Math.Max((from c in Enemies where Distance(c) <= A.Get("Bravery").range select c).Count() - 1, 0);

			if (HasAbility("Swarm") && data is Creep)
				result += (from c in Friends where c.Name == Name && c != this && Distance(c) <= A.Get("Swarm").range select c).Count();

			if (HasEffect("True Strike")) result += 100;
			if (HasEffect("Melded")) result += 100;

			return result;
		}
	}

	public int Defence
	{
		get
		{
			int result = data.Defence + BothAD;

			int enemiesNearby = (from c in Enemies where c.position.IsAdjacentTo(position) select c).Count();
			result -= Math.Max(enemiesNearby - 1, 0);

			result += (from c in Friends where c.HasAbility("Defender") && Distance(c) <= A.Get("Defender").range select c).Count();

			if (HasEffect("Marked Prey")) result -= 3;

			return result;
		}
	}

	public int Armor { get { return 0; } }

	public bool IsAIControlled
	{
		get
		{
			if (HasEffect("Power Strike") || HasEffect("Sleeping")) return true;
			else if (HasEffect("Mind Controlled")) return false;
			else return isAIControlled;
		}
	}

	public override int Importance { get { return data.Importance; } }

	public override bool IsVisible { get { return HasEffect("Melded") || HasEffect("Hidden") ? false : true; } }

	public bool CanSee(LCreature lc)
	{
		if (!lc.IsVisible) return false;
		if (HasEffect("Blind") && Distance(lc) > 1) return false;
		if (HasEffect("Blindsight") && GetEffect("Blindsight").parameter == lc) return false;

		return true;
	}

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

	public bool IsEnemyTo(LCreature lc)
	{
		bool reference = HasEffect("Mind Tricked") ? (GetEffect("Mind Tricked").parameter as LCreature).isInParty : isInParty;
        return reference != lc.isInParty;
	}
	public bool IsFriendTo(LCreature lc) { return !IsEnemyTo(lc); }

	public List<LCreature> Enemies { get { return (from c in B.AliveCreatures where c.IsEnemyTo(this) select c).Cast<LCreature>().ToList(); } }
	public List<LCreature> Friends { get { return (from c in B.AliveCreatures where c.IsFriendTo(this) select c).Cast<LCreature>().ToList(); } }

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

	private int DamageDealtBy(LCreature lc) { return damageDealt.ContainsKey(lc) ? damageDealt[lc] : 0;	}
	private void RememberDamage(LCreature lc, int damage)
	{
		if (damageDealt.ContainsKey(lc)) damageDealt[lc] += damage;
		else damageDealt.Add(lc, damage);
	}

	public void DoDamage(LCreature lc, int damage, bool pure)
	{
		int finalDamage = pure || HasAbility("Prodigious Precision") ? damage : Math.Max(damage - lc.Armor, 0);
		lc.data.AddEndurance(-finalDamage);
		lc.RememberDamage(this, finalDamage);
		if (lc.data.endurance == 0) Kill();
		B.combatAnimations.Add(new DamageAnimation(finalDamage, B.GraphicCoordinates(lc.position), 1.0f, pure));
	}

	public int HitChance(LCreature lc)
	{
		if (HasAbility("Backstab"))
		{
			ZPoint.Direction d = (lc.position - position).GetDirection();
			LCreature behind = B.GetLCreature(position.Shift(d, 2));
			if (behind != null && behind.IsEnemyTo(lc)) return 100;
		}

		return (int)(100.0f * (Math.Max(0.0f, Math.Min(4.0f + Attack - lc.Defence, 8.0f)) / 8.0f));
	}

	public void DoAttack(LCreature lc)
	{
		AnimateAttack(lc.position);

		List<ZPoint.Direction> availableDirections = ZPoint.Directions.Where(d => B.IsWalkable(lc.position.Shift(d))).ToList();
		int n = availableDirections.Count;
		if (lc.HasAbility("Heightened Grace") && n > 0) lc.SetPosition(position.Shift(availableDirections[R.Next(n)]), 4.0f / lc.MovementTime, false);

		else if (World.Instance.random.Next(100) < HitChance(lc))
		{
			int damage = Damage;

			if (HasEffect("Power Strike")) { damage *= 3; RemoveEffect("Power Strike"); }
			if (HasEffect("Melded")) damage *= 2;
			if (HasAbility("Backstab")) damage += 3;

			DoDamage(lc, damage, false);

			RemoveEffect("Destined to Succeed");
		}

		if (HasAbility("Annoy")) lc.AddEffect("Annoyed", 5);

		RemoveEffects("True Strike", "Hidden", "Melded", "Fake Death");
		PassTurn(AttackTime);
	}

	public void MoveOrAttack(ZPoint.Direction d, bool control)
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

		RemoveEffects("Melded", "Fake Death");

		if (control == true && controlMovementCounter > 0)
		{
			controlMovementCounter--;
			ContinueTurn(MovementTime);
		}
		else PassTurn(MovementTime);
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
		foreach (Effect e in effects.Where(f => f.timeLeft <= 0).ToList())
		{
			effects.Remove(e);

			if (e.NameIs("Destined to Die")) AddEffect("Death Prediction Failed", 10);
			else if (e.NameIs("Destined to Succeed")) AddEffect("Success Prediction Failed", 10);
		}

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

	private void RemoveEffects(params string[] names) { foreach (string name in names) RemoveEffect(name); }

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

	public override void Draw()
	{
		base.Draw();

		if (HasEffect("Roots")) M.DrawRectangle(GraphicPosition + new ZPoint(0, 28), new ZPoint(32, 5), Color.DarkGreen);
	}
}
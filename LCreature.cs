using System;
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
	public int Stamina { get { return data.stamina; } }
	public bool IsAlive { get { return data.IsAlive; } }
	public List<CAbility> Abilities { get { return data.Abilities; } }

	private Battlefield B { get { return World.Instance.battlefield; } }
	private MainScreen M { get { return MainScreen.Instance; } }
	private Random R { get { return World.Instance.random; } }
	private GeneralBase<CAbility> A { get { return BigBase.Instance.abilities; }	}

	protected override void Init()
	{
		initiative = 0.0f;
		controlMovementCounter = 3;
		base.Init();
	}

	public override bool IsWalkable { get { return !IsAlive; } }
	public override bool IsFlat { get { return !IsAlive; } }
	public override string Name { get { return data.Name; } }
	public string UniqueName { get { return data.UniqueName; } }

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
			if (HasEffect("net")) result -= 2;

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

	public int Armor { get { return data.Armor; } }

	public bool IsAIControlled
	{
		get
		{
			if (HasOneOfEffects("Power Strike", "Sleeping", "Unconscious", "Paralyzed")) return true;
			else if (HasEffect("Mind Controlled")) return false;
			else return isAIControlled;
		}
	}

	public override int Importance { get { return IsAlive ? data.Importance : 5; } }

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

	public override Color RelationshipColor { get {	return isInParty ? Color.Green : Color.Red;	} }
	public Color LogColor { get { return isInParty ? Color.White : Color.Orange; } }

	public bool IsEnemyTo(LCreature lc)
	{
		LCreature reference = this;
		if (HasEffect("Mind Controlled")) reference = GetEffect("Mind Controlled").parameter as LCreature;
		else if (HasEffect("Mind Tricked")) reference = GetEffect("Mind Tricked").parameter as LCreature;

		if (lc == this) return false;
		else return reference.isInParty != lc.isInParty;
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
		B.combatAnimations.Add(new RMove(rPosition, v, 0.5f * MovementTime));
		B.combatAnimations.Add(new RMove(rPosition, -v, 0.5f * MovementTime));
	}

	private void AnimateAttack(ZPoint p, float gameTime)
	{
		Vector2 v = p - position;
		v.Normalize();
		v *= 0.5f;

		B.combatAnimations.Add(new RMove(rPosition, v, 0.5f * gameTime));
		B.combatAnimations.Add(new RMove(rPosition, -v, 0.5f * gameTime));
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

		lc.data.AddHP(-finalDamage);
		if (lc.Stamina > lc.HP) lc.data.AddStamina(lc.HP - lc.Stamina);

		lc.RememberDamage(this, finalDamage);
		if (lc.data.hp == 0) lc.Kill();
		B.combatAnimations.Add(new DamageAnimation(finalDamage, Battlefield.GC(lc.position), 1.0f, pure));
	}

	public int HitChance(LCreature lc) { return (int)(100.0f * (Math.Max(0.0f, Math.Min(4.0f + Attack - lc.Defence, 8.0f)) / 8.0f)); }

	public void DoAttack(LCreature lc)
	{
		AnimateAttack(lc.position, AttackTime);

		B.log.AddLine(UniqueName, LogColor);
		B.log.Add(" attacks " + lc.UniqueName, Color.Pink);

		int hitChance = HitChance(lc);
		int damage = Damage;
		ZPoint.Direction direction = (lc.position - position).GetDirection();

		if (lc.HasEffect("Sleeping")) hitChance = 100;
		if (HasAbility("Backstab") && Distance(lc) == 1)
		{
			LCreature behind = B.GetLCreature(position.Shift(direction, 2));
			if (behind != null && behind.IsEnemyTo(lc))
			{
				hitChance = 100;
				damage += 3;
				B.log.Add(" from behind");
			}
		}

		List<ZPoint.Direction> availableDirections = ZPoint.Directions.Where(d => B.IsWalkable(lc.position.Shift(d))).ToList();
		int n = availableDirections.Count;

		if (lc.HasAbility("Heightened Grace") && n > 0)
		{
			lc.SetPosition(lc.position.Shift(availableDirections[R.Next(n)]), lc.MovementTime, false);
			B.log.Add(" but " + lc.UniqueName + " was ready for that!");
		}
		else if (World.Instance.random.Next(100) < hitChance)
		{
			if (HasEffect("Power Strike")) { damage *= 3; RemoveEffect("Power Strike"); }
			if (HasEffect("Melded")) damage *= 2;

			DoDamage(lc, damage, false);

			if(HasItemAbility("Cleave") && Distance(lc) == 1)
			{
				ZPoint[] secondaryTargets = new ZPoint[2];
				secondaryTargets[0] = lc.position + ZPoint.Next(direction);
				secondaryTargets[1] = lc.position + ZPoint.Previous(direction);
				foreach (ZPoint p in secondaryTargets)
				{
					LCreature secondaryTarget = B.GetLCreature(p);
					if (secondaryTarget != null) DoDamage(secondaryTarget, damage, false);
				}
			}

			if (HasEffect("Poisoned Weapon"))
			{
				Effect e = GetEffect("Poisoned Weapon");
				if (e.parameter as string == "Paralyzing Poison") lc.AddEffect("Paralyzed", 3);
			}

			RemoveEffect("Destined to Succeed");

			B.log.Add(" and deals " + Math.Max(0, damage - lc.Armor) + " damage.");
		}
		else B.log.Add(" and misses.");

		if (HasAbility("Annoy")) lc.AddEffect("Annoyed", 5);

		RemoveEffects("True Strike", "Hidden", "Melded", "Fake Death");
		lc.RemoveEffect("Sleeping");

		PassTurn(AttackTime);
	}

	public void MoveOrAttack(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		LCreature c = B.GetLCreature(destination);

		if (c != null) DoAttack(c);
		else Move(d, control);
	}

	private bool CanMove { get { return !HasOneOfEffects("Roots", "Net"); } }

	public void Move(ZPoint.Direction d, bool control)
	{
		ZPoint destination = position.Shift(d);
		if (CanMove && B.IsWalkable(destination)) SetPosition(destination, MovementTime, true);
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
		SetPosition(position, time, true);
		PassTurn(time);
	}

	public void Wait() { Wait(MovementTime); }

	public override void Run()
	{
		if (Stamina == 0) AddEffect("Sleeping", 1);

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
		foreach (Effect e in effects.Where(f => f.timeLeft <= 0).ToList()) RemoveEffect(e.data.name);

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
		else
		{
			Effect e = new Effect(name, time, parameter);
            effects.Add(e);

			if (e.NameIs("True Strike", "Blind", "Fake Death", "Sleeping", "Blindsight", "Mind Tricked", "Mind Controlled"))
				B.log.AddLine(UniqueName + " ", LogColor);

			if (e.NameIs("Roots"))
			{
				B.log.AddLine("Multiple roots crawl from the ground and entangle " + UniqueName + "! ", Color.Pink);
				B.log.Add(UniqueName, LogColor);
				B.log.Add(" can't move!", Color.Pink);
			}
			else if (e.NameIs("True Strike")) B.log.Add("feels confident.", Color.Pink);
			else if (e.NameIs("Blind")) B.log.Add("can't see anything!", Color.Pink);
			else if (e.NameIs("Fake Death")) B.log.Add("looks completely dead.", Color.Pink);
			else if (e.NameIs("Sleeping")) B.log.Add("falls asleep.", Color.Pink);
			else if (e.NameIs("Blindsight")) B.log.Add("can't see " + (e.parameter as LCreature).UniqueName + " now.", Color.Pink);
			else if (e.NameIs("Mind Tricked")) B.log.Add("is now fighting for the wrong party!", Color.Pink);
			else if (e.NameIs("Mind Controlled")) B.log.Add("is now controlled by " + (e.parameter as LCreature).UniqueName + "!", Color.Pink);
		}
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
			Effect e = query.Single();

			if (e.NameIs("Destined to Die"))
			{
				AddEffect("Death Prediction Failed", 10);

				B.log.AddLine((e.parameter as LCreature).UniqueName + "'s death prediction failed. ", Color.Pink);
				B.log.Add(UniqueName, LogColor);
				B.log.Add(" feels relieved.", Color.Pink);
			}
			else if (e.NameIs("Destined to Succeed"))
			{
				AddEffect("Success Prediction Failed", 10);

				B.log.AddLine((e.parameter as LCreature).UniqueName + "'s success prediction failed. ", Color.Pink);
				B.log.Add(UniqueName, LogColor);
				B.log.Add(" feels depressed.", Color.Pink);
			}
			else if (e.NameIs("Net")) B.Add(new LItem("Net"), position);
			else
			{
				B.log.AddLine(UniqueName, LogColor);
				B.log.Add(" is no longer ", Color.Pink);
				B.log.Add(e.data.name + ".", e.data.SgnColor);
			}

			effects.Remove(e);
		}
	}

	private void RemoveEffects(params string[] names) { foreach (string name in names) RemoveEffect(name); }
	private bool HasOneOfEffects(params string[] names)
	{
		foreach (string name in names) if (HasEffect(name)) return true;
		return false;
	}

	public void DrawEffects(ZPoint p, ZPoint descriptionP)
	{
		Screen screen = new Screen(p, new ZPoint(1, 32));
		MouseTriggerKeyword.Clear("effect");

		int i = 0;
		foreach (Effect e in effects)
		{
			screen.Draw(e.data.texture, new ZPoint(32 * i, 0), e.data.SgnColor);
			screen.DrawStringWithShading(M.fonts.small, ((int)e.timeLeft).ToString(), new ZPoint(32 * i + 26, 20), Color.White);

			MouseTriggerKeyword.Set("effect", i.ToString(), p + new ZPoint(32 * i, 0), new ZPoint(32, 32));
			i++;
		}

		var mtk = MouseTriggerKeyword.GetUnderMouse("effect");
		if (mtk != null) effects[int.Parse(mtk.parameter)].data.DrawDescription(descriptionP);
	}

	public override void Draw()
	{
		base.Draw();
		if (!IsAlive) return;

		Action<string> draw = textureName => Battlefield.Draw(textureName, rPosition);

		if (HasEffect("Net")) draw("net");
				
		if (HasEffect("Unconscious")) draw("otherEffect");
		else if (HasEffect("Sleeping")) draw("sleeping");
		else if (HasEffect("Mind Controlled")) draw("psionicEffect");
		else if (HasEffect("Mind Tricked")) draw("questionMark");
		else if (HasEffect("Power Strike")) draw("timeEffect");
		else if (HasOneOfEffects("Blind", "Blindsight")) draw("visionEffect");
		else if (HasOneOfEffects("Destined to Die", "Success Prediction Failed", "Marked Prey")) draw("negativeEffect");
		else if (HasOneOfEffects("Destined to Succeed", "Death Prediction Failed", "Faked Death", "True Strike")) draw("positiveEffect");
		else if (HasOneOfEffects("Annoyed", "Attention")) draw("aggroEffect");

		if (HasEffect("Roots")) M.DrawRectangle(GraphicPosition + new ZPoint(0, 28), new ZPoint(32, 5), Color.DarkGreen);
	}

	public Inventory Ground
	{
		get
		{
			Inventory result = new Inventory(3, 1, null, "ground");
			int pickupDistance = 2;
			List<LItem>[] list = new List<LItem>[pickupDistance];
			for (int k = 0; k < pickupDistance; k++)
			{
				list[k] = B.Items.Where(i => Distance(i.position) == k).ToList();
				for (int i = 0; i < list[k].Count && i < 3; i++) result.Add(list[k][i].data, i);
			}
			return result;
		}
	}

	public bool HasItemAbility(string name)
	{
		if (!(data is Character)) return false;

		Inventory i = (data as Character).inventory;
		Ability a = BigBase.Instance.iAbilityTypes.Get(name);

		foreach (Item item in i.Items) if (item.data.ability != null && item.data.ability.name == a.name) return true;
		return false;
	}
}
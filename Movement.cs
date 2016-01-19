using Microsoft.Xna.Framework;

public class Movement : LocalComponent
{
	public Movement(LocalObject o) : base(o) { }

	public float Time
	{
		get
		{
			float result = t.shape != null ? t.shape.data.movementTime : 1;
			if (t.inventory != null) result *= t.inventory.Prod(b => b.mtm) * t.inventory.TimeMultiplier;
			return result;
		}
	}

	private void AnimateFailedMovement(ZPoint.Direction d)
	{
		Vector2 v = 0.25f * (Vector2)(ZPoint.Zero.Shift(d));
		B.combatAnimations.Add(new RMove(t.p.r, v, 0.5f * Time));
		B.combatAnimations.Add(new RMove(t.p.r, -v, 0.5f * Time));
	}

	private bool CanMove { get { return !t.HasEffect("Roots", "Net"); } }

	public void Move(ZPoint.Direction d, bool control)
	{
		ZPoint destination = t.p.value.Shift(d);
		if (CanMove && B.IsWalkable(destination, t)) t.p.Set(destination, Time, true);
		else AnimateFailedMovement(d);

		t.RemoveEffect("Melded", "Fake Death");

		if (control == true && t.initiative.movementCounter > 0)
		{
			t.initiative.movementCounter--;
			t.initiative.ContinueTurn(Time);
		}
		else t.initiative.PassTurn(Time);
	}

	public void MoveOrAttack(ZPoint.Direction d, bool control)
	{
		LocalObject o = B.Get(t.p.value.Shift(d));

		if (o != null && !o.p.IsWalkable)
		{
			if (o.shape != null && o.shape.data.name == "Chest" && t.skills != null && t.skills["Mechanisms"] > 0)
			{
				foreach (Item item in o.inventory.Items) B.Add(new LocalObject(item), o.p.value);
				B.Remove(o);
			}
			else if (o.hp != null && t.attack != null) t.attack.Execute(o);
		}
		else t.movement.Move(d, control);
	}
}

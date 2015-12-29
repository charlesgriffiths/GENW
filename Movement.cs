using Microsoft.Xna.Framework;

public class Movement : LocalComponent
{
	public int counter;

	public Movement(LocalObject o) : base(o)
	{
		counter = 3;
	}

	public float Time
	{
		get
		{
			float result = t.shape != null ? t.shape.movementTime : 1;
			if (t.inventory != null) result *= t.inventory.Prod(b => b.mtm);
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
		if (CanMove && B.IsWalkable(destination)) t.p.Set(destination, Time, true);
		else AnimateFailedMovement(d);

		t.RemoveEffect("Melded", "Fake Death");

		if (control == true && counter > 0)
		{
			counter--;
			t.initiative.ContinueTurn(Time);
		}
		else t.initiative.PassTurn(Time);
	}
}

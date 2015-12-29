using System;
using Microsoft.Xna.Framework;

public class LocalDrawing : LocalComponent
{
	public float scaling;

	public LocalDrawing(LocalObject o) : base(o)
	{
		scaling = 1;
	}

	public void Draw()
	{
		Log.Assert(t.p != null, "KDrawing.Draw");
		B.Draw(t.GetTexture, t.p.r, scaling, t.p.IsVisible ? Color.White : new Color(0.5f, 0.5f, 0.5f, 0.5f));

		Action<string> draw = textureName => Battlefield.Draw(textureName, t.p.r);

		if (t.HasEffect("Net")) draw("net");

		if (t.HasEffect("Unconscious")) draw("otherEffect");
		else if (t.HasEffect("Sleeping")) draw("sleeping");
		else if (t.HasEffect("Mind Controlled")) draw("psionicEffect");
		else if (t.HasEffect("Mind Tricked")) draw("questionMark");
		else if (t.HasEffect("Power Strike")) draw("timeEffect");
		else if (t.HasEffect("Blind", "Blindsight")) draw("visionEffect");
		else if (t.HasEffect("Destined to Die", "Success Prediction Failed", "Marked Prey")) draw("negativeEffect");
		else if (t.HasEffect("Destined to Succeed", "Death Prediction Failed", "Faked Death", "True Strike")) draw("positiveEffect");
		else if (t.HasEffect("Annoyed", "Attention")) draw("aggroEffect");

		if (t.HasEffect("Roots")) M.DrawRectangle(t.p.GC + new ZPoint(0, 28), new ZPoint(32, 5), Color.DarkGreen);
	}
}

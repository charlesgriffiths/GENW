using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public partial class Battlefield
{
	public static Vector2 ScreenPosition { get { return new Vector2(16 + 96, 8); } }
	public static Vector2 GraphicCoordinates(RPoint p) { return ScreenPosition + new Vector2(32 * p.x, 32 * p.y); }
	public static Vector2 GC(RPoint p) { return GraphicCoordinates(p); }
	public static ZPoint ZCoordinates(Vector2 mouse) { return new ZPoint((mouse - ScreenPosition) / 32.0f); }

	private static void Draw(Texture2D texture, RPoint rPosition, Color color) {
		M.Draw(texture, new ZPoint(GraphicCoordinates(rPosition) - new Vector2(texture.Width/2 - 16, texture.Height - 32)), color); }
	public static void Draw(Texture2D texture, RPoint rPosition) { Draw(texture, rPosition, Color.White); }
	public void Draw(Texture2D texture, RPoint rPosition, float scaling, Color color)
	{
		if (scaling == 1.0f) Draw(texture, rPosition, color);
		else
		{
			int width = (int)(scaling * texture.Width);
			int height = (int)(scaling * texture.Height);
			ZPoint center = new ZPoint(GraphicCoordinates(rPosition)) + new ZPoint(16, 32 - height / 2);
			M.spriteBatch.Draw(texture, new Rectangle(center.x - width/2, center.y - height/2, width, height), color);
		}
	}

	private void DrawZones()
	{
		if (ability == null)
		{
			foreach (ZPoint p in GreenZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), 
				new ZPoint(32, 32), new Color(0, 0.3f, 0, 0.1f));
			foreach (ZPoint p in YellowZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), 
				new ZPoint(32, 32), new Color(0.3f, 0.3f, 0, 0.1f));

			foreach (ZPoint p in ReachableCreaturePositions)
				M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0.3f, 0.3f, 0, 0.1f));

			if (Mouse.IsIn(TotalZone))
			{
				List<ZPoint.Direction> path = Path(current.p.value, Mouse);
				expectedInitiative = current.initiative.value - path.Count * current.movement.Time;
				DrawPath(current.p.value, path, null);
			}
			else if (Mouse.IsIn(ReachableCreaturePositions))
			{
				List<ZPoint.Direction> path = Path(current.p.value, Mouse);
				expectedInitiative = current.initiative.value - (path.Count - 1) * current.movement.Time - current.attack.Time;
				DrawPath(current.p.value, path, Get(Mouse));
			}
		}
		else
		{
			foreach (ZPoint p in AbilityZone)
				M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0, 0.3f, 0, 0.1f));

			expectedInitiative = current.initiative.value - ability.castTime;
		}
	}

	private void DrawScale(bool horizontal)
	{
		ZPoint position = horizontal ? new ZPoint(128, M.size.y - 68) : new ZPoint(16 + 32, 8 + 16 + 32);
		int length = horizontal ? 736 : 720 - 16 - 64 - 32, height = horizontal ? 20 : 16;
		Screen screen = new Screen(position, new ZPoint(height, length).Transpose(horizontal));

		screen.Fill(Stuff.MyColor("Dark Grey"));

		var query = ActiveObjects.OrderBy(lc => lc.initiative.r.x);
		float zeroInitiative = -query.Last().initiative.r.x;

		Func<float, int> func = f => (int)(100.0f * (-f - zeroInitiative)) + 1;
		Func<LocalObject, ZPoint> yz = lc => horizontal == lc.team.isInParty ? new ZPoint(height, 0) : new ZPoint(-32, -32);
		Func<LocalObject, ZPoint> iconPosition = lc => new ZPoint(yz(lc).x, func(lc.initiative.r.x) + 1).Transpose(horizontal);

		foreach (LocalObject lc in query) MouseTriggerObject<LocalObject>.Set(lc, screen.position + iconPosition(lc), new ZPoint(32, 32));
		var trigger = MouseTrigger.GetUnderMouse<MouseTriggerObject<LocalObject>>();

		foreach (LocalObject lc in query)
		{
			Color color = lc.p.TheSameAs(Mouse) || (trigger != null && trigger.t == lc) ? lc.RelationshipColor : Color.White;

			if (scaleAnimations.CurrentTarget != lc.initiative.r)
				screen.DrawRectangle(new ZPoint(yz(lc).y, func(lc.initiative.r.x)).Transpose(horizontal), 
				new ZPoint(height + 32, 1).Transpose(horizontal), color);

			screen.Draw(lc.GetTexture, iconPosition(lc));
		}

		if (expectedInitiative < 0)
			screen.DrawRectangle(new ZPoint(0, func(expectedInitiative)).Transpose(horizontal),
			new ZPoint(height + 32, 1).Transpose(horizontal), Color.DodgerBlue);

		expectedInitiative = 0.0f;

		if (trigger != null) Draw(NamedTexture.Get("other/zSelection"), trigger.t.p.value);
		MouseTrigger.Clear<MouseTriggerObject<LocalObject>>();
	}

	private void DrawEndButton()
	{
		int length = 128, height = 32;
		Screen screen = new Screen(new ZPoint(M.size.x - 8 - 144 - length / 2, M.size.y - 8 - height), new ZPoint(length, height));

		MouseTriggerKeyword.Clear("End Battle");
		Resolution resolution = GetResolution();
        if (resolution != Resolution.Not) MouseTriggerKeyword.Set("End Battle", screen.position, screen.size);
		MouseTriggerKeyword mt = MouseTriggerKeyword.GetUnderMouse("End Battle");

		screen.Fill(Stuff.MyColor("Dark Grey"));

		string text = resolution == Resolution.Victory ? "Victory!" : resolution == Resolution.Retreat ? "Run!" : "End Battle";
		screen.DrawString(M.fonts.verdanaBold, text, 9, 
			resolution == Resolution.Not ? Color.Gray : mt != null ? Color.Red : Color.White);
	}

	public void Draw()
	{
		M.Fill(new Color(0, 0, 0, 0.6f));

		foreach (LocalObject l in objects)
		{
			l.p.animations.Draw();
			if (l.initiative != null) l.initiative.animations.Draw();
		}

		for (int j = 0; j < Size.y; j++)
			for (int i = 0; i < Size.x; i++)
			{
				ZPoint p = new ZPoint(i, j);
				Draw(this[p].texture[data[i, j].variation], p);

				foreach (var d in ZPoint.Directions)
				{
					ZPoint q = p + d;
					if (!InRange(q)) continue;
					bool draw = (!this[p].IsWalkable && !this[p].IsFlat && this[q].IsWalkable && this[q].IsFlat) || 
						(this[p].name != "Sky" && this[q].name == "Sky");
					if (draw) Draw(NamedTexture.Get("local/wall mask " + (int)d), p);
				}
			}

		if (combatAnimations.IsEmpty) DrawZones();

		var query = objects.OrderBy(o => !o.p.IsWalkable).ThenBy(o => o.p.y).ThenBy(o => -o.Importance);
		foreach (LocalObject l in query) l.drawing.Draw();

		combatAnimations.Draw();
		scaleAnimations.Draw();

		if (ability != null && Mouse.IsIn(AbilityZone)) Draw(NamedTexture.Get("other/target"), Mouse);
		else if (InRange(Mouse)) Draw(NamedTexture.Get("other/zSelection"), Mouse);
		if (spotlight != null && spotlight != current) M.Draw(NamedTexture.Get("other/zSelection"), spotlight.p.GC);

		foreach (DelayedDrawing dd in delayedDrawings) dd.Draw();
		delayedDrawings.Clear();

		DrawScale(false);
		spotlight.p.DrawInfo(new ZPoint(M.size.x - 288 - 8, 8));
		if (MyGame.Instance.combatLog) log.Draw();
		DrawEndButton();
	}
}

class DelayedDrawing //используется при рисовании вероятности попадания
{
	private SpriteFont font;
	private string text;
	private ZPoint position;
	private Color color;

	public DelayedDrawing(SpriteFont fonti, string texti, ZPoint positioni, Color colori)
	{ font = fonti; text = texti; position = positioni; color = colori; }

	public void Draw() { MainScreen.Instance.DrawString(font, text, position, color); }
}
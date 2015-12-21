using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public partial class Battlefield
{
	public static Vector2 ScreenPosition { get { return new Vector2(16 + 96, 8)/*Vector2(8, 8)*/; } }
	public static Vector2 GraphicCoordinates(RPoint p) { return ScreenPosition + new Vector2(32 * p.x, 32 * p.y); }
	public static Vector2 GC(RPoint p) { return GraphicCoordinates(p); }
	public static ZPoint ZCoordinates(Vector2 mouse) { return new ZPoint((mouse - ScreenPosition) / 32.0f); }

	public void LoadTextures()
	{
		arrowTexture = M.game.Content.Load<Texture2D>("other/arrow");
		targetTexture = M.game.Content.Load<Texture2D>("other/target");
		damageIcon = M.game.Content.Load<Texture2D>("other/damage");
		armorIcon = M.game.Content.Load<Texture2D>("other/armor");
	}

	private static void Draw(Texture2D texture, RPoint rPosition, Color color)
	{ M.Draw(texture, new ZPoint(GraphicCoordinates(rPosition) - new Vector2(texture.Width/2 - 16, texture.Height - 32)), color); }

	public static void Draw(Texture2D texture, RPoint rPosition) { Draw(texture, rPosition, Color.White); }
	public static void Draw(string textureName, RPoint rPosition) { Draw(BigBase.Instance.textures.Get(textureName).Single(), rPosition); }

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
			foreach (ZPoint p in GreenZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0, 0.3f, 0, 0.1f));
			foreach (ZPoint p in YellowZone) M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0.3f, 0.3f, 0, 0.1f));

			foreach (ZPoint p in ReachableCreaturePositions)
				M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0.3f, 0.3f, 0, 0.1f));

			if (Mouse.IsIn(TotalZone))
			{
				List<ZPoint.Direction> path = Path(CurrentLCreature.position, Mouse);
				expectedInitiative = CurrentLCreature.initiative - path.Count * CurrentLCreature.MovementTime;
				DrawPath(CurrentLCreature.position, path, null);
			}
			else if (Mouse.IsIn(ReachableCreaturePositions))
			{
				List<ZPoint.Direction> path = Path(CurrentLCreature.position, Mouse);
				expectedInitiative = CurrentLCreature.initiative - (path.Count - 1) * CurrentLCreature.MovementTime - CurrentLCreature.AttackTime;
				DrawPath(CurrentLCreature.position, path, GetLCreature(Mouse));
			}
		}
		else
		{
			foreach (ZPoint p in AbilityZone)
				M.DrawRectangle(new ZPoint(GraphicCoordinates(p)), new ZPoint(32, 32), new Color(0, 0.3f, 0, 0.1f));

			expectedInitiative = CurrentLCreature.initiative - ability.castTime;
		}
	}

	private void DrawScale(bool horizontal)
	{
		ZPoint position = horizontal ? new ZPoint(128, M.size.y - 68) : new ZPoint(16 + 32, 8 + 16 + 32);
		int length = horizontal ? 736 : 720 - 16 - 64 - 32, height = horizontal ? 20 : 16;
		Screen screen = new Screen(position, new ZPoint(height, length).Transpose(horizontal));

		screen.Fill(Stuff.MyColor("Dark Grey"));

		var query = AliveCreatures.OrderBy(lc => lc.rInitiative.x);
		float zeroInitiative = -query.Last().rInitiative.x;

		Func<float, int> func = f => (int)(100.0f * (-f - zeroInitiative)) + 1;
		Func<LCreature, ZPoint> yz = lc => horizontal == lc.isInParty ? new ZPoint(height, 0) : new ZPoint(-32, -32);
		Func<LCreature, ZPoint> iconPosition = lc => new ZPoint(yz(lc).x, func(lc.rInitiative.x) + 1).Transpose(horizontal);

		foreach (LCreature lc in query) MouseTriggerObject<LCreature>.Set(lc, screen.position + iconPosition(lc), new ZPoint(32, 32));
		var trigger = MouseTrigger.GetUnderMouse<MouseTriggerObject<LCreature>>();

		foreach (LCreature lc in query)
		{
			Color color = lc.position.TheSameAs(Mouse) || (trigger != null && trigger.t == lc) ? lc.RelationshipColor : Color.White;

			if (scaleAnimations.CurrentTarget != lc.rInitiative)
				screen.DrawRectangle(new ZPoint(yz(lc).y, func(lc.rInitiative.x)).Transpose(horizontal), 
				new ZPoint(height + 32, 1).Transpose(horizontal), color);

			screen.Draw(lc.texture, iconPosition(lc));
		}

		if (expectedInitiative < 0)
			screen.DrawRectangle(new ZPoint(0, func(expectedInitiative)).Transpose(horizontal),
			new ZPoint(height + 32, 1).Transpose(horizontal), Color.DodgerBlue);

		expectedInitiative = 0.0f;

		if (trigger != null) Draw(M.zSelectionTexture, trigger.t.position);
		MouseTrigger.Clear<MouseTriggerObject<LCreature>>();
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
		screen.DrawString(M.fonts.verdanaBold, text, 9, resolution == Resolution.Not ? Color.Gray : mt != null ? Color.Red : Color.White);
	}

	public void Draw()
	{
		M.Fill(new Color(0, 0, 0, 0.8f));

		foreach (LObject l in objects)
		{
			l.movementAnimations.Draw();
			l.scaleAnimations.Draw();
		}

		for (int j = 0; j < Size.y; j++)
			for (int i = 0; i < Size.x; i++)
			{
				ZPoint p = new ZPoint(i, j);
				M.Draw(this[p].texture, GraphicCoordinates(p));
			}

		if (combatAnimations.IsEmpty) DrawZones();

		var query = objects.OrderBy(o => o.position.y).ThenBy(o => -o.Importance);
		foreach (LObject l in query) l.Draw();

		combatAnimations.Draw();
		scaleAnimations.Draw();

		if (ability != null && Mouse.IsIn(AbilityZone)) Draw(targetTexture, Mouse);
		else if (InRange(Mouse)) Draw(M.zSelectionTexture, Mouse);
		if (spotlightObject != null && spotlightObject != currentObject) M.Draw(M.zSelectionTexture, GraphicCoordinates(spotlightObject.rPosition));

		foreach (DelayedDrawing dd in delayedDrawings) dd.Draw();
		delayedDrawings.Clear();

		DrawScale(false);
		(spotlightObject as LCreature).DrawInfo(new ZPoint(M.size.x - 288 - 8, 8));
		if (MyGame.Instance.combatLog) log.Draw();
		DrawEndButton();
	}
}

public partial class LCreature : LObject
{
	public void DrawInfo(ZPoint position)
	{
		int length = 288, height = 124 + 48;
		Screen screen = new Screen(position, new ZPoint(length, height));
		screen.Fill(Color.Black);

		float hpFraction = (float)HP / MaxHP;
		float staminaFraction = (float)Stamina / MaxHP;

		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(hpFraction * length), 20), new Color(0.2f, 0, 0));
		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(staminaFraction * length), 20), new Color(0.4f, 0, 0));
		for (int i = 1; i <= MaxHP; i++) screen.DrawRectangle(new ZPoint((int)(i * (float)length / MaxHP), 0), new ZPoint(1, 20), Color.Black);

		SpriteFont font = M.fonts.verdanaBold;
		screen.DrawString(font, data.FullName, 23, Color.White);

		screen.Draw(B.damageIcon, new ZPoint(0, 40));
		screen.DrawString(font, Damage.ToString(), new ZPoint(22, 43), Color.White);
		screen.DrawString(font, Attack.ToString() + "/" + Defence, 43, Color.White);
		screen.DrawString(font, Armor.ToString(), new ZPoint(length - 32, 43), Color.White);
		screen.Draw(B.armorIcon, new ZPoint(length - 20, 40));

		DrawEffects(screen.position + new ZPoint(0, 60), screen.position + new ZPoint(24, 180));
		if (data is Character) (data as Character).inventory.Draw(screen.position + new ZPoint(0, 92));
		Ground.Draw(screen.position + new ZPoint(192, 92));
		DrawAbilities(screen, new ZPoint(0, 124));
	}

	private void DrawAbilities(Screen screen, ZPoint position)
	{
		Func<int, ZPoint> aPosition = k => screen.position + position + new ZPoint(48 * k, 0);
		ZPoint aSize = new ZPoint(48, 48);

		for (int n = 0; n < 6; n++) MouseTriggerKeyword.Set("ability", n.ToString(), aPosition(n), aSize);
		var mtk = MouseTriggerKeyword.GetUnderMouse("ability");

		int i = 0;
		foreach (CAbility a in Abilities)
		{
			bool mouseOn = mtk != null && mtk.parameter == i.ToString();

			M.Draw(a.texture, aPosition(i), mouseOn ? a.color : Color.White);

			if (a.targetType == Ability.TargetType.Passive) M.DrawRectangle(aPosition(i), aSize, new Color(0, 0, 0, 0.7f));

			else if (this == B.CurrentLCreature) M.DrawStringWithShading(M.fonts.small, Stuff.AbilityHotkeys[i].ToString(),
				aPosition(i)/* + new ZPoint(37, 33)*/, Color.White);

			if (mouseOn) a.DrawDescription(screen.position + position + new ZPoint(24, 56));

			i++;
		}
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
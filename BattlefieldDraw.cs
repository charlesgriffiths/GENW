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

	private void DrawScale(ZPoint position)
	{
		bool horizontal = false;
		int length = horizontal ? 736 : 720 - 16 - 64 - 32, height = horizontal ? 20 : 16;
		Screen screen = new Screen(position, new ZPoint(horizontal ? length : height, horizontal ? height : length));

		screen.Fill(Stuff.MyColor("Dark Grey"));

		var query = from c in AliveCreatures orderby c.rInitiative.x select c;
		float zeroInitiative = -query.Last().rInitiative.x;
		Func<float, int> func = f => (int)(100.0f * (-f - zeroInitiative)) + 1;

		MouseTriggerLCreature trigger = null;
		foreach (LCreature c in query)
		{
			int rInitiative = func(c.rInitiative.x); //(int)(150.0f * (-c.rInitiative.x - zeroInitiative)) + 1;
			int y = -32, z = -32;
			if (horizontal == c.isInParty) { y = height; z = 0; }

			ZPoint iconPosition = new ZPoint(y, rInitiative + 1);
			if (horizontal) iconPosition = new ZPoint(rInitiative + 1, y);

			MouseTriggerLCreature.Set(c, screen.position + iconPosition, new ZPoint(32, 32));
			trigger = MouseTriggerLCreature.GetUnderMouse();

			Color color = Color.White;
			if (c.position.TheSameAs(Mouse) || (trigger != null && c == trigger.creature)) color = c.RelationshipColor;

			if (scaleAnimations.CurrentTarget != c.rInitiative)
			{
				if (horizontal) screen.DrawRectangle(new ZPoint(rInitiative, z), new ZPoint(1, height + 32), color);
				else screen.DrawRectangle(new ZPoint(z, rInitiative), new ZPoint(height + 32, 1), color);
			}

			screen.Draw(c.texture, iconPosition);
		}

		if (expectedInitiative < 0)
		{
			if (horizontal) screen.DrawRectangle(new ZPoint(func(expectedInitiative), 0), new ZPoint(1, height + 32), Color.DodgerBlue);
			else screen.DrawRectangle(new ZPoint(0, func(expectedInitiative)), new ZPoint(height + 32, 1), Color.DodgerBlue);
		}

		expectedInitiative = 0.0f;

		if (trigger != null) Draw(M.zSelectionTexture, trigger.creature.position);
		MouseTriggerLCreature.Clear();
	}

	private void DrawAbilities(LCreature c, Screen screen, ZPoint position) // эту штуку нужно будет немного переписать, пока криво
	{
		for (int i = 0; i < 6; i++) MouseTriggerKeyword.Set("ability", i, screen.position + position + new ZPoint(48 * i, 0), new ZPoint(48, 48));

		foreach (Ability a in c.Abilities)
		{
			int i = c.Abilities.IndexOf(a);
			MouseTriggerKeyword t = MouseTriggerKeyword.Get("ability", i);
			M.Draw(a.texture, t.position);

			if (a.targetType == Ability.TargetType.Passive) M.DrawRectangle(t.position, t.size, new Color(0, 0, 0, 0.7f));
			else if (c == CurrentLCreature) M.DrawStringWithShading(M.fonts.small, Stuff.AbilityHotkeys[i].ToString(), t.position + new ZPoint(37, 33), Color.White);
		}

		MouseTriggerKeyword forDescription = MouseTriggerKeyword.GetUnderMouse("ability");
		if (forDescription != null && forDescription.parameter < c.Abilities.Count)
		{
			Ability a = c.Abilities[forDescription.parameter];
			M.Draw(a.texture, forDescription.position, a.color);

			if (c == CurrentLCreature && a.targetType != Ability.TargetType.Passive)
				M.DrawStringWithShading(M.fonts.small, Stuff.AbilityHotkeys[forDescription.parameter].ToString(),
				forDescription.position + new ZPoint(37, 33), Color.White);

			a.DrawDescription(screen.position + position + new ZPoint(24, 56));
		}
	}

	private void DrawInfo(LCreature c, ZPoint position)
	{
		int length = 288, height = 1;
		Screen screen = new Screen(position, new ZPoint(length, height));
		//screen.Fill(Color.Black);

		float hpFraction = (float)c.HP / c.MaxHP;
		float staminaFraction = (float)c.Stamina / c.MaxHP;

		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(hpFraction * length), 20), new Color(0.2f, 0, 0));
		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(staminaFraction * length), 20), new Color(0.4f, 0, 0));
		for (int i = 1; i <= c.MaxHP; i++) screen.DrawRectangle(new ZPoint((int)(i * (float)length / c.MaxHP), 0), new ZPoint(1, 20), Color.Black);

		SpriteFont font = M.fonts.verdanaBold;

		string name = c.Name;
		if (c.UniqueName != "") name = c.UniqueName + ", " + c.Name;
		screen.DrawString(font, name, 23, Color.White);

		screen.Draw(damageIcon, new ZPoint(0, 40));
		screen.DrawString(font, c.Damage.ToString(), new ZPoint(22, 43), Color.White);
		screen.DrawString(font, c.Attack.ToString() + "/" + c.Defence, 43, Color.White);
		screen.DrawString(font, c.Armor.ToString(), new ZPoint(length - 32, 43), Color.White);
		screen.Draw(armorIcon, new ZPoint(length - 20, 40));

		c.DrawEffects(screen.position + new ZPoint(0, 60), screen.position + new ZPoint(24, 180));
		if (c.data is Character) (c.data as Character).inventory.Draw(screen.position + new ZPoint(0, 92));
		DrawAbilities(c, screen, new ZPoint(0, 124));
	}

	private void DrawEndButton()
	{
		int length = 128, height = 32;
		Screen screen = new Screen(new ZPoint(M.size.x - 8 - 144 - length / 2, M.size.y - 8 - height), new ZPoint(length, height));

		MouseTriggerKeyword.Clear("End Battle");
		Resolution resolution = GetResolution();
        if (resolution != Resolution.Not) MouseTriggerKeyword.Set("End Battle", 0, screen.position, screen.size);
		MouseTriggerKeyword mt = MouseTriggerKeyword.GetUnderMouse("End Battle");

		screen.Fill(Stuff.MyColor("Dark Grey"));

		string text = resolution == Resolution.Victory ? "Victory!" : resolution == Resolution.Retreat ? "Run!" : "End Battle";
		screen.DrawString(M.fonts.verdanaBold, text, 9, resolution == Resolution.Not ? Color.Gray : mt != null ? Color.Red : Color.White);
	}

	public void Draw()
	{
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

		//DrawScale(new ZPoint(128, M.size.y - 68));
		DrawScale(new ZPoint(16 + 32, 8 + 16 + 32));
		DrawInfo(spotlightObject as LCreature, new ZPoint(M.size.x - 288 - 8, 8));
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
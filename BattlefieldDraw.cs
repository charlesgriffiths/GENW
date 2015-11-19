using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

partial class Battlefield
{
	public Vector2 GraphicCoordinates(RPoint p) { return screenPosition + new Vector2(32 * p.x, 32 * p.y); }
	public ZPoint ZCoordinates(Vector2 mouse) { return new ZPoint((mouse - screenPosition) / 32.0f); }

	public void LoadTextures()
	{
		arrowTexture = M.game.Content.Load<Texture2D>("other/arrow");
		targetTexture = M.game.Content.Load<Texture2D>("other/target");
		damageIcon = M.game.Content.Load<Texture2D>("other/damage");
		armorIcon = M.game.Content.Load<Texture2D>("other/armor");
	}

	private void Draw(Texture2D texture, RPoint rPosition)
	{
		M.Draw(texture, GraphicCoordinates(rPosition) - new Vector2(texture.Width/2 - 16, texture.Height - 32));
	}

	private void Draw(Texture2D texture, RPoint rPosition, float scaling)
	{
		if (scaling == 1.0f) Draw(texture, rPosition);
		else
		{
			int width = (int)(scaling * texture.Width);
			int height = (int)(scaling * texture.Height);
			ZPoint center = new ZPoint(GraphicCoordinates(rPosition)) + new ZPoint(16, 32 - height / 2);
			M.spriteBatch.Draw(texture, new Rectangle(center.x - width/2, center.y - height/2, width, height), Color.White);
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

	private void DrawScale(ZPoint position, ZPoint zMouse)
	{
		int length = 736, height = 20;
		Screen screen = new Screen(position, new ZPoint(length, height));

		screen.Fill(Stuff.DarkDarkGray);

		var query = from c in AliveCreatures orderby c.rInitiative.x select c;
		float zeroInitiative = -query.Last().rInitiative.x;

		MouseTriggerLCreature trigger = null;
		foreach (LCreature c in query)
		{
			int rInitiative = (int)(150.0f * (-c.rInitiative.x - zeroInitiative)) + 1;
			int y = -32, z = -32;
			if (c.isInParty) { y = height; z = 0; }

			ZPoint iconPosition = new ZPoint(rInitiative + 1, y);
			MouseTriggerLCreature.Set(c, screen.position + iconPosition, new ZPoint(32, 32));
			trigger = MouseTriggerLCreature.GetUnderMouse();

			Color color = Color.White;
			if (c.position.TheSameAs(zMouse) || (trigger != null && c == trigger.creature)) color = c.RelationshipColor;

			if (scaleAnimations.CurrentTarget != c.rInitiative)
				screen.DrawRectangle(new ZPoint(rInitiative, z), new ZPoint(1, height + 32), color);

			screen.Draw(c.texture, iconPosition);
		}

		if (expectedInitiative < 0)
			screen.DrawRectangle(new ZPoint((int)(150.0f * (-expectedInitiative - zeroInitiative)) + 1, 0), new ZPoint(1, height + 32), Color.DodgerBlue);
		expectedInitiative = 0.0f;

		if (trigger != null) Draw(M.zSelectionTexture, trigger.creature.position);
		MouseTriggerLCreature.Clear();
	}

	private void DrawAbilities(LCreature c, Screen screen, ZPoint position)
	{
		for (int i = 0; i < 6; i++) MouseTriggerKeyword.Set("ability", i, screen.position + position + new ZPoint(48 * i, 0), new ZPoint(48, 48));

		foreach (Ability a in c.Abilities)
		{
			int i = c.Abilities.IndexOf(a);
			MouseTriggerKeyword t = MouseTriggerKeyword.Get("ability", i);
			M.Draw(a.texture, t.position);

			if (a.targetType == Ability.TargetType.Passive) M.DrawRectangle(t.position, t.size, new Color(0, 0, 0, 0.7f));
			else if (c == CurrentLCreature) M.DrawStringWithShading(M.smallFont, Stuff.AbilityHotkeys[i].ToString(), t.position + new ZPoint(37, 33), Color.White);
		}

		MouseTriggerKeyword forDescription = MouseTriggerKeyword.GetUnderMouse("ability");
		if (forDescription != null && forDescription.parameter < c.Abilities.Count)
		{
			Ability a = c.Abilities[forDescription.parameter];
			M.Draw(a.texture, forDescription.position, Color.Red);

			if (c == CurrentLCreature && a.targetType != Ability.TargetType.Passive)
				M.DrawStringWithShading(M.smallFont, Stuff.AbilityHotkeys[forDescription.parameter].ToString(),
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
		float enduranceFraction = (float)c.Endurance / c.MaxHP;

		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(hpFraction * length), 20), new Color(0.2f, 0, 0));
		screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(enduranceFraction * length), 20), new Color(0.4f, 0, 0));
		for (int i = 1; i <= c.MaxHP; i++) screen.DrawRectangle(new ZPoint((int)(i * (float)length / c.MaxHP), 0), new ZPoint(1, 20), Color.Black);

		SpriteFont font = MainScreen.Instance.verdanaBoldFont;

		string name = c.Name;
		if (c.UniqueName != "") name = c.UniqueName + ", " + c.Name;
		screen.DrawString(font, name, 23, Color.White);

		screen.Draw(damageIcon, new ZPoint(0, 40));
		screen.DrawString(font, c.Damage.ToString(), new ZPoint(22, 43), Color.White);
		screen.DrawString(font, c.Attack.ToString() + "/" + c.Defence, 43, Color.White);
		screen.DrawString(font, c.Armor.ToString(), new ZPoint(length - 32, 43), Color.White);
		screen.Draw(armorIcon, new ZPoint(length - 20, 40));

		c.DrawEffects(screen.position + new ZPoint(0, 60));
		if (c.data is Character) (c.data as Character).inventory.Draw(screen.position + new ZPoint(0, 92));
		//screen.DrawRectangle(new ZPoint(32 * 6, 92), new ZPoint(1, 32), Stuff.DarkDarkGray);
		DrawAbilities(c, screen, new ZPoint(0, 124));
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
		foreach (LObject l in query) Draw(l.texture, l.rPosition, l.scaling);

		combatAnimations.Draw();
		scaleAnimations.Draw();

		if (ability != null && Mouse.IsIn(AbilityZone)) Draw(targetTexture, Mouse);
		else if (InRange(Mouse)) Draw(M.zSelectionTexture, Mouse);
		if (spotlightObject != null && spotlightObject != currentObject) M.Draw(M.zSelectionTexture, GraphicCoordinates(spotlightObject.rPosition));

		foreach (DelayedDrawing dd in delayedDrawings) dd.Draw();
		delayedDrawings.Clear();

		DrawScale(new ZPoint(128, M.size.y - 68), Mouse);
		DrawInfo(spotlightObject as LCreature, new ZPoint(M.size.x - 288 - 8, 8));
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
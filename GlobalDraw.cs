using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public partial class Player : GObject
{
	public void LoadTextures()
	{
		Texture = M.game.Content.Load<Texture2D>("other/player");
	}

	public void DrawParty(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(1, 1));
		int vOffset = 0, iOffset = 0, hiOffset = 40, vStep = 40;
		MouseTrigger.Clear<MouseTriggerObject<LocalObject>>();

		foreach (var c in party)
		{
			Screen icon = new Screen(position + new ZPoint(0, vOffset), new ZPoint(32, 32));
			icon.Fill(Stuff.MyColor("Very Dark Grey"));
			icon.Draw(c.GetTexture, ZPoint.Zero);
			MouseTriggerObject<LocalObject>.Set(c, icon.position, icon.size);

			float hpMissing = 1 - (float)c.hp.value / c.hp.Max;
			float staminaMissing = 1 - (float)c.hp.stamina / c.hp.Max;

			icon.DrawRectangle(new ZPoint(0, 32), new ZPoint(32, -(int)(staminaMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));
			icon.DrawRectangle(new ZPoint(0, 32), new ZPoint(32, -(int)(hpMissing * 32)), new Color(0.2f, 0.0f, 0.0f, 0.2f));

			if (c.inventory != null) c.inventory.Draw(position + new ZPoint(hiOffset, iOffset));

			vOffset += vStep;
			if (c.inventory != null) iOffset += vStep;
		}

		var mtc = MouseTrigger.GetUnderMouse<MouseTriggerObject<LocalObject>>();
		if (mtc != null)
		{
			LocalObject c = mtc.t;
			Screen icon = new Screen(position + new ZPoint(0, vStep * party.IndexOf(c)), new ZPoint(32, 32));
			icon.DrawString(M.fonts.verySmall, c.hp.stamina.ToString() + "/" + c.hp + "/" + c.hp.Max, 27, Color.White);
			c.DrawInfo(position + new ZPoint(6 * 32 + 48, 40));
		}

		Action<Inventory> draw = i =>
		{
			i.Draw(position + new ZPoint(hiOffset, iOffset));
			iOffset += i.Height * 32 + 8;
		};

		draw(inventory);
		if (!crafting.IsEmpty) DrawCrafting(screen.position + new ZPoint(240, iOffset));
		draw(crafting);
		if (!ground.IsEmpty) draw(ground);
		if (barter != null) DrawBarter(new ZPoint(hiOffset + 6 * 32 + 16, 8));
	}

	private void DrawBarter(ZPoint position)
	{
		toSell.Draw(position);
		toBuy.Draw(position + new ZPoint(32 * 12 + 16, 0));
		barter.inventory.Draw(position + new ZPoint(32 * 24 + 24, 0));

		M.DrawRectangle(position + new ZPoint(32 * 31, 0), new ZPoint(32, 32), Stuff.MyColor("Very Dark Grey"));
		M.Draw(barter.dialog.texture, position + new ZPoint(32 * 31, 0));

		MouseTriggerKeyword.Set("trade", position + new ZPoint(0, 40), new ZPoint(32 * 24 + 16, 16));
		var mtk = MouseTriggerKeyword.GetUnderMouse("trade");

		float loss = toBuy.Value * 2 - toSell.Value * (1 + 0.25f * Max(Skill.Get("Speech")));
		Color color = loss < 0 ? mtk != null ? Stuff.MyColor("Light Blue") : Color.Green : Color.Red;
		M.DrawRectangle(position + new ZPoint(32 * 12 + 7, 0), new ZPoint(2, 56), color);
		M.DrawRectangle(position + new ZPoint(32 * 12 + 8, 40), new ZPoint((int)(loss * 10), 16), color);

		if (mtk != null && G.LeftMouseButtonClicked)
		{
			List<Item> list = new List<Item>();
			foreach (Item item in toSell.Items) list.Add(item);
			toSell.Clear();

			foreach (Item item in toBuy.Items) toSell.Add(item);
			toBuy.Clear();

			foreach (Item item in list) toBuy.Add(item);
		}
	}

	private void DrawCrafting(ZPoint position)
	{
		List<ItemShape> shapes = craftableShapes.Keys.ToList();

		int length = 160, height = 16;
		Func<int, ZPoint> iPosition = n => position + new ZPoint(0, height * n);

		int i = 0;
		MouseTrigger.Clear<MouseTriggerObject<ItemShape>>();
		foreach (ItemShape s in shapes)
		{
			MouseTriggerObject<ItemShape>.Set(s, iPosition(i), new ZPoint(length, height));
			i++;
		}

		var mto = MouseTrigger.GetUnderMouse<MouseTriggerObject<ItemShape>>();

		i = 0;
		foreach (ItemShape s in shapes)
		{
			bool underMouse = mto != null && mto.t == s;
			bool isComposable = s.IsComposable();
			bool isReducible = craftableShapes[s] > 0;

			Color color;
			if (isComposable) color = underMouse ? Color.Red : Color.White;
			else if (isReducible && underMouse) color = Color.Gray;
			else color = Stuff.MyColor("Dark Grey");

			M.DrawString(M.fonts.verdanaBold, s.name, iPosition(i), color);
			if (craftableShapes[s] != 0) M.DrawString(M.fonts.verdanaBold, craftableShapes[s].ToString(), iPosition(i) + new ZPoint(length - 10, 0), color);

			if (underMouse)
			{
				if (G.LeftMouseButtonClicked && isComposable) craftableShapes[s]++;
				else if (G.RightMouseButtonClicked && isReducible) craftableShapes[s]--;
			}

			i++;
		}

		string keyword = "CRAFT!";
		MouseTriggerKeyword.Clear(keyword);

		int numberOfItems = (from pair in craftableShapes select pair.Value).Sum();

		if (numberOfItems > 0 && numberOfItems <= 6)
		{
			ZPoint buttonPosition = iPosition(i) + new ZPoint(0, 8);
            MouseTriggerKeyword.Set(keyword, buttonPosition, new ZPoint(length, height));
			var mtk = MouseTriggerKeyword.GetUnderMouse(keyword);
			M.DrawString(M.fonts.verdanaBold, keyword, buttonPosition, mtk != null ? Stuff.MyColor("Light Blue") : Color.White);

			if (mtk != null && G.LeftMouseButtonClicked)
			{
				crafting.Clear();
				foreach (var pair in craftableShapes) crafting.Add(pair.Key, pair.Value);
				UpdateCrafting();
			}
		}
	}

	public override void Draw()
	{
		movementAnimations.Draw();
		M.spriteBatch.Draw(Texture, M.GraphicCoordinates(rPosition));
	}
}

//public abstract partial class Creature
//{
	//private static MainScreen M { get { return MainScreen.Instance; } }

	/*public void DrawInfo(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(192, 1));
		screen.DrawString(M.fonts.verdanaBold, FullName, ZPoint.Zero, Color.White);
		
		if (this is Character)
		{
			Character ch = this as Character;
			screen.DrawString(M.fonts.small, "(" + ch.background.name + ", " + ch.origin.name + ")", new ZPoint(0, screen.offset), Color.White);
			screen.offset += 8;

			foreach (Skill skill in BigBase.Instance.skills.data)
			{
				int previousOffset = screen.offset;
				screen.DrawString(M.fonts.small, skill.name, new ZPoint(0, screen.offset), Color.White);
				int trueOffset = screen.offset;
				screen.DrawString(M.fonts.small, ch[skill].ToString(), new ZPoint(100, previousOffset), Color.White);
				screen.offset = trueOffset;
			}
		}
	}*/
//}
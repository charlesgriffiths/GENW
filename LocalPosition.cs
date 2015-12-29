using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class LocalPosition : LocalComponent
{
	public ZPoint value;
	public RPoint r;
	public AnimationQueue animations;

	public LocalPosition(LocalObject o) : base(o)
	{
		value = new ZPoint();
		r = new RPoint();
		animations = new AnimationQueue();
	}

	public ZPoint GC { get { return new ZPoint(Battlefield.GC(r)); } }

	public bool IsWalkable
	{
		get
		{
			if (t.item != null) return true;
			else return t.hp.value < 0;
		}
	}

	public bool IsFlat
	{
		get
		{
			if (t.item != null) return true;
			else return t.hp.value < 0;
		}
	}

	public bool IsVisible {	get	{ return t.HasEffect("Melded", "Hidden") ? false : true; } }

	public void Set(ZPoint p, float gameTime, bool commonQueue)
	{
		RMove rMove = new RMove(r, p - value, gameTime);
		if (commonQueue) B.combatAnimations.Add(rMove);
		else animations.Add(rMove);
		value = p;
	}

	public bool TheSameAs(ZPoint p) { return value.TheSameAs(p); }
	public int x { get { return value.x; } }
	public int y { get { return value.y; } }

	public bool IsAdjacentTo(LocalObject u) { return value.IsAdjacentTo(u.p.value); }
	public bool IsAdjacentTo(List<ZPoint> zone) { return zone.Where(p => p.IsAdjacentTo(value)).Count() > 0; }
	public bool IsReachableFrom(List<ZPoint> zone) {
		return (zone.Where(p => B.IsReachable(p, value, B.current.p.Range))).Count() > 0; }

	public int Distance(LocalObject u) { return MyMath.ManhattanDistance(value, u.p.value); }
	public int Distance(ZPoint p) { return MyMath.ManhattanDistance(value, p); }

	public bool CanSee(LocalObject u)
	{
		if (!IsVisible) return false;

		if (t.HasEffect("Blind") && Distance(u) > 1) return false;
		if (t.HasEffect("Blindsight") && t.effects.Get("Blindsight").parameter == u) return false;

		return true;
	}

	public int Range { get { return t.inventory != null ? (from i in t.inventory.Items select i.data.range).Max() : 1; } }

	public void DoDamage(LocalObject u, int damage, bool pure)
	{
		bool pastLife = u.hp.value > 0;
		int finalDamage = pure || t.HasEffect("Prodigious Precision") ? damage : Math.Max(damage - u.hp.Armor, 0);

		u.hp.Add(-finalDamage, false);

		u.hp.RememberDamage(t, finalDamage);
		//if (u.hp.value < 0 && pastLife) u.hp.Kill();

		B.combatAnimations.Add(new TextAnimation(finalDamage.ToString(), Texture.Get(pure ? "pureDamage" : "damage"),
			M.fonts.verdanaBold, Color.White, u.p.GC, 1, true));
	}

	public Inventory Ground
	{
		get
		{
			Inventory result = new Inventory(3, 1, null, "ground", true);
			int pickupDistance = 2;
			List<LocalObject>[] list = new List<LocalObject>[pickupDistance];
			for (int k = 0; k < pickupDistance; k++)
			{
				list[k] = B.Items.Where(i => Distance(i) == k).ToList();
				for (int i = 0; i < list[k].Count && i < 3; i++) result.Add(list[k][i].item.data, i);
			}
			return result;
		}
	}

	public void DrawInfo(ZPoint position)
	{
		int length = 288, height = 124 + 48;
		Screen screen = new Screen(position, new ZPoint(length, height));
		screen.Fill(Color.Black);

		if (t.hp != null)
		{
			float hpFraction = (float)t.hp.value / t.hp.Max;
			float staminaFraction = (float)t.hp.stamina / t.hp.Max;

			screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(hpFraction * length), 20), new Color(0.2f, 0, 0));
			screen.DrawRectangle(new ZPoint(0, 0), new ZPoint((int)(staminaFraction * length), 20), new Color(0.4f, 0, 0));
			for (int i = 1; i <= t.hp.Max; i++) screen.DrawRectangle(new ZPoint((int)(i * (float)length / t.hp.Max), 0),
				new ZPoint(1, 20), Color.Black);
		}

		SpriteFont font = M.fonts.verdanaBold;
		screen.DrawString(font, t.FullName, 23, Color.White);

		if (t.attack != null)
		{
			screen.Draw(B.damageIcon, new ZPoint(0, 40));
			screen.DrawString(font, t.attack.Damage.ToString(), new ZPoint(22, 43), Color.White);
			screen.DrawString(font, t.attack.Value.ToString() + "/" + t.defence.Value, 43, Color.White);
		}

		if (t.hp != null)
		{
			screen.DrawString(font, t.hp.Armor.ToString(), new ZPoint(length - 32, 43), Color.White);
			screen.Draw(B.armorIcon, new ZPoint(length - 20, 40));
		}

		if (t.effects != null) t.effects.Draw(screen.position + new ZPoint(0, 60), screen.position + new ZPoint(24, 180));
		if (t.inventory != null) t.inventory.Draw(screen.position + new ZPoint(0, 92));
		if (t.abilities != null) t.abilities.Draw(screen, new ZPoint(0, 124));
		Ground.Draw(screen.position + new ZPoint(192, 92));
	}
}

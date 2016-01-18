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
		r = new RPoint();
		animations = new AnimationQueue();
	}

	public ZPoint GC { get { return new ZPoint(Battlefield.GC(value)); } }

	public bool IsWalkable
	{
		get
		{
			if (t.shape != null) return t.shape.data.isWalkable;
			else if (t.hp != null) return t.hp.value <= 0;
			else return true;
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
		if (value == null) value = new ZPoint();

		RMove rMove = new RMove(r, p - value, gameTime);
		if (commonQueue) B.combatAnimations.Add(rMove);
		else animations.Add(rMove);
		value = p;
	}

	public bool TheSameAs(ZPoint p) { return value.TheSameAs(p); }
	public int x { get { return value.x; } }
	public int y { get { return value.y; } }

	public bool IsAdjacentTo(LocalObject o) { return value.IsAdjacentTo(o.p.value); }
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

	public int Range { get { return t.inventory != null && !t.inventory.IsEmpty ? (from i in t.inventory.Items select i.data.range).Max() : 1; } }

	public void DoDamage(LocalObject o, int damage, bool pure)
	{
		bool pastLife = o.hp.value > 0;
		int finalDamage = pure || t.HasAbility("Prodigious Precision") ? damage : Math.Max(damage - o.hp.Armor, 0);

		o.hp.Add(-finalDamage, false);

		o.hp.RememberDamage(t, finalDamage);
		//if (u.hp.value < 0 && pastLife) u.hp.Kill();

		B.combatAnimations.Add(new TextAnimation(finalDamage.ToString(), NamedTexture.Get(pure ? "local/pureDamage" : "local/damage"),
			M.fonts.verdanaBold, Color.White, o.p.GC, 1, true));
	}

	public Inventory Ground
	{
		get
		{
			Inventory result = new Inventory(3, 1, "ground", true, null, null);
			int pickupDistance = 1;
			List<LocalObject>[] list = new List<LocalObject>[pickupDistance + 1];

			for (int k = 0; k <= pickupDistance; k++)
			{
				list[k] = B.Items.Where(o => Distance(o) == k).ToList();
				for (int i = 0; i < list[k].Count; i++)
				{
					Item item = list[k][i].item;
					if (result.CanAdd(item)) result.Add(item, true);
				}
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
			screen.Draw(NamedTexture.Get("other/damage"), new ZPoint(0, 40));
			screen.DrawString(font, t.attack.Damage.ToString(), new ZPoint(22, 43), Color.White);
			screen.DrawString(font, t.attack.Value.ToString() + "/" + t.defence.Value, 43, Color.White);
		}

		if (t.hp != null)
		{
			screen.DrawString(font, t.hp.Armor.ToString(), new ZPoint(length - 32, 43), Color.White);
			screen.Draw(NamedTexture.Get("other/armor"), new ZPoint(length - 20, 40));
		}

		if (t.effects != null) t.effects.Draw(screen.position + new ZPoint(0, 60), screen.position + new ZPoint(24, 180));
		if (t.inventory != null) t.inventory.Draw(screen.position + new ZPoint(0, 92));
		if (t.abilities != null) t.abilities.Draw(screen, new ZPoint(0, 124));
		Ground.Draw(screen.position + new ZPoint(192, 92));
	}

	private LocalShape Corpse
	{
		get
		{
			if (t.shape != null) return t.shape.data.corpse;
			else if (t.race != null) return LocalShape.Get("Blood");
			else return null;
		}
	}

	public void Kill()
	{
		if (Corpse != null) B.Add(new LocalObject(Corpse), value);
		if (t.shape != null && t.shape.data.onDeath != null) B.Add(new LocalObject(new Item(t.shape.data.onDeath)), value);

		B.Remove(t);
	}
}

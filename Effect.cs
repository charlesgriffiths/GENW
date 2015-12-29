using System.Xml;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class EffectShape : NamedObject
{
	public Texture2D texture;
	public string description;
	private Sgn sgn;

	private enum Sgn { Positive, Neutral, Negative };
	public Color SgnColor { get { return sgn == Sgn.Positive ? Color.Green : sgn == Sgn.Neutral ? Color.DodgerBlue : Color.Red; } }

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		description = MyXml.GetString(xnode, "description");
		char c = MyXml.GetChar(xnode, "sgn");

		if (c == '+') sgn = Sgn.Positive;
		else if (c == '0') sgn = Sgn.Neutral;
		else if (c == '-') sgn = Sgn.Negative;
		else Log.Error("wrong sign of effect " + name);
	}

	public static void LoadTextures()
	{
		foreach (EffectShape e in BigBase.Instance.effects.data)
			e.texture = M.game.Content.Load<Texture2D>("effects/" + e.name);
	}

	public void DrawDescription(ZPoint position)
	{
		Screen screen = new Screen(position, new ZPoint(240, 190));
		screen.DrawString(M.fonts.verdanaBold, name, new ZPoint(3, 3), Color.White);
		screen.DrawString(M.fonts.small, description, new ZPoint(3, screen.offset + 8), Color.White, screen.size.x - 3);
	}
}

public class Effect
{
	public EffectShape data;
	public float timeLeft;
	public object parameter;

	public Effect(string name, float time)
	{
		data = BigBase.Instance.effects.Get(name);
		timeLeft = time;
	}

	public Effect(string name, float time, object parameteri)
	{
		data = BigBase.Instance.effects.Get(name);
		timeLeft = time;
		parameter = parameteri;
	}

	public bool NameIs(string name) { return data == BigBase.Instance.effects.Get(name); }
	public bool NameIs(params string[] names)
	{
		foreach (string name in names) if (NameIs(name)) return true;
		return false;
	}
}

public class Effects : LocalComponent
{
	public List<Effect> data;

	public Effects(LocalObject o) : base(o)
	{
		data = new List<Effect>();
	}

	public bool Has(string name) { return data.Where(e => e.data.name == name).Count() > 0; }
	public Effect Get(string name) { return data.Where(e => e.data.name == name).Single(); }

	public void Add(string name, float time) { Add(name, time, null); }
	public void Add(string name, float time, object parameter)
	{
		var query = data.Where(e => e.data.name == name);
		if (query.Count() > 0)
		{
			Effect e = query.Single();
			if (e.timeLeft < time) e.timeLeft = time;
		}
		else
		{
			Effect e = new Effect(name, time, parameter);
			data.Add(e);

			if (e.NameIs("True Strike", "Blind", "Fake Death", "Sleeping", "Blindsight", "Mind Tricked", "Mind Controlled"))
				B.log.AddLine(t.CommonName + " ", t.LogColor);

			if (e.NameIs("Roots"))
			{
				B.log.AddLine("Multiple roots crawl from the ground and entangle " + t.CommonName + "! ", Color.Pink);
				B.log.Add(t.CommonName, t.LogColor);
				B.log.Add(" can't move!", Color.Pink);
			}
			else if (e.NameIs("True Strike")) B.log.Add("feels confident.", Color.Pink);
			else if (e.NameIs("Blind")) B.log.Add("can't see anything!", Color.Pink);
			else if (e.NameIs("Fake Death")) B.log.Add("looks completely dead.", Color.Pink);
			else if (e.NameIs("Sleeping")) B.log.Add("falls asleep.", Color.Pink);
			else if (e.NameIs("Blindsight")) B.log.Add("can't see " + (e.parameter as LocalObject).CommonName + " now.", Color.Pink);
			else if (e.NameIs("Mind Tricked")) B.log.Add("is now fighting for the wrong party!", Color.Pink);
			else if (e.NameIs("Mind Controlled"))
				B.log.Add("is now controlled by " + (e.parameter as LocalObject).CommonName + "!", Color.Pink);
		}
	}

	public void Remove(string name)
	{
		var query = data.Where(e => e.data.name == name);
		if (query.Count() > 0)
		{
			Effect e = query.Single();

			if (e.NameIs("Destined to Die"))
			{
				Add("Death Prediction Failed", 10);

				B.log.AddLine((e.parameter as LocalObject).CommonName + "'s death prediction failed. ", Color.Pink);
				B.log.Add(t.CommonName, t.LogColor);
				B.log.Add(" feels relieved.", Color.Pink);
			}
			else if (e.NameIs("Destined to Succeed"))
			{
				Add("Success Prediction Failed", 10);

				B.log.AddLine((e.parameter as LocalObject).CommonName + "'s success prediction failed. ", Color.Pink);
				B.log.Add(t.CommonName, t.LogColor);
				B.log.Add(" feels depressed.", Color.Pink);
			}
			//else if (e.NameIs("Net")) B.Add(new LocalObject(ItemShape.Get("Net")), t.position.value);
			else
			{
				B.log.AddLine(t.CommonName, t.LogColor);
				B.log.Add(" is no longer ", Color.Pink);
				B.log.Add(e.data.name + ".", e.data.SgnColor);
			}

			data.Remove(e);
		}
	}

	public void RemoveAll(params string[] names) { foreach (string name in names) Remove(name); }
	public bool HasOne(params string[] names)
	{
		foreach (string name in names) if (Has(name)) return true;
		return false;
	}

	public int BothAD
	{
		get
		{
			int result = 0;

			if (t.HasAbility("Lone Warrior") && B.ActiveObjects.Where(u => t.p.Distance(u) <=
				CAbility.Get("Lone Warrior").range).Count() == 0) result += 2;

			if (Has("Attention") && t.team.IsFriendTo(Get("Attention").parameter as LocalObject)) result += 1;
			if (Has("Net")) result -= 2;
			if (Has("Blind")) result -= 3;

			if (Has("Destined to Die")) result -= Has("Attention") ? 6 : 3;
			if (Has("Success Prediction Failed")) result -= Has("Attention") ? 4 : 2;
			if (Has("Destined to Succeed")) result += Has("Attention") ? 6 : 3;
			if (Has("Death Prediction Failed")) result += Has("Attention") ? 4 : 2;

			return result;
		}
	}

	public void Draw(ZPoint p, ZPoint descriptionP)
	{
		Screen screen = new Screen(p, new ZPoint(1, 32));
		MouseTriggerKeyword.Clear("effect");

		int i = 0;
		foreach (Effect e in data)
		{
			screen.Draw(e.data.texture, new ZPoint(32 * i, 0), e.data.SgnColor);
			screen.DrawStringWithShading(M.fonts.small, ((int)e.timeLeft).ToString(), new ZPoint(32 * i + 26, 20), Color.White);

			MouseTriggerKeyword.Set("effect", i.ToString(), p + new ZPoint(32 * i, 0), new ZPoint(32, 32));
			i++;
		}

		var mtk = MouseTriggerKeyword.GetUnderMouse("effect");
		if (mtk != null) data[int.Parse(mtk.parameter)].data.DrawDescription(descriptionP);
	}
}

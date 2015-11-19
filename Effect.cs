﻿using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class EffectShape : NamedObject
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
			e.texture = MainScreen.Instance.game.Content.Load<Texture2D>("effects/" + e.name);
	}
}

class Effect
{
	public EffectShape data;
	public float timeLeft;

	public Effect(string name, float time)
	{
		data = BigBase.Instance.effects.Get(name);
		timeLeft = time;
	}
}

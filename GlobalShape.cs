﻿using System;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

public class GlobalShape : NamedObject
{
	private string textureName;
	public Texture2D texture;
	public float speed;
	public bool isActive;
	public Dialog dialog;

	public Dictionary<string, int> partyShape = new Dictionary<string, int>();

	public override void Load(XmlNode xnode)
	{
		name = MyXml.GetString(xnode, "name");
		textureName = MyXml.GetString(xnode, "icon");
		if (textureName == "") textureName = name;
		isActive = MyXml.GetBool(xnode, "active");

		if (isActive) speed = MyXml.GetFloat(xnode, "speed");
		else speed = 1.0f;

		string dialogName = MyXml.GetString(xnode, "dialog");
		if (dialogName != "") dialog = BigBase.Instance.dialogs.Get(dialogName);

		for (xnode = xnode.FirstChild; xnode != null; xnode = xnode.NextSibling)
			partyShape.Add(MyXml.GetString(xnode, "name"), Math.Max(MyXml.GetInt(xnode, "quantity"), 1));
	}

	public static void LoadTextures()
	{
		foreach (GlobalShape s in BigBase.Instance.gShapes.data)
			s.texture = MainScreen.Instance.game.Content.Load<Texture2D>("global/" + s.textureName);
	}
}
﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

class DialogScreen
{
	private DialogNode dialogNode;
	private string dialogName;
	private GObject gObject;

	public Screen screen;
	public SpriteFont dialogFont;

	private Player P { get { return World.Instance.player; } }

	public DialogScreen(ZPoint position, ZPoint size)
	{
		screen = new Screen(position, size);
	}

	public void Draw()
	{
		if (MyGame.Instance.gameState != MyGame.GameState.Dialog) return;

		screen.Fill(new Color(0.2f, 0.2f, 0.2f));
		screen.DrawString(dialogFont, dialogNode.text, new ZPoint(10, 10), Color.White, 45);

		for (int i = 0; i < dialogNode.responses.Count; i++)
			screen.DrawString(dialogFont, (i+1) + ". " + dialogNode.responses[i].text, 
				new ZPoint(10, 170 + i*20), Color.White, 47);
	}

	private static int KeyCode(KeyboardState k)
	{
		if (k.IsKeyDown(Keys.D1)) return 1;
		else if (k.IsKeyDown(Keys.D2)) return 2;
		else if (k.IsKeyDown(Keys.D3)) return 3;
		else if (k.IsKeyDown(Keys.D4)) return 4;
		else if (k.IsKeyDown(Keys.D5)) return 5;
		else if (k.IsKeyDown(Keys.D6)) return 6;
		else if (k.IsKeyDown(Keys.D7)) return 7;
		else if (k.IsKeyDown(Keys.D8)) return 8;
		else if (k.IsKeyDown(Keys.D9)) return 9;
		else return 0;
	}

	public void Press(DialogResponse r)
	{
		string nextNode = r.jump;

		if (r.name == "fight") { World.Instance.battlefield.StartBattle(gObject); return; }

		if (dialogName == "Wild Dogs Encounter")
		{
			if (r.name == "condition1")
			{
				if (P.party.Count <= 1) nextNode = "1positive";
				else nextNode = "1negative";
			}
			else if (r.name == "condition2")
			{
				if (P.party.Count <= 2) nextNode = "2positive";
				else nextNode = "2negative";
			}
		}

		if (nextNode != "") dialogNode = BigBase.Instance.dialogs.Get(dialogName).nodes[nextNode];
		else MyGame.Instance.gameState = MyGame.GameState.Global;
	}

	public void Press(KeyboardState keyboardState)
	{
		int k = KeyCode(keyboardState);
		if (k > 0 && k <= dialogNode.responses.Count) Press(dialogNode.responses[k-1]);
	}

	public void StartDialog(string name, GObject g)
	{
		dialogName = name;
		dialogNode = BigBase.Instance.dialogs.Get(name).nodes["entry"];
		gObject = g;
		MyGame.Instance.gameState = MyGame.GameState.Dialog;
	}
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

class DialogScreen
{
	private DialogNode dialogNode;
	private Dialog dialog;
	private GObject gObject;

	public Screen screen;

	private Player P { get { return World.Instance.player; } }

	public DialogScreen(ZPoint position, ZPoint size)
	{
		screen = new Screen(position, size);
	}

	public void Draw()
	{
		if (MyGame.Instance.dialog == false) return;
		
		screen.Fill(new Color(0.2f, 0.2f, 0.2f, 0.9f));

		SpriteFont font = MainScreen.Instance.verdanaFont;
        screen.DrawString(font, dialogNode.text, new ZPoint(10, 10), Color.White, 45);

		for (int i = 0; i < dialogNode.responses.Count; i++)
			screen.DrawString(font, (i+1) + ". " + dialogNode.responses[i].text, 
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

		if (r.name == "fight") World.Instance.battlefield.StartBattle(gObject);

		if (dialog.name == "The First Dialog")
		{
			if (r.name == "Boo-Boo") P.party.Add(new PartyCreep("Krokar", "Boo-Boo"));
			else if (r.name == "escherian shard") { }
		}
		else if (dialog.name == "Wild Dogs Encounter")
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

		if (nextNode != "") dialogNode = dialog.nodes[nextNode];
		else MyGame.Instance.dialog = false;
	}

	public void Press(KeyboardState keyboardState)
	{
		int k = KeyCode(keyboardState);
		if (k > 0 && k <= dialogNode.responses.Count) Press(dialogNode.responses[k-1]);
	}

	public void StartDialog(string dialogName)
	{
		StartDialog(BigBase.Instance.dialogs.Get(dialogName), null);
	}

	public void StartDialog(Dialog d, GObject g)
	{
		dialog = d;
		if (d.isUnique && d.happened) return;

		dialogNode = d.nodes["entry"];
		gObject = g;
		d.happened = true;
		MyGame.Instance.dialog = true;
	}
}
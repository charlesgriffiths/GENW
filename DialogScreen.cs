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

		screen.Draw(dialog.texture, new ZPoint(8, 8));

		screen.offset = 8;
		SpriteFont font = MainScreen.Instance.verdanaFont;
        screen.DrawString(font, dialogNode.text, new ZPoint(48, screen.offset), Color.White, screen.size.x - 48);
		screen.DrawString(font, dialogNode.description, new ZPoint(48, screen.offset), Color.Gray, screen.size.x - 48);

		screen.offset = 80;
		for (int i = 0; i < dialogNode.responses.Count; i++)
		{
			DialogResponse r = dialogNode.responses[i];
			ZPoint p = new ZPoint(8, screen.offset);

			MouseTriggerKeyword.Set("dialog", i + 1, screen.position + p, new ZPoint(font.MeasureString(r.text)) + new ZPoint(20, 0));
			MouseTriggerKeyword mt = MouseTriggerKeyword.GetUnderMouse();

			Color color = Color.White;
			if (mt != null && mt.name == "dialog" && mt.parameter == i + 1) color = Color.DodgerBlue;
            screen.DrawString(font, (i + 1) + ". " + r.text, p, color, screen.size.x - 20);
		}
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
			if (r.name == "Boo-Boo") P.party.Add(new Creep("Krokar", "Boo-Boo"));
			else if (r.name == "escherian shard") { }
		}
		else if (dialog.name == "Wild Dogs Encounter")
		{
			int threshold = gObject.Name == "Wild Dogs Large Pack" ? 6 : 1;
			if (r.name == "condition1") nextNode = P.party.Count <= threshold ? "1positive" : "1negative";
			else if (r.name == "condition2") nextNode = P.party.Count <= threshold + 1 ? "2positive" : "2negative";
		}

		if (nextNode != "") dialogNode = dialog.nodes[nextNode];
		else MyGame.Instance.dialog = false;

		MouseTriggerKeyword.Clear("dialog");
	}

	public void Press(int k) { if (k > 0 && k <= dialogNode.responses.Count) Press(dialogNode.responses[k - 1]); }
	public void Press(KeyboardState keyboardState) { Press(KeyCode(keyboardState));	}

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
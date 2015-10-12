using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

class DialogScreen
{
	public bool isActive = false;

	private DialogNode dialogNode;
	private string dialogName;
	private GObject gObject;

	public Screen screen;
	public SpriteFont dialogFont;

	public DialogScreen(ZPoint position, ZPoint size)
	{
		screen = new Screen(position, size, new Color(0.2f, 0.2f, 0.2f));
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (isActive == false) return;

		screen.Fill(spriteBatch);
		screen.DrawString(dialogFont, dialogNode.text, new ZPoint(10, 10), Color.LightGray, 50, spriteBatch);

		for (int i = 0; i < dialogNode.responses.Count; i++)
			screen.DrawString(dialogFont, (i+1) + ". " + dialogNode.responses[i].text, 
				new ZPoint(10, 170 + i*20), Color.White, 47, spriteBatch);
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
		if (dialogName == "Morlocks Encounter")
		{
			if (r.name == "fight") { gObject.Kill(); World.Instance.player.partySize--; }
			else if (r.name == "join") { gObject.Kill(); World.Instance.player.partySize++; }
		}
		else if (dialogName == "Wild Dogs Encounter")
		{
			if (r.name == "fight") { gObject.Kill(); World.Instance.player.partySize--; }
		}

		if (r.jump != "") dialogNode = BigBase.Instance.dialogBase.Get(dialogName).nodes[r.jump];
		else isActive = false;
	}

	public void Press(KeyboardState keyboardState)
	{
		int k = KeyCode(keyboardState);
		if (k > 0 && k <= dialogNode.responses.Count) Press(dialogNode.responses[k-1]);
	}

	public void StartDialog(string name, GObject g)
	{
		dialogName = name;
		dialogNode = BigBase.Instance.dialogBase.Get(name).nodes["entry"];
		gObject = g;
		isActive = true;
	}
}
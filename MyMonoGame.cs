using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class MyMonoGame : Game
{
	GraphicsDeviceManager graphics;

	private World W { get { return World.Instance; } }
	private MainScreen M { get { return MainScreen.Instance; } }
	private Battlefield B { get { return World.Instance.battlefield; } }
	private MyGame G { get { return MyGame.Instance; } }

	public MyMonoGame()
	{
		graphics = new GraphicsDeviceManager(this);
		graphics.PreferredBackBufferWidth = MainScreen.Instance.size.x;
		graphics.PreferredBackBufferHeight = MainScreen.Instance.size.y;
		Window.Position = new Point(MainScreen.Instance.windowPosition.x, MainScreen.Instance.windowPosition.y);
		graphics.ApplyChanges();

		Content.RootDirectory = "Content";
	}

	protected override void Initialize()
	{
		base.Initialize();
		IsMouseVisible = true;

		G.previousKeyboardState = Keyboard.GetState();
		G.previousMouseState = Mouse.GetState();
	}

	protected override void LoadContent()
	{
		MainScreen.Instance.Init(this);
		BigBase.Instance.Load();

		MainScreen.Instance.LoadTextures();
		GlobalTile.LoadTextures();
		LocalTile.LoadTextures();
		EffectShape.LoadTextures();
		ClassAbility.LoadTextures();
		GlobalShape.LoadTextures();
		Texture.LoadTextures();
		CharacterClass.LoadTextures();
		LocalShape.LoadTextures();
		ItemShape.LoadTextures();
		Dialog.LoadTextures();

		World.Instance.Load();
		World.Instance.LoadTextures();
	}

	protected override void UnloadContent()
	{
		Content.Unload();
	}

	private bool KeyPressed() {	return G.keyboardState != G.previousKeyboardState; }
	private bool KeyPressed(Keys key)
	{
		if (G.keyboardState.IsKeyDown(key) && !G.previousKeyboardState.IsKeyDown(key)) return true;
		else return false;
	}

	private bool LeftButtonPressed()
	{
		if (G.mouseState.LeftButton == ButtonState.Pressed && !(G.previousMouseState.LeftButton == ButtonState.Pressed)) return true;
		else return false;
	}

	protected override void Update(GameTime gameTime)
	{
		G.previousKeyboardState = G.keyboardState;
		G.previousMouseState = G.mouseState;
		G.keyboardState = Keyboard.GetState();
		G.mouseState = Mouse.GetState();

		if (G.keyboardState.IsKeyDown(Keys.F1)) Exit();
		if (KeyPressed(Keys.OemTilde)) G.input = !G.input;

		if (G.input && KeyPressed()) G.console.Press(G.keyboardState);
		else if (G.dialog)
		{
			if (KeyPressed()) M.dialogScreen.Press(G.keyboardState);
			else if (G.LeftMouseButtonClicked)
			{
				var mtk = MouseTriggerKeyword.GetUnderMouse("dialog");
				if (mtk != null) M.dialogScreen.Press(int.Parse(mtk.parameter));
			}
		}
		else if (G.battle && B.ability != null)
		{
			if (G.mouseState.LeftButton == ButtonState.Pressed && B.Mouse.IsIn(B.AbilityZone))
				B.current.abilities.Use(B.ability, B.Mouse);

			if (KeyPressed(Keys.Escape)) B.ability = null;
		}
		else if (G.battle && B.ability == null)
		{
			if (G.LeftMouseButtonClicked)
			{
				var mtk = MouseTrigger.GetUnderMouse<MouseTriggerKeyword>();
				if (mtk != null)
				{
					if (mtk.name == "End Battle") B.EndBattle();
				}
				else B.SetSpotlight();
			}
			if (G.RightMouseButtonClicked) B.GoTo();

			if (KeyPressed(Keys.Space)) B.current.initiative.Wait();

			else if (KeyPressed(Keys.Right)) B.current.movement.MoveOrAttack(ZPoint.Direction.Right, G.keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Up)) B.current.movement.MoveOrAttack(ZPoint.Direction.Up, G.keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Left)) B.current.movement.MoveOrAttack(ZPoint.Direction.Left, G.keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Down)) B.current.movement.MoveOrAttack(ZPoint.Direction.Down, G.keyboardState.IsKeyDown(Keys.LeftControl));

			if (B.current.abilities != null)
			{
				foreach (char key in Stuff.AbilityHotkeys)
				{
					int index = Stuff.AbilityHotkeys.IndexOf(key);
					Abilities A = B.current.abilities;
					Experience XP = B.current.xp;

					if (KeyPressed(Stuff.GetKey(key)) && index < A.list.Count)
					{
						ClassAbility a = A.list[index];
						if (A.cooldowns[a] == 0)
						{
							if (A.Has(a)) A.Use(a);
							else if (XP.AbilityPoints > 0) XP.learned.Add(a);
						}
					}
				}
			}

			if (B.current.inventory != null)
			{
				foreach (char key in Stuff.ItemHotkeys)
				{
					int index = Stuff.ItemHotkeys.IndexOf(key);
					LocalObject lc = B.current;
					if (lc.inventory != null)
					{
						Item item = lc.inventory[index];
						if (KeyPressed(Stuff.GetKey(key)) && item != null && item.data.ability != null && item.cooldown == 0)
						{
							item.cooldown += item.data.ability.cooldownTime;
							lc.abilities.Use(item.data.ability);
						}
					}
				}
			}

			if (G.editor)
			{
				if (G.mouseState.ScrollWheelValue > G.previousMouseState.ScrollWheelValue) M.editor.GoLeft();
				else if (G.mouseState.ScrollWheelValue < G.previousMouseState.ScrollWheelValue) M.editor.GoRight();

				if (G.mouseState.LeftButton == ButtonState.Pressed) B.SetTile(M.editor.LocalBrush);
			}
		}
		else
		{
			if (G.RightMouseButtonClicked && MouseTrigger.GetUnderMouse<MouseTrigger>() == null) W.player.GoTo();
			if (G.LeftMouseButtonClicked)
			{
				var mti = MouseTrigger.GetUnderMouse<MouseTriggerInventory>();
				if (mti != null && mti.GetItem() != null)
				{
					bool shift = G.keyboardState.IsKeyDown(Keys.LeftShift);

                    G.dndItem = new Item(mti.GetItem().data);
					G.inventory = mti.inventory;
					G.cell = mti.cell;

					if (G.dndItem != null)
					{
						if (shift)
						{
							G.dndItem.numberOfStacks = mti.GetItem().numberOfStacks;
							mti.inventory.RemoveStack(mti.cell);
						}
						else mti.inventory.Remove(mti.cell);

						IsMouseVisible = false;
					}

					if (mti.inventory == W.player.crafting) W.player.UpdateCrafting();
				}
			}

			if (G.dndItem != null && G.LeftMouseButtonReleased)
			{
				var mti = MouseTrigger.GetUnderMouse<MouseTriggerInventory>();
				var mtc = MouseTrigger.GetUnderMouse<MouseTriggerObject<LocalObject>>();

				if (mti != null && G.inventory == mti.inventory && G.inventory[G.cell] == null && mti.GetItem() != null && mti.GetItem().data != G.dndItem.data)
				{ // поменять местами 2 предмета
					G.inventory.Add(new Item(mti.GetItem().data), G.cell);
					G.inventory[G.cell].numberOfStacks = mti.GetItem().numberOfStacks;
					mti.inventory.RemoveStack(mti.cell);
					mti.inventory.Add(new Item(G.dndItem.data), mti.cell);
					mti.inventory[mti.cell].numberOfStacks = G.dndItem.numberOfStacks;
				}
				else if (mti != null && mti.inventory.CanAdd(G.dndItem, mti.cell) && G.inventory.isInParty == mti.inventory.isInParty &&
					!(mti.inventory.name == "SELL" && G.dndItem.data.IsRenewable))
					mti.inventory.Add(G.dndItem, mti.cell);
				else if (mtc != null && mtc.t.eating.CanEat(G.dndItem) && G.inventory.isInParty) mtc.t.eating.Eat(G.dndItem);
				else G.inventory.Add(G.dndItem, G.cell);

				if (mti != null && mti.inventory == W.player.crafting) W.player.UpdateCrafting();

				G.dndItem = null;
				G.inventory = null;
				IsMouseVisible = true;
			}

			if (KeyPressed(Keys.Home)) W.player.Move(HexPoint.HexDirection.N);
			else if (KeyPressed(Keys.End)) W.player.Move(HexPoint.HexDirection.S);
			else if (KeyPressed(Keys.Insert)) W.player.Move(HexPoint.HexDirection.NW);
			else if (KeyPressed(Keys.Delete)) W.player.Move(HexPoint.HexDirection.SW);
			else if (KeyPressed(Keys.PageUp)) W.player.Move(HexPoint.HexDirection.NE);
			else if (KeyPressed(Keys.PageDown)) W.player.Move(HexPoint.HexDirection.SE);

			if (G.editor)
			{
				if (G.mouseState.ScrollWheelValue > G.previousMouseState.ScrollWheelValue) M.editor.GoLeft();
				else if (G.mouseState.ScrollWheelValue < G.previousMouseState.ScrollWheelValue) M.editor.GoRight();

				if (G.mouseState.LeftButton == ButtonState.Pressed) W.map[M.Mouse] = M.editor.GlobalBrush;
			}
		}

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);
		M.spriteBatch.Begin();
		MainScreen.Instance.Draw();
		M.spriteBatch.End();
		base.Draw(gameTime);
	}
}

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
        GTile.LoadTextures();
		LTile.LoadTextures();
		Effect.LoadTextures();
		Ability.LoadTextures();
		GObjectShape.LoadTextures();
		Texture.LoadTextures();
		CreepShape.LoadTextures();
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
		G.keyboardState = Keyboard.GetState();
		G.mouseState = Mouse.GetState();

		if (G.keyboardState.IsKeyDown(Keys.F1)) Exit();
		if (KeyPressed(Keys.OemTilde)) G.input = !G.input;

		if (G.input && KeyPressed()) G.console.Press(G.keyboardState);
		else if (G.dialog && KeyPressed()) M.dialogScreen.Press(G.keyboardState);
		else if (G.battle && B.ability != null)
		{
			if (G.mouseState.LeftButton == ButtonState.Pressed && B.Mouse.IsIn(B.AbilityZone))
				B.CurrentLCreature.UseAbility(B.ability, B.Mouse);

			if (KeyPressed(Keys.Escape)) B.ability = null;
        }
		else if (G.battle && B.ability == null)
		{
			if (G.mouseState.LeftButton == ButtonState.Pressed) B.SetSpotlight();
			if (G.RightMouseButtonClicked) B.GoTo();

			if (KeyPressed(Keys.Space)) B.CurrentLCreature.Wait();

			else if (KeyPressed(Keys.Right)) B.CurrentLCreature.TryToMove(ZPoint.Direction.Right, G.keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Up)) B.CurrentLCreature.TryToMove(ZPoint.Direction.Up, G.keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Left)) B.CurrentLCreature.TryToMove(ZPoint.Direction.Left, G.keyboardState.IsKeyDown(Keys.LeftControl));
			else if (KeyPressed(Keys.Down)) B.CurrentLCreature.TryToMove(ZPoint.Direction.Down, G.keyboardState.IsKeyDown(Keys.LeftControl));

			foreach (char key in Stuff.AbilityHotkeys)
			{
				int index = Stuff.AbilityHotkeys.IndexOf(key);
				List<Ability> abilities = B.CurrentLCreature.data.Abilities;
				if (KeyPressed(Stuff.GetKey(key)) && index < abilities.Count) B.CurrentLCreature.UseAbility(abilities[index]);
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
			if (G.RightMouseButtonClicked) W.player.GoTo();

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

		G.previousKeyboardState = G.keyboardState;
		G.previousMouseState = G.mouseState;
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

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class RMove : Animation
{
	private RPoint target;
	private Vector2 delta;

	public RPoint Target { get { return target; } }

	public RMove(RPoint targeti, Vector2 v, float gameTime)
	{
		Log.Assert(gameTime > 0, "speed is less then zero in RMove");
		target = targeti;

		maxFrameTime = (int)(gameTime * FramesInOneGameTime);
		if (maxFrameTime == 0) maxFrameTime = 1;
		delta = v / maxFrameTime;
		frameTime = 0;
	}

	public override void Draw()
	{
		target.Add(delta);
		base.Draw();
	}

	public override bool SpendsTime { get { return true; } }
}

class ScalingAnimation : Animation
{
	private LocalObject target;
	private float delta;

	public ScalingAnimation(LocalObject targeti, float finalScaling, float gameTime)
	{
		target = targeti;
		maxFrameTime = 2 * (int)(6.0f / gameTime);
		delta = (finalScaling - target.drawing.scaling) / maxFrameTime;
		frameTime = 0;
	}

	public override void Draw()
	{
		if (frameTime * 2 < maxFrameTime) target.drawing.scaling += delta;
		else target.drawing.scaling -= delta;
		base.Draw();
	}
}

class TextureAnimation : Animation
{
	private Texture2D texture;
	private RPoint position;
	private RMove rMove;

	public TextureAnimation(Texture2D texturei, Vector2 start, Vector2 finish, float gameTime)
	{
		position = new RPoint(start);
		rMove = new RMove(position, finish - start, gameTime);
		texture = texturei;

		maxFrameTime = (int)(gameTime * FramesInOneGameTime);
		frameTime = 0;
	}

	//public TextureAnimation(Texture2D _texture, Vector2 start, Vector2 finish, float gameTime) :
		//this(BigBase.Instance.textures.Get(textureName).Single(), start, finish, gameTime) { }

	public override void Draw()
	{
		MainScreen.Instance.Draw(texture, position);
		rMove.Draw();
		base.Draw();
	}

	public override bool SpendsTime { get { return true; } }
}

class TextAnimation : Animation
{
	Vector2 position, shift;
	Texture2D texture;
	SpriteFont font;
	Color color;
	string text;
	bool up;

	public TextAnimation(string texti, Texture2D texturei, SpriteFont fonti, Color colori, Vector2 positioni, float seconds, bool upi)
	{
		text = texti;
		position = positioni;
		texture = texturei;
		font = fonti;
		color = colori;
		up = upi;

		maxFrameTime = (int)(seconds * 60.0f);
		frameTime = 0;

		Random R = World.Instance.random;
		shift.X = R.Next(20) - 10;
		shift.Y = R.Next(20) - 10;
	}

	public override void Draw()
	{
		MainScreen m = MainScreen.Instance;
		if (texture != null) m.Draw(texture, position);
		m.DrawString(font, text, new ZPoint(position + shift) + new ZPoint(10, 10 + (up ? -1 : 1) * (int)(frameTime * 0.35f)), color);
		base.Draw();
	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Screen
{
	public ZPoint position, size;
	public int offset = 0;

	private MainScreen M { get { return MainScreen.Instance; } }

	public Screen(ZPoint positioni, ZPoint sizei) {
		position = positioni;
		size = sizei; }

	protected Screen() {
		ZPoint position = new ZPoint(10, 10);
		ZPoint size = new ZPoint(200, 100);	}

	public void DrawRectangle(ZPoint p, ZPoint s, Color color)
	{
		ZPoint realPosition = p;
		ZPoint realSize = s;

		if (s.x < 0) {
			realPosition.x = p.x + s.x;
			realSize.x = -s.x; }

		if (s.y < 0) {
			realPosition.y = p.y + s.y;
			realSize.y = -s.y; }

		Rectangle rectangle = new Rectangle(position.x + realPosition.x, position.y + realPosition.y, realSize.x, realSize.y);
        M.spriteBatch.Draw(M.universalTexture, rectangle, color);
	}

	public void Fill(Color color) {	DrawRectangle(ZPoint.Zero, size, color); }
	public void Draw(Texture2D texture, ZPoint p, Color color) { MainScreen.Instance.spriteBatch.Draw(texture, position + p, color); }
	public void Draw(Texture2D texture, ZPoint p) {	Draw(texture, p, Color.White); }
	public void Draw(Texture2D texture, Vector2 v) { Draw(texture, new ZPoint(v)); }

	public void DrawString(SpriteFont font, string text, ZPoint p, Color color)	{
		MainScreen.Instance.spriteBatch.DrawString(font, text, position + p, color);
		offset += (int)font.MeasureString(text).Y; }

	public void DrawStringWithShading(SpriteFont font, string text, ZPoint p, Color color) {
		DrawRectangle(p - new ZPoint(3, 0), new ZPoint(font.MeasureString(text)) + new ZPoint(6, 0), new Color(0, 0, 0, 0.8f));
		DrawString(font, text, p, color); }

	public void DrawString(SpriteFont font, string text, int y, Color color) {
		float x = size.x / 2.0f - font.MeasureString(text).X / 2.0f;
		DrawString(font, text, new ZPoint((int)x, y), color); }

	public void DrawString(SpriteFont font, string text, Color color) {
		Vector2 s = font.MeasureString(text);
		DrawString(font, text, new ZPoint((int)(0.5f * (size.x - s.X)), (int)(0.5f * (size.y - s.Y))), color); }

	public void DrawString(SpriteFont font, string text, ZPoint p, Color color, int length)	{
		DrawString(font, Stuff.Split(text, font, length), p, color); }
}

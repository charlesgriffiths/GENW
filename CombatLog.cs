using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class CombatLog
{
	List<List<Tuple<string, Color>>> data = new List<List<Tuple<string, Color>>>();

	public static MainScreen M { get { return MainScreen.Instance; } }
	public static int MaxLines { get { return 9; } }
	public static SpriteFont Font { get { return M.verdanaFont; } }
	public static int LineHeight { get { return (int)Font.MeasureString("ABC").Y; } }
	public static int Length { get { return 600; } }
	public static int Height { get { return 6 + MaxLines * LineHeight; } }
	public static ZPoint Position { get { return new ZPoint((int)Battlefield.ScreenPosition.X + 432 - Length / 2, M.size.y - 16 - Height); } }

	public void AddLine(string s, Color color)
	{
		if (data.Count >= MaxLines) data.Remove(data[0]);
		List<Tuple<string, Color>> line = new List<Tuple<string, Color>>();
		line.Add(new Tuple<string, Color>(s, color));
		data.Add(line);
	}

	public void Add(string s, Color color)
	{
		int n = data.Count;
		Log.Assert(n > 0, "combat log is empty");
		data[n - 1].Add(new Tuple<string, Color>(s, color));
	}

	public void Add(string s)
	{
		int n = data.Count;
		Log.Assert(n > 0, "combat log is empty");
		var line = data[n - 1];

		Add(s, line[line.Count - 1].Item2);
	}

	public void RemoveLastLine() { data.Remove(data[data.Count - 1]); }

	public void Draw()
	{
		Screen screen = new Screen(Position, new ZPoint(Length, Height));
		screen.Fill(new Color(0, 0, 0, 0.6f));

		int vOffset = 3 + LineHeight * (MaxLines - data.Count);
		foreach (var line in data)
		{
			int hOffset = 3;
			foreach (var part in line)
			{
				screen.DrawString(Font, part.Item1, new ZPoint(hOffset, vOffset), part.Item2);
				hOffset += (int)Font.MeasureString(part.Item1).X;
			}
			vOffset += LineHeight;
		}
	}
}

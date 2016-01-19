/*
Ближайшие планы:
1. Добавлять XP за разные достижения:
	крафтинг новых предметов
	победа над новыми глобальными объектами
	первые 3 удачные бартера
	просто открытие глобальной карты
	открытие сундуков на локальной карте

Commit:
*/

using System;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using (var game = new MyMonoGame()) game.Run();
    }
}
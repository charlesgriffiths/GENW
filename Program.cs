﻿/*
Ближайшие планы:
0. Все предметные абилки.

1. Ввести уровни, чтобы начать следить за user experience. Ради тестов просто ввести читерские консольные команды.

1. Реализовать бартер и соответствующих глобальных существ.
1. Кулдауны изображать просто цыферками на месте хоткеев и серым цветом, как и пассивки. Ха, точняк, большими цифрами!
1. Собаки после смерти должны оставлять куски мяса.
1. Анимировать функцию AddHP.
1. Предметы, лежащие на земле на глобальной карте.

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
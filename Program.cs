﻿/*
Ближайшие планы:
0. Анимация растяжением не очень нравится, добавить лучше спрайтовую.
0. Выводить описания эффектов.

1. Реализовать все имеющиеся силы и предметы.
1. Реализовать бартер и соответствующих глобальных существ.
1. S/A/I все-таки сделать атрибутами. Иначе мы не сможем расу огров иметь и одновременно использовать strength-условия в диалогах.
1. Глобальное инфо, включая скиллы.

2. Крипов в партии тоже можно кормить, перенося еду на их иконки.
2. Собаки после смерти должны оставлять куски мяса.

Commit: Added all racial abilities.
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
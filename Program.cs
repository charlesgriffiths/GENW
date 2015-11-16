/*
Ближайшие планы:
1. Интересно протупил, классы LCharacter и LCreep не нужны вообще!
1. Реализовать все имеющиеся силы и предметы.
1. Реализовать бартер и соответствующих глобальных существ.

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
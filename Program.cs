/*
Ближайшие планы:
1. Диалоговые окна в энкаунтерах.
2. Сражения.
*/

using System;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using (var game = new MyGame()) game.Run();
    }
}
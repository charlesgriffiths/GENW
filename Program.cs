/*
Ближайшие планы:
1. Сражения.
2. Изменить встречу с собаками так, чтобы они нападали только на слабые партии, а от сильных убегали.
Этого эффекта можно достичь с помощью jump="!".
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
/*
Ближайшие планы:
2. Интерактивные объекты на глобальной карте: закупка еды в деревнях, находки еды на природе и существа, самостоятельно
передвигающиеся по карте и ворующие еду у игрока в случае столкновения.
3. Combat.
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
﻿/*
Ближайшие планы:
0. По мостам должно быть можно ходить.
0. На глобальную shallow water наступать нельзя, это бред.
0. Проверять, что все лежит в одной компоненте связности.

1. Опять баг с неполными хитами. Я в принципе знаю, в каком месте его искать.
1. Adding XP for crapload of things: крафтинг новых предметов, убийство новых существ, победа над новыми глобальными объектами, первые 3 удачные бартера, просто открытие глобальной карты, применение новых абилок.

2. Добавить 11 новых палитр. Предусмотреть кастомные карты и кастомные сиды для повторного вхождения в ту же пещеру.
2. Размер карты должен определяться по количеству существ в сражении.

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
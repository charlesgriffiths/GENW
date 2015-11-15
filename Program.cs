﻿/*
Ближайшие планы:
1. Вместо автоматического конца битвы сделать кнопку "End Battle", которая большую часть битвы будет оставаться серой.
С помощью этой же кнопки можно убежать с поля боя.

2. Красивые подсказки к силам и инвентарю.
2. Реализовать все имеющиеся силы и предметы. Для этого сначала понадобится добавить эффекты.
2. Пример инвентаря: веревка и Mountain Pass. Возможность случайно потерять сопартийца там.
2. Реализовать бартер.

Commit: Added the "End Battle" button. Added item info.
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
﻿/*
Ближайшие планы:
0. Улучшить организацию кода с помощью частичных классов.
0. Текст в диалогах оформлять нормально: перенос на новую строку (в т.ч. во вариантах ответа) и выравнивание по второму символу во вариантах ответа.
В т.ч. выводить серым поле description.
0. Деревья на локальных картах.
0. Добавить здоровый глобальный лес и нормальную воду на выходе из подземелья лича. Монстров в этой зоне не будет.

1. Хм, а управление мышкой в сражениях можно сделать очень удобным, не понадобится даже о ctrl-movement говорить!
1. Иметь в виду, что на ноутбуках мои клавишы передвижения по глобальной карте находятся черт знает где.
1. Поэтому как можно быстрее реализовать честный поиск пути, хотя бы на глобальной карте.

2. Добавить вариации тайлов.
2. Сохранение локальных карт во временный текстовый файл.
2. Башни, деревни и улучшенные иконки на глобальной карте. Название деревней подписывать.
Устрелить имеющихся глобальных существ и диалоги с ними.
2. Картинку (глобальный тайл) энкаунтера выводить в окно диалога. Так будет красивее. Походу, придется сделать специальную базу для таких картинок.
Если соответствующей картинки нет, то рисовать просто кучей всех существ из партии объекта.
2. Классы и расы для игрока. Быстрая их смена. Силы, большинство из них пока серые. Устрелить имеющихся локальных существ.
2. Есть небольшой баг: глобальные существа при начале диалога могут убегать с позиции.

4. Подсказки при наведении мышкой на объекты, как на локальной карте, так и на глобальной. Посмотреть еще раз, как сделано в Crawl.
Управление мышкой в сражениях.
4. Все-таки вероятности попадания нам нужны. Продумать боевую систему из-за этого.

5. Скорость передвижения на глобальной карте должна зависеть от типа ландшафта, а анимация - от скорости.
5. Сделать известное исправление, исключающее возможные ошибки в ИИ-коде.
5. Улучшить наконец ИИ, чтобы монстры не бессмысленно передвигались. Added a basic AI.
5. Если убить лидера собак, то остальные разбегаются.

6. Инвентарь. Пока только глобальный.
6. Реализовать, что я придумал про Mountain Pass. Возможность случайно потерять сопартийца. Полезность веревки.
6. Вариации локальных тайлов.
6. Если навести мышкой на иконку на шкале, выделится соответствующее существо на локальной карте.
6. У отдельных оружий (острых) кровавый эффект.

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
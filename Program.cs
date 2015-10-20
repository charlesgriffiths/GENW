﻿/*
Ближайшие планы:
2. Использовать хорошие шрифты.
2. Сохранение локальных карт во временный текстовый файл.
2. Башни, деревни и улучшенные иконки на глобальной карте. Название деревней подписывать. Если в горном тайле пещера, то тайл проходим.
Устрелить имеющихся глобальных существ и диалоги с ними.
2. Деревья и кусты на локальных картах.
2. Картинку (глобальный тайл) энкаунтера выводить в окно диалога. Так будет красивее.
2. Классы и расы для игрока. Быстрая их смена. Силы, большинство из них пока серые. Устрелить имеющихся локальных существ.

3. Сразу после коммита. Понять, как текстовые файлы лучше добавлять в проект, чтобы загружать их на гитхаб.

4. Подсказки при наведении мышкой на объекты, как на локальной карте, так и на глобальной. Посмотреть еще раз, как сделано в Crawl.
Управление мышкой в сражениях.
4. Выводить повреждения в битвах на экран (как в kotc).

5. Честная партия героев и существ. Возможность терять сопартийцев в битвах.
5. Скорость передвижения на глобальной карте должна зависеть от типа ландшафта, а анимация - от скорости.
5. Сделать известное исправление, исключающее возможные ошибки в ИИ-коде.
5. Улучшить наконец ИИ, чтобы монстры не бессмысленно передвигались.
5. Если убить лидера собак, то остальные разбегаются.

6. Инвентарь. Пока только глобальный.
6. Реализовать, что я придумал про Mountain Pass. Возможность случайно потерять сопартийца. Полезность веревки.
6. Вариации локальных тайлов.
6. Если навести мышкой на иконку на шкале, выделится соответствующее существо на локальной карте.

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
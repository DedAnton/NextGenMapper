[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# Что такое Next Gen Mapper?
Вы правильно поняли - это очередной маппер, который позволит писать ещё меньше кода, по сравнению с мапперами "предыдущего поколения", а также сравняться по производительности с мапперами, написанными вручную.

# Идеология
Мне хотелось создать инструмент, который даёт максимальную отдачу при минимальных усилиях. Не тратит много твоего времени на обучение и освоение синтаксиса. Не усложняет работу в случае нетривиальных задач. Не скрывает детали реализации.

Пример:

```C#
using System;
using NextGenMapper;

namespace NextGenMapperDemo
{
    class Program
    {
        static void Main()
        {
            var source = new UserSource("Vasya", "Pupkin", new DateTime(2007, 01, 01));

            var destination = source.Map<UserDestination>();

            Console.WriteLine(destination.ToString());
        }
    }

    public record UserSource(string FirstName, string SecondName, DateTime Birthday);
    public record UserDestination(string FirstName, string SecondName, DateTime Birthday);
}
```

Ни атрибутов, ни конфигурации, никаких лишних действий, просто подключил пакет и написал `source.Map<Destination>()`

И нет вопросов, а как это работает, а не смапит ли оно что-то не так, а не съест ли слишком много производительности, потому что можно просто посмотреть что за метод вызывается

```C#
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    public static partial class Mapper
    {
        public static NextGenMapperDemo.UserDestination Map<To>(this NextGenMapperDemo.UserSource source) => new NextGenMapperDemo.UserDestination
        (
            source.FirstName,
            source.SecondName,
            source.Birthday
        )
        {
            
        };
        
    }
}
```

# Как это работает?
Я использю новую фичу языка C# - [Source Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/), она позволяет проанализировать написанный код и сгенерировань новые файлы, которые будут встроены в сборку.
Вот так выглядит метод который вызывается изначально:
```C#
public static partial class Mapper
{
    public static To Map<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");
}
```
Но когда мы его вызываем, генератор анализирует этот вызов, смотрит какие аргументы были переданы, и генерирует функцию для маппинга с сигнатурой `public static DestinationType Map<To>(this SourceType source)`. Хитрость в том, что сигнатуры методов идентичны, но у сгенерированного метода параметры более специфичны и подходят лучше, вызывается именно он ([такое поведение описано в спецификации](https://github.com/dotnet/csharplang/blob/a4c9db9a69ae0d1334ed5675e8faca3b7574c0a1/spec/expressions.md#better-function-member)). Такой подход создает некоторые проблемы, не все из которых решены, но я работаю над этим.
Эта сгенерированная функция помещается в статический класс и мы можем использовать её где угодно.

# Пример посложнее
Если для мапинга необходима сложная логика, то придётся написать кастомную функцию

```C#
using System;
using NextGenMapper;

namespace NextGenMapperDemo
{
    class Program
    {
        static void Main()
        {
            var source = new UserSource("Vasya", "Pupkin", new DateTime(2007, 01, 01));

            var destination = source.Map<UserDestination>();

            Console.WriteLine(destination.ToString());
        }
    }

    [Mapper]
    class CustomMaps
    {
        [Partial]
        UserDestination MyMap(UserSource src) => new UserDestination(
            $"{src.FirstName} {src.SecondName}", 
            (int)(DateTime.Now - src.Birthday).TotalDays / 365
        );
    }

    public record UserSource(string FirstName, string SecondName, DateTime Birthday);
    public record UserDestination(string Name, int Age);
}
```

И вот что в итоге будет сгенерировано

```C#
using System;
using NextGenMapper;
using NextGenMapperDemo;

namespace NextGenMapper
{
    public static partial class Mapper
    {
        
        public static NextGenMapperDemo.UserDestination Map<To>(this NextGenMapperDemo.UserSource _a___source)
        {
            NextGenMapperDemo.UserDestination _a__UserFunction(NextGenMapperDemo.UserSource src)
            {
                return new UserDestination($"{src.FirstName} {src.SecondName}", (int)(DateTime.Now - src.Birthday).TotalDays / 365);
            }
            var _a__result = _a__UserFunction(_a___source);
        
            return new NextGenMapperDemo.UserDestination
            (
                _a__result.Name,
                _a__result.Age
            )
            {
                
            };
        }
    }
}
```

Но что если в классах будет только одно свойство, которое нужно смапить с особенной логикой, а остальные свойства идентичны, не писать же маппинг для каждого свойства вручную. Для этой ситуации есть решение, это так называемые частичные кастомные методы.

Изменим классы чтобы подходили для примера

```C#
public record UserSource
{
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string SameProperty1 { get; set; }
    public string SameProperty2 { get; set; }
    public string SameProperty3 { get; set; }
}
public record UserDestination
{
    public string Name { get; set; }
    public string SameProperty1 { get; set; }
    public string SameProperty2 { get; set; }
    public string SameProperty3 { get; set; }
}
```
Напишем метод в котором укажем, как мапить FirstName и SecondName на Name и добавим к нему атрибут `[Partial]`

```C#
[Mapper]
class CustomMaps
{
    [Partial]
    UserDestination MyMap(UserSource src) => new UserDestination { Name = $"{src.FirstName} {src.SecondName}" };
}
```

И теперь посмотрим, что же было сгенерированно
```C#
public static NextGenMapperDemo.UserDestination Map<To>(this NextGenMapperDemo.UserSource _a___source)
{
    NextGenMapperDemo.UserDestination _a__UserFunction(NextGenMapperDemo.UserSource src)
    {
        return new UserDestination{Name = $"{src.FirstName} {src.SecondName}"};
    }
    var _a__result = _a__UserFunction(_a___source);

    return new NextGenMapperDemo.UserDestination
    (

    )
    {
        Name = _a__result.Name,
        SameProperty1 = _a___source.SameProperty1,
        SameProperty2 = _a___source.SameProperty2,
        SameProperty3 = _a___source.SameProperty3
    };
}
```

Выглядит немного сложнее чем прошлый пример, но ничего сверхъестественного тут нет. Кастомная частичная функция, которую мы написали, преобразуется в локальную функцию, используя её мы создаём объект типа `DestinationType` с свойствами, которые мы инициализируем в кастомной функции. Затем мы создаем новый объект, подставляя в него свойства, инициализированные кастомной функцией, получая их из результата работы локальной функции, и мапим все оставшиеся свойства. После чего возвращаем этот объект как результат.

Как вы могли заметить в паследнем примере классы, а точнее записи (или рекорды), значительно изменились, мы сделали каждое свойство открытым для изменения, что не всегда хорошо. Если мы хотим сделать поля только для чтения, нам нужно будет сделать два конструктора, один мы будем использовать только для кастомной функции, а второй будет инициализировать все поля
```C#
public record UserDestination
{
    public UserDestination(string name)
    {
        Name = name;
    }

    public UserDestination(string name, string sameProperty1, string sameProperty2, string sameProperty3) : this(name)
    {
        SameProperty1 = sameProperty1;
        SameProperty2 = sameProperty2;
        SameProperty3 = sameProperty3;
    }

    public string Name { get; }
    public string SameProperty1 { get; }
    public string SameProperty2 { get; }
    public string SameProperty3 { get; }
}
```

Мне такой вариант не очень нравится. Другой способ, это используя новую фишку C#, пометить свойства как доступные для инициализации, то есть мы сможем инициализировать их при создании объекта, но не сможем их потом изменить, то что нужно (это не фишка рекордов, с классами это тоже работает)
```C#
public class UserDestination
{
    public string Name { get; init; }
    public string SameProperty1 { get; init; }
    public string SameProperty2 { get; init; }
    public string SameProperty3 { get; init; }
}
```

Но это не всё, есть третий вариант, очень необычный, эксперементальный, не по назначению использующий синтаксис языка.

Оставим в `UserDestination` только один конструктор с свойствами только для чтения
```C#
public class UserDestination
{
    public UserDestination(string name, string sameProperty1, string sameProperty2, string sameProperty3)
    {
        Name = name;
        SameProperty1 = sameProperty1;
        SameProperty2 = sameProperty2;
        SameProperty3 = sameProperty3;
    }

    public string Name { get; }
    public string SameProperty1 { get; }
    public string SameProperty2 { get; }
    public string SameProperty3 { get; }
}
```

И теперь напишем кастомную функцию для маппинга
```C#
[Partial]
UserDestination MyMap(UserSource src) => new UserDestination($"{src.FirstName} {src.SecondName}", default, default, default);
```

Да, мы просто передайм `default` в качестве аргументов, которые мы не хотим мапить вручную. Я понимаю, что не все это оценят, и тут есть о чем поспорить, хотя бы о том, что все равно нужно что-то писать, и если будет десять таких свойств, то десять раз придётся писать `default`, но мне всё равно нравится этот способ, и я лично буду использовать его иногда. 

А вот что получается в сгенерированном файле
```C#
public static NextGenMapperDemo.UserDestination Map<To>(this NextGenMapperDemo.UserSource src) => new NextGenMapperDemo.UserDestination
(
    $"{src.FirstName} {src.SecondName}",
    src.SameProperty1,
    src.SameProperty2,
    src.SameProperty3
)
{

};
```

# Приемущества
Говоря о приемуществах я буду сравнивать Next Gen Mapper с типичными мапперами, вроде AutoMapper.

- Меньше объем кода
- Проще научится пользоваться, нет множетсва специальных методов, особого синтаксиса для описания кофигурации маппинга
- Производительность на уровне написанного вручную маппера
- То, как будет проведен маппинг не скрывается, можно посмотреть на сгенерированный метод, нажав F12 (Go to definition), поставить точку останова, подебажить его в случае необходимости
- Не тратится лишнее время при работе приложения, потому что функции для маппинга генерируются во время компиляции
- Меньше ошибок в рантайме, например, если вы неправильно написали кастомный метод, или забыли добавить публичный конструктор к классу, вы узнаете об это на этапе компиляции. У мапперов обычно есть метод, который можно запускать в тестах, который проверяет, правильно ли был сконфигурирован маппинг, но я считаю, что это менее удобно чем диагностики в visual studio
- Меньше библиотек на выходе, сгенерированные классы добавляются в сборку пользователя, а сама библиотека NextGenMapper для работы приложения не нужна

# Недостатки
- Технология Source Generators всё ещё сыровата, например, приходится перезапускать студию чтобы IntelliSense начала работать с генератором (вроде только один раз при подключении пакета), также я испытывал некоторые проблемы при разработке, у меня несколько раз ломалась студи (позже я смог с этим справится)
- Необходимо использовать .NET 5, не возможно использовать в старых проектах
- Можно не заметить как что-то сломалось, достаточно изменить какие-то поля в классе, или его конструктор, и функция для маппига сгенерируется заного, и возможно с ошибками, хотя в идеале пользователю должна показаться ошибка в диагностиках, и я думаю, что от этой проблемы можно почти польностью избавится. (Подобная проблема может быть и в других мапперах) 

# Коллекции
На данный момент поддерживается маппинг для следуюущих типов коллекций
- List
- Array
- ICollection_T,
- IEnumerable_T,
- IList_T,
- IReadOnlyCollection_T,
- IReadOnlyList_T

В планах добавить поддержку иммутабельных коллекций

Апи для маппинга коллекций ничем не отличается
```C#
class Program
{
    static void Main(string[] args)
    {
        var source = new UserSource[] { new ("Vasya", "Pupkin") };

        var destination = source.Map<UserDestination[]>();

        Console.WriteLine(destination[0].ToString());
    }
}

public record UserSource(string FirstName, string SecondName);
public record UserDestination(string FirstName, string SecondName);
```

При этом генерируется два метода, для маппинга `UserSource` в `UserDestination`, и для мапинга `UserSource[]` в `UserDestination[]`
```C#
public static NextGenMapperDemo.UserDestination Map<To>(this NextGenMapperDemo.UserSource source) => new NextGenMapperDemo.UserDestination
(
    source.FirstName,
    source.SecondName
)
{

};

public static NextGenMapperDemo.UserDestination[] Map<To>(this NextGenMapperDemo.UserSource[] sources)
    => sources.Select(x => x.Map<NextGenMapperDemo.UserDestination>()).ToArray();
```

# Перечисления
Перечисления мапятся по простому правилу, если есть заданное в коде значение, то мапится по нему, если нет, то по имени
```C#
class Program
{
    static void Main(string[] args)
    {
        var source = SourceEnum.A;

        var destination = source.Map<DestinationEnum>();

        Console.WriteLine(destination.ToString());
    }
}

public enum SourceEnum
{
    A,
    B,
    C,
    X = 123
}

public enum DestinationEnum
{
    A,
    B,
    C,
    Y = 123
}
```

и сгенерированная функция для маппинга
```C#
public static NextGenMapperDemo.DestinationEnum Map<To>(this NextGenMapperDemo.SourceEnum source) => source switch
{
    NextGenMapperDemo.SourceEnum.A => NextGenMapperDemo.DestinationEnum.A,
    NextGenMapperDemo.SourceEnum.B => NextGenMapperDemo.DestinationEnum.B,
    NextGenMapperDemo.SourceEnum.C => NextGenMapperDemo.DestinationEnum.C,
    NextGenMapperDemo.SourceEnum.X => NextGenMapperDemo.DestinationEnum.Y,
    _ => throw new System.ArgumentOutOfRangeException("Error when mapping NextGenMapperDemo.SourceEnum to NextGenMapperDemo.DestinationEnum")
};
```
 
# Как использовать
Достаточно просто подключить пакет `NextGenMapper`. Для работы необходим .NET 5
```
dotnet add package NextGenMapper --prerelease
```
:hammer_and_wrench: в данный момент это сделать невозможно, я не добавил пакет в nuget, но я скоро это исправлю

# Какая-то тактика которой я придерживаюсь
В данный момент Next Gen Mapper нельзя использовать в серьёзный проектах, и первостепенная задача это довести его до состояния, когда его можно будет использовать в продакшене.
Для этого необходимо добавить все базовые функции (большая чать уже добавлена), поработать над производительностью, а также обеспечить стабильность.

Следить за работой можно на [доске проекта](https://github.com/DedAnton/NextGenMapper/projects/1)

# Чем помочь
Для того чтобы помочь достаточно установить пакет и опробовать его на своих реальных проектах, а затем создать issue и описать, насколько просто было использовать маппер, с какими проблемами пришлось столкнуться, и что можно улучшить.
Эта информация сейчас очень ценна.

Также можно брать задачи с лейблом `good first issue`, подробнее [тут]()

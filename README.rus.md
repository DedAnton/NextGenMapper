<p align="center">
    <img src="https://user-images.githubusercontent.com/36799941/191375272-27b0034d-0418-44a6-95c6-802b863de2b3.svg" width="242" height="242">
</p>
<p align="center">
    <a href="https://opensource.org/licenses/MIT">
        <img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT">
    </a>
    <img alt="GitHub release (latest by date including pre-releases)" src="https://img.shields.io/github/v/release/DedAnton/NextGenMapper?include_prereleases">
    <a href="https://vk.com/away.php?utf=1&to=https%3A%2F%2Fwww.tinkoff.ru%2Fcf%2F3ySZ9DEsxfL">
        <img src="https://img.shields.io/badge/%24-donate-9cf" alt="donate">
    </a>
    <h4 align="center">Чрезвычайно быстрый и легкий, минималистичный объектный маппер, генерируемый на лету</h4>
</p>

https://user-images.githubusercontent.com/36799941/191618500-31f7e179-3510-49dc-ad13-18e07de8309b.mov

# Особенности
 - Генерация методов для маппинга на лету
 - Не используется рефлексия и деревья выражений
 - Производительность как у маппера, написанного вручную
 - Минимум выделения памяти
 - Не увеличивает время запуска приложения
 - Отсутствие зависимостей в итоговой сборке
 - Без сторонних инструментов и зависимостей IDE
 - Не мешает статическому анализу
 - Поддержка навигации по коду
 - Простота отладки
 - Никаких атрибутов и текучего апи

NextGenMapper - это инструмент, который просто решает проблему, и старается не создавать новых. 

# Использование

Добавьте `using NextGenMapper` и вызовите метод расширения `Map` на объекте, который хотите смаппить
```c#
using NextGenMapper;

var source = new Source("Anton", 25);

var destination = source.Map<Destination>();

Console.WriteLine(destination);

record Source(string Name, int Age);
record Destination(string Name, int Age);
```
<br>

Для того, чтобы настроить маппинг определённых свойств, вместо метода `Map`, вызовите метод `MapWith`, передав значение переопределяемого свойтсва в качестве аргумента
```c#
using NextGenMapper;

var source = new Source("Anton", "Ryabchikov", 25);

var destination = source.MapWith<Destination>(name: source.FirstName + ' ' + source.LastName);

Console.WriteLine(destination);

record Source(string FirstName, string LastName, int Age);
record Destination(string Name, int Age);
```
<br>

Чтобы NextGenMapper использовал ваш маппинг при маппинге других объектов, нужно создать частичный класс `Mapper` в пространстве имен `NextGenMapper` и добавить в него метод `Map` с вашей реализацией
```c#
namespace NextGenMapper;

internal static partial class Mapper
{
    internal static Destination Map<To>(this Source source) 
        => source.MapWith<Destination>(name: source.FirstName + ' ' + source.LastName);
}
```
<br>

На данный момент поддерживаются следующие типы коллекций: `List<T>`, `Array<T>`, `ICollection<T>`, `IEnumerable<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`.
```c#
var sourceCollection = new List<Source> { new("Anton", 25) };

var destination = sourceCollection.Map<List<Destination>>();
```
<br>

Перечисления также можно маппить.
```c#
var source = Source.EnumValue;

var destination = source.Map<Destination>();
```
<br>

> **Note**: 
> Из-за использования новой технологии, на некоторых версиях Visual Studio иногда могут возникать проблемы с подсветкой синтаксиса, если IntelliCode говорит об ошибке, но решение билдиться без ошибок - просто перезагрузите Visual Studio 

### Установка

Добавить пакет можно через package manager console:
```
PM> Install-Package NextGenMapper -prerelease
```
Или через .Net CLI:
```
dotnet add package NextGenMapper --prerelease
```

# Как это работает?
NextGenMapper использует новую возможность языка C# - [Генераторы исходного кода](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/). Описать работу Генератора исходного кода можно следующими шагами:
 1. Код компилируется
 2. Генератор исходного кода анализирует сборку
 3. Генерирует новый код на основе анализа
 4. Компилирует новый код и добавляет его к сборке

Вот так выглядит метод который вызывается изначально:
```C#
internal static To Map<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");
```
Когда мы его вызываем, генератор анализирует этот вызов, и генерирует функцию для маппинга:
```C#
internal static Destination Map<To>(this Source source) 
    => new Destination(source.Name, source.Age);
```
Хитрость в том, что сигнатуры методов идентичны, но у сгенерированного метода параметры более специфичны и подходят лучше, поэтому вызывается именно он ([такое поведение описано в спецификации](https://github.com/dotnet/csharplang/blob/a4c9db9a69ae0d1334ed5675e8faca3b7574c0a1/spec/expressions.md#better-function-member)).

# Скоро
 - Поддержка IQueryable 
 - Использование [Incremental Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)

Все задачи и ход их выполнения можно посмотреть на [доске проекта](https://github.com/users/DedAnton/projects/3)
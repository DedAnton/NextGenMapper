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
    <h4 align="center">Extremely fast and lightweight, minimalistic object-to-object mapper, generated on the fly</h4>
</p>

https://user-images.githubusercontent.com/36799941/191618500-31f7e179-3510-49dc-ad13-18e07de8309b.mov

# What is Next Gen Mapper?
You understood correctly - this is another mapper that will allow you to write even less code compared to the "previous generation" mappers, as well as be equal in performance to hand-written mappers.

# Ideology
I wanted to create a tool that gives you the most bang for your buck with the least amount of effort. Doesn't spend a lot of your time learning and mastering the syntax. Doesn't complicate work in case of non-trivial tasks. Doesn't hide implementation details.

Example:

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

No attributes, no configuration, no unnecessary actions, just connected the package and wrote `source.Map <Destination> ()`

And there are no questions, but how exactly the mapping takes place, and not whether it will map something wrong, and whether it will eat too much performance, because you can just see what kind of method is called

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

# How it works?
I am using a new feature of the C # language - [Source Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/), it allows you to analyze the written code and generate new files that will be embedded in the assembly.
This is how the method that is called initially looks like:
```C#
public static partial class Mapper
{
    public static To Map<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");
}
```
But when we call it, the generator analyzes this call, looks at what arguments were passed, and generates a function for mapping with the signature `public static DestinationType Map <To> (this SourceType source)`. The trick is that the method signatures are identical, but the parameters of the generated method are more specific and fit better, it is it that is called ([this behavior is described in the specification](https://github.com/dotnet/csharplang/blob/a4c9db9a69ae0d1334ed5675e8faca3b7574c0a1/spec/expressions.md#better-function-member)). This approach creates some problems, not all of which have been resolved, but I am working on it.
This generated function is placed in a static class and we can use it anywhere.

# A more complex example
If you need complex logic for mapping, then you have to write a custom function

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

And this is what will eventually be generated

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

But what if there is only one property in the classes that needs to be mapped with special logic, and the rest of the properties are identical, write the mapping for each property manually? There is a solution for this situation, these are the so-called partial custom methods.

Let's change the classes to suit the example

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
Let's write a method in which we indicate how to map FirstName and SecondName to Name and add the attribute `[Partial]` to it

```C#
[Mapper]
class CustomMaps
{
    [Partial]
    UserDestination MyMap(UserSource src) => new UserDestination { Name = $"{src.FirstName} {src.SecondName}" };
}
```

And now let's see what was generated
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

It looks a little more complicated than the previous example, but there is nothing supernatural here. The custom partial function that we wrote is converted into a local function, using it we create an object of type `DestinationType` with properties that we initialize in the custom function. Then we create a new object, substituting into it the properties initialized by the custom function, getting them from the result of the local function, and map all the remaining properties. Then we return this object as a result.

As you may have noticed in the past example, the classes, or rather the records, have changed significantly, we made each property open to change, which is not always good. If we want to make the fields read-only, we will need to make two constructors, one we will use only for the custom function, and the second will initialize all the fields.
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

I don't really like this option. Another way is to use a new C # trick to mark properties as available for initialization, that is, we can initialize them when creating an object, but we cannot change them later, what is needed (this is not a record feature, this also works with classes)
```C#
public class UserDestination
{
    public string Name { get; init; }
    public string SameProperty1 { get; init; }
    public string SameProperty2 { get; init; }
    public string SameProperty3 { get; init; }
}
```

But that's not all, there is a third option, a very unusual, experimental one that uses the syntax of the language inappropriately.

Leave in `UserDestination` only one constructor with read-only properties
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

And now let's write a custom mapping function
```C#
[Partial]
UserDestination MyMap(UserSource src) => new UserDestination($"{src.FirstName} {src.SecondName}", default, default, default);
```

Yes, we just pass `default` as arguments, which we don't want to manually map. I understand that not everyone will appreciate this, and there is something to argue about, at least about the fact that you still need to write something, and if there are ten such properties, then ten times will have to write `default`, but I don't care like this way and I personally will use it sometimes.

And here is what happens in the generated file
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

# Advantages
I will be comparing Next Gen Mapper with typical mappers like AutoMapper.

- Less code size
- Easier to learn to use, there are no many special methods, special syntax for describing the mapping configuration
- Performance like a hand-written mapper
- How the mapping will be carried out is not hidden, you can look at the generated method by pressing F12 (Go to definition), set a breakpoint, debug it if necessary
- No extra time wasted when the application is running, because the mapping functions are generated at compile time
- Fewer runtime errors, for example, if you wrote a custom method incorrectly, or forgot to add a public constructor to a class, you will find out about this at compile time. Mappers usually have a method that can be run in tests that checks if the mapping has been configured correctly, but I find it less convenient than diagnostics in visual studio
- Fewer libraries in the output, generated classes are added to the user's assembly, and the NextGenMapper library itself is not needed for the application to work

# Disadvantages
- The Source Generators technology is still damp, for example, you have to restart the studio for IntelliSense to start working with the generator (it seems like only once when connecting the package), I also experienced some problems during development, my studio broke several times (later I was able to cope with this )
- It is necessary to use .NET 5, it is not possible to use in old projects
- You may not notice how something is broken, it is enough to change some fields in the class, or its constructor, and the function for the mappig will be generated incorrectly, and possibly with errors, although ideally the user should see an error in diagnostics, and I think that this problem can be almost completely eliminated. (A similar problem may exist in other mappers)

# Collections
Currently, mapping is supported for the following collection types
- List
- Array
- ICollection_T,
- IEnumerable_T,
- IList_T,
- IReadOnlyCollection_T,
- IReadOnlyList_T

Plans to add support for immutable collections

The api for mapping collections is no different
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

This generates two methods, for mapping `UserSource` to` UserDestination`, and for mapping `UserSource []` into `UserDestination []`
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

# Enumerations
Enumerations are mapped according to a simple rule, if there is a value specified in the code, then it is mapped by it, if not, then by name
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

and the generated mapping function
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
 
# How to use
It is enough just to include the package `NextGenMapper`. .NET 5 is required to work
```
PM> Install-Package NextGenMapper -prerelease
```
add `using NextGenMapper;` in class where you want to use mapping.

You may need to restart Visual Studio for intelliSense to start working with the generator.

# Plans
At the moment, Next Gen Mapper cannot be used in serious projects, and the primary task is to bring it to a state where it can be used in production.
To do this, you need to add all the basic functions (a lot has already been added), work on performance, and also ensure stability.

You can follow the work on the [project board](https://github.com/DedAnton/NextGenMapper/projects/1)

# How can I help
In order to help, it is enough to install the package and test it on your real projects, and then create an issue and describe how easy it was to use the mapper, what problems you had to face, and what could be improved.
This information is now very valuable.

You can also take tasks with the label `good first issue` [here](https://github.com/DedAnton/NextGenMapper/projects/1?card_filter_query=label%3A%22good+first+issue%22). And also find mistakes in the readme.

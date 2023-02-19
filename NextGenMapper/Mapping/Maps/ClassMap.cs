using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.Mapping.Maps.Models;

namespace NextGenMapper.Mapping.Maps;

internal interface IMap
{
    string Source { get; }
    string Destination { get; }
}

internal readonly struct ClassMap : IMap, IEquatable<ClassMap>
{
    public ClassMap(
        string source,
        string destination,
        ImmutableArray<PropertyMap> constructorProperties,
        ImmutableArray<PropertyMap> initializerProperties)
    {
        Source = source;
        Destination = destination;
        ConstructorProperties = constructorProperties;
        InitializerProperties = initializerProperties;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<PropertyMap> ConstructorProperties { get; }
    public ImmutableArray<PropertyMap> InitializerProperties { get; }

    public bool Equals(ClassMap other) => Source == other.Source && Destination == other.Destination;
    //public SourceLocation Location { get; }
}

//public readonly struct SourceLocation
//{
//    public SourceLocation(string filePath, TextSpan textSpan, LinePositionSpan lineSpan)
//    {
//        FilePath = filePath;
//        TextSpan = textSpan;
//        LineSpan = lineSpan;
//    }

//    public SourceLocation(Location location)
//    {
//        FilePath = location.
//    }

//    public string FilePath { get; }
//    public TextSpan TextSpan { get; }
//    public LinePositionSpan LineSpan { get; }
//}

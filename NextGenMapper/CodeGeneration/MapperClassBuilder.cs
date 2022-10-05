using NextGenMapper.CodeAnalysis.Maps;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeGeneration;

public class MapperClassBuilder
{
    private const int TabWidth = 4;

    private const string Usings = "using NextGenMapper.Extensions;";
    private const string Namespace = "namespace NextGenMapper";
    private const string Class = "internal static partial class Mapper";

    private const string NewLine = "\r\n";
    private const char OpenBracket = '(';
    private const char CloseBracket = ')';
    private const char OpenCurlyBracket = '{';
    private const char CloseCurlyBracket = '}';
    private const char OpenAngleBracket = '<';
    private const char CloseAngleBracket = '>';
    private const char Equal = '=';
    private const char Dot = '.';
    private const char Comma = ',';
    private const char Semicolon = ';';
    private const char Quotes = '"';
    private const char Underscore = '_';
    private const char WhiteSpace = ' ';
    private const string Lambda = "=>";
    private const char X = 'x';

    private const string Internal = "internal";
    private const string Static = "static";
    private const string Source = "source";
    private const string Map = "Map";
    private const string MapWith = "MapWith";
    private const string To = "To";
    private const string This = "this";
    private const string New = "new";
    private const string Switch = "switch";
    private const string Throw = "throw";
    private const string Select = "Select";
    private const string Return = "return";
    private const string Var = "var";

    public string Generate(IReadOnlyCollection<TypeMap> maps)
    {
        var builder = new ValueStringBuilder(stackalloc char[1024]);

        builder.Append(Usings);
        builder.Append(NewLine);
        builder.Append(NewLine);
        builder.Append(Namespace);
        builder.Append(NewLine);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 1);
        builder.Append(Class);
        builder.Append(NewLine);
        AppendTabs(ref builder, 1);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        foreach (var map in maps)
        {
            if (map is ClassMapWithStub classMapWithStub)
            {
                AppendPlaceholderClassMapWith(ref builder, classMapWithStub);
            }
            if (map is ClassMapWith classMapWith)
            {
                if (classMapWith.Arguments.Length > 0)
                {
                    AppendClassMapWith(ref builder, classMapWith);
                }
            }
            else if (map is ClassMap classMap)
            {
                AppendCommonMapMethod(ref builder, classMap);
            }
            else if (map is EnumMap enumMap)
            {
                AppendEnumMapMethod(ref builder, enumMap);
            }
            else if (map is CollectionMap collectionMap)
            {
                AppendCollectionMapMethod(ref builder, collectionMap);
            }
            builder.Append(NewLine);
            builder.Append(NewLine);
        }
        AppendTabs(ref builder, 1);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        builder.Append(CloseCurlyBracket);

        return builder.ToString();
    }

    private void AppendClassMapWith(ref ValueStringBuilder builder, ClassMapWith map)
    {
        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(WhiteSpace);
        builder.Append(MapWith);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(map.From.ToString());
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(Comma);
        builder.Append(NewLine);
        var counter = 1;
        foreach (var argument in map.Arguments)
        {
            AppendTabs(ref builder, 3);
            builder.Append(argument.Type.ToString());
            builder.Append(WhiteSpace);
            builder.Append(argument.Name);

            if (counter < map.Arguments.Length)
            {
                builder.Append(Comma);
            }
            builder.Append(NewLine);
            counter++;
        }
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        counter = 1;
        foreach (var property in map.ConstructorProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            var argument = Array.Find(map.Arguments, x => property.ToName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
            if (argument is not null)
            {
                builder.Append(argument.Name);
            }
            else
            {
                builder.Append(Source);
                builder.Append(Dot);
                builder.Append(property.FromName);
                if (!(property.IsSameTypes || property.HasImplicitConversion))
                {
                    builder.Append(Dot);
                    builder.Append(Map);
                    builder.Append(OpenAngleBracket);
                    builder.Append(property.ToType.ToString());
                    builder.Append(CloseAngleBracket);
                    builder.Append(OpenBracket);
                    builder.Append(CloseBracket);
                }
            }
            if (counter < map.ConstructorProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2); ;
        builder.Append(OpenCurlyBracket);
        counter = 1;
        foreach (var property in map.InitializerProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(property.ToName);
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            var argument = Array.Find(map.Arguments, x => property.ToName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
            if (argument is not null)
            {
                builder.Append(argument.Name);
            }
            else
            {
                builder.Append(Source);
                builder.Append(Dot);
                builder.Append(property.FromName);
                if (!(property.IsSameTypes || property.HasImplicitConversion))
                {
                    builder.Append(Dot);
                    builder.Append(Map);
                    builder.Append(OpenAngleBracket);
                    builder.Append(property.ToType.ToString());
                    builder.Append(CloseAngleBracket);
                    builder.Append(OpenBracket);
                    builder.Append(CloseBracket);
                }
            }
            if (counter < map.InitializerProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    private void AppendPlaceholderClassMapWith(ref ValueStringBuilder builder, ClassMapWithStub map)
    {
        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(map.To.ToString());
        builder.Append(WhiteSpace);
        builder.Append(MapWith);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(map.From.ToString());
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(Comma);
        builder.Append(NewLine);
        var counter = 1;
        foreach (var property in map.Parameters)
        {
            AppendTabs(ref builder, 3);
            builder.Append(property.Type.ToString());
            builder.Append(WhiteSpace);
            builder.Append(property.Name);
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            builder.Append("default");

            if (counter < map.Parameters.Length)
            {
                builder.Append(Comma);
            }
            counter++;
            builder.Append(NewLine);
        }
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Throw);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        //TODO: add diagnostic for prevent calling stub method
        builder.Append("System.NotImplementedException(\"This method is a stub and is not intended to be called\")");
        builder.Append(Semicolon);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
    }

    private void AppendAbstractCollectionMapMethod(ref ValueStringBuilder builder, CollectionMap map)
    {
        AppendTabs(ref builder, 2);
        builder.Append("internal static ");
        builder.Append(map.To.ToString());
        builder.Append(" Map<To>(this ");
        builder.Append(map.From.ToString());
        builder.Append(" source)");
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("if (!source.TryGetSpan(out var span))");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 4);
        builder.Append("span = System.Linq.Enumerable.ToArray(source);");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        if (map.CollectionTo is CollectionType.List or CollectionType.IReadOnlyList or CollectionType.IReadOnlyCollection)
        {
            builder.Append("var destination = new System.Collections.Generic.List<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">(span.Length);");
        }
        else if (map.CollectionTo is CollectionType.Array or CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList)
        {
            builder.Append("var destination = new ");
            builder.Append(map.ItemTo.ToString());
            builder.Append("[span.Length];");
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("for (var i = 0; i < span.Length; i++)");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 4);
        if (map.CollectionTo is CollectionType.List or CollectionType.IReadOnlyList or CollectionType.IReadOnlyCollection)
        {
            builder.Append("destination.Add(span[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">());");
        }
        else if (map.CollectionTo is CollectionType.Array or CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList)
        {
            builder.Append("destination[i] = span[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">();");
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("return destination;");
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
    }

    private void AppendArrayMapMethod(ref ValueStringBuilder builder, CollectionMap map)
    {
        AppendTabs(ref builder, 2);
        builder.Append("internal static ");
        builder.Append(map.To.ToString());
        builder.Append(" Map<To>(this ");
        builder.Append(map.From.ToString());
        builder.Append(" source)");
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        if (map.CollectionTo is CollectionType.List or CollectionType.IReadOnlyList or CollectionType.IReadOnlyCollection)
        {
            builder.Append("var destination = new System.Collections.Generic.List<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">(source.Length);");
        }
        else if (map.CollectionTo is CollectionType.Array or CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList)
        {
            builder.Append("var destination = new ");
            builder.Append(map.ItemTo.ToString());
            builder.Append("[source.Length];");
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("var sourceSpan = new System.Span<");
        builder.Append(map.ItemFrom.ToString());
        builder.Append(">(source);");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("for (var i = 0; i < source.Length; i++)");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 4);
        if (map.CollectionTo is CollectionType.List or CollectionType.IReadOnlyList or CollectionType.IReadOnlyCollection)
        {
            builder.Append("destination.Add(sourceSpan[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">());");
        }
        else if (map.CollectionTo is CollectionType.Array or CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList)
        {
            builder.Append("destination[i] = sourceSpan[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">();");
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("return destination;");
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
    }

    private void AppendListMapMethod(ref ValueStringBuilder builder, CollectionMap map)
    {
        AppendTabs(ref builder, 2);
        builder.Append("internal static ");
        builder.Append(map.To.ToString());
        builder.Append(" Map<To>(this ");
        builder.Append(map.From.ToString());
        builder.Append(" source)");
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        if (map.CollectionTo is CollectionType.List or CollectionType.IReadOnlyList or CollectionType.IReadOnlyCollection)
        {
            builder.Append("var destination = new System.Collections.Generic.List<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">(source.Count);");
        }
        else if (map.CollectionTo is CollectionType.Array or CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList)
        {
            builder.Append("var destination = new ");
            builder.Append(map.ItemTo.ToString());
            builder.Append("[source.Count];");
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("#if NET5_0_OR_GREATER");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("var sourceSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(source);");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("#endif");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("for (var i = 0; i < source.Count; i++)");
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(OpenCurlyBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 4);
        if (map.CollectionTo is CollectionType.List or CollectionType.IReadOnlyList or CollectionType.IReadOnlyCollection)
        {
            builder.Append("#if NET5_0_OR_GREATER");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("destination.Add(sourceSpan[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">());");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("#else");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("destination.Add(source[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">());");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("#endif");
        }
        else if (map.CollectionTo is CollectionType.Array or CollectionType.IEnumerable or CollectionType.ICollection or CollectionType.IList)
        {
            builder.Append("#if NET5_0_OR_GREATER");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("destination[i] = sourceSpan[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">();");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("#else");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("destination[i] = source[i].Map<");
            builder.Append(map.ItemTo.ToString());
            builder.Append(">();");
            builder.Append(NewLine);
            AppendTabs(ref builder, 4);
            builder.Append("#endif");
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(CloseCurlyBracket);
        builder.Append(NewLine);
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append("return destination;");
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
    }

    private void AppendCollectionMapMethod(ref ValueStringBuilder builder, CollectionMap map)
    {
        if (map.CollectionFrom is CollectionType.IEnumerable or CollectionType.ICollection 
            or CollectionType.IList or CollectionType.IReadOnlyCollection or CollectionType.IReadOnlyList)
        {
            AppendAbstractCollectionMapMethod(ref builder, map);
        }
        else if (map.CollectionFrom is CollectionType.Array)
        {
            AppendArrayMapMethod(ref builder, map);
        }
        else if (map.CollectionFrom is CollectionType.List)
        {
            AppendListMapMethod(ref builder, map);
        }
        else
        {
            throw new NotImplementedException("Collection not supported");
        }
    }

    private void AppendEnumMapMethod(ref ValueStringBuilder builder, EnumMap map)
    {
        var from = map.From.ToString().AsSpan();
        var to = map.To.ToString().AsSpan();

        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(WhiteSpace);
        builder.Append(Map);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(OpenBracket);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(WhiteSpace);
        builder.Append(Switch);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenCurlyBracket);
        foreach (var field in map.Fields)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(field.FromType.ToString());
            builder.Append(Dot);
            builder.Append(field.FromName);
            builder.Append(WhiteSpace);
            builder.Append(Lambda);
            builder.Append(WhiteSpace);
            builder.Append(field.ToType.ToString());
            builder.Append(Dot);
            builder.Append(field.ToName);
            builder.Append(Comma);
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 3);
        builder.Append(Underscore);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(Throw);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append("System.ArgumentOutOfRangeException");
        builder.Append(OpenBracket);
        builder.Append(Quotes);
        builder.Append("Error when mapping");
        builder.Append(WhiteSpace);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append("to");
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(Quotes);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    private void AppendCommonMapMethod(ref ValueStringBuilder builder, ClassMap map)
    {
        var from = map.From.ToString().AsSpan();
        var to = map.To.ToString().AsSpan();

        AppendTabs(ref builder, 2);
        builder.Append(Internal);
        builder.Append(WhiteSpace);
        builder.Append(Static);
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(WhiteSpace);
        builder.Append(Map);
        builder.Append(OpenAngleBracket);
        builder.Append(To);
        builder.Append(CloseAngleBracket);
        builder.Append(OpenBracket);
        builder.Append(This);
        builder.Append(WhiteSpace);
        builder.Append(from);
        builder.Append(WhiteSpace);
        builder.Append(Source);
        builder.Append(CloseBracket);
        builder.Append(WhiteSpace);
        builder.Append(Lambda);
        builder.Append(WhiteSpace);
        builder.Append(New);
        builder.Append(WhiteSpace);
        builder.Append(to);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(OpenBracket);
        var counter = 1;
        foreach (var property in map.ConstructorProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(Source);
            builder.Append(Dot);
            builder.Append(property.FromName);
            if (!(property.IsSameTypes || property.HasImplicitConversion))
            {
                builder.Append(Dot);
                builder.Append(Map);
                builder.Append(OpenAngleBracket);
                builder.Append(property.ToType.ToString());
                builder.Append(CloseAngleBracket);
                builder.Append(OpenBracket);
                builder.Append(CloseBracket);
            }
            if (counter < map.ConstructorProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseBracket);
        builder.Append(NewLine);
        AppendTabs(ref builder, 2); ;
        builder.Append(OpenCurlyBracket);
        counter = 1;
        foreach (var property in map.InitializerProperties)
        {
            builder.Append(NewLine);
            AppendTabs(ref builder, 3);
            builder.Append(property.ToName);
            builder.Append(WhiteSpace);
            builder.Append(Equal);
            builder.Append(WhiteSpace);
            builder.Append(Source);
            builder.Append(Dot);
            builder.Append(property.FromName);
            if (!(property.IsSameTypes || property.HasImplicitConversion))
            {
                builder.Append(Dot);
                builder.Append(Map);
                builder.Append(OpenAngleBracket);
                builder.Append(property.ToType.ToString());
                builder.Append(CloseAngleBracket);
                builder.Append(OpenBracket);
                builder.Append(CloseBracket);
            }
            if (counter < map.InitializerProperties.Count)
            {
                builder.Append(Comma);
            }
            counter++;
        }
        builder.Append(NewLine);
        AppendTabs(ref builder, 2);
        builder.Append(CloseCurlyBracket);
        builder.Append(Semicolon);
    }

    private void AppendTabs(ref ValueStringBuilder builder, int count)
    {
        for (int i = 0; i < count * TabWidth; i++)
        {
            builder.Append(WhiteSpace);
        }
    }
}
//using Benchmark.Utils;
//using NextGenMapper.Utils;
//using System.Runtime.InteropServices;

//namespace Benchmark.Benchmarks.Experiments;

//[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Net60)]
//public class SourceStringBuilding
//{
//    private const int TabWidth = 4;
//    private const string NewLine = "\r\n";
//    private const char OpenBracket = '(';
//    private const char CloseBracket = ')';
//    private const char OpenCurlyBracket = '{';
//    private const char CloseCurlyBracket = '}';
//    private const char OpenAngleBracket = '<';
//    private const char CloseAngleBracket = '>';
//    private const char Equal = '=';
//    private const char Dot = '.';
//    private const char Comma = ',';
//    private const char Semicolon = ';';
//    private const char WhiteSpace = ' ';
//    private const string Lambda = "=>";
//    private const string Internal = "internal";
//    private const string Static = "static";
//    private const string Source = "source";
//    private const string Map = "Map";
//    private const string To = "To";
//    private const string This = "this";
//    private const string New = "new";

//    private ClassMap map;
//    private Compilation compilation;
//    private Member[] constructorProperties;
//    private Member[] initializerProperties;
//    private string from;
//    private string to;

//    [GlobalSetup]
//    public void Setup()
//    {
//        var source = @"
//public namespace Test;
//public class Source
//{
//    public int Property1 { get; set; }
//    public int Property2 { get; set; }
//    public int Property3 { get; set; }
//    public int Property4 { get; set; }
//    public int Property5 { get; set; }
//    public int Property6 { get; set; }
//    public int Property7 { get; set; }
//    public int Property8 { get; set; }
//    public int Property9 { get; set; }
//    public int Property10 { get; set; }
//}
//public class Destination
//{
//    public int Property1 { get; set; }
//    public int Property2 { get; set; }
//    public int Property3 { get; set; }
//    public int Property4 { get; set; }
//    public int Property5 { get; set; }
//    public int Property6 { get; set; }
//    public int Property7 { get; set; }
//    public int Property8 { get; set; }
//    public int Property9 { get; set; }
//    public int Property10 { get; set; }
//}";
//        compilation = CompilationHelper.CreateCompilation(new[] { source }, "bench");
//        var sourceType= compilation.GetTypeByMetadataName("Test.Source");
//        var destinationType = compilation.GetTypeByMetadataName("Test.Destination");
//        var sourceProeprties = sourceType.GetMembers().OfType<IPropertySymbol>();
//        var destinationProperties = destinationType.GetMembers().OfType<IPropertySymbol>();
//        var propertiesMaps = sourceProeprties.Zip(destinationProperties).Select(x => MemberMap.Initializer(x.First, x.Second));
//        map = new ClassMap(sourceType, destinationType, propertiesMaps, Location.None);
//        constructorProperties = Array.Empty<Member>();
//        initializerProperties = propertiesMaps
//            .Select(x => new Member
//            {
//                FromName = x.FromName,
//                ToName = x.ToName,
//                FromType = x.FromType.ToString(),
//                ToType = x.ToType.ToString(),
//                IsSameTypes = x.IsSameTypes,
//                HasImplicitConversion = compilation.HasImplicitConversion(x.FromType, x.ToType)
//            })
//            .ToArray();
//        from = map.FromType.ToDisplayString(NullableFlowState.None);
//        to = map.ToType.ToDisplayString(NullableFlowState.None);
//    }

//    //[Benchmark]
//    public string StringInterpolation()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to}
//(
//{string.Join(",\r\n", map.ConstructorProperties.Select(x => $"    source.{x.FromName}{(!(x.IsSameTypes || compilation.HasImplicitConversion(x.FromType, x.ToType)) ? $".Map<{x.ToType}>()" : string.Empty)}"))}
//)
//{{
//{string.Join(",\r\n", map.InitializerProperties.Select(x => $"    {x.ToName} = source.{x.FromName}{(!(x.IsSameTypes || compilation.HasImplicitConversion(x.FromType, x.ToType)) ? $".Map<{x.ToType}>()" : string.Empty)}"))}
//}};";
//    }

//    //[Benchmark]
//    public string ValueStringBuilder()
//    {
//        var builder = new ValueStringBuilder();

//        AppendTabs(ref builder, 2);
//        builder.Append(Internal);
//        builder.Append(WhiteSpace);
//        builder.Append(Static);
//        builder.Append(WhiteSpace);
//        builder.Append(to);
//        builder.Append(WhiteSpace);
//        builder.Append(Map);
//        builder.Append(OpenAngleBracket);
//        builder.Append(To);
//        builder.Append(CloseAngleBracket);
//        builder.Append(OpenBracket);
//        builder.Append(This);
//        builder.Append(WhiteSpace);
//        builder.Append(from);
//        builder.Append(WhiteSpace);
//        builder.Append(Source);
//        builder.Append(CloseBracket);
//        builder.Append(WhiteSpace);
//        builder.Append(Lambda);
//        builder.Append(WhiteSpace);
//        builder.Append(New);
//        builder.Append(WhiteSpace);
//        builder.Append(to);
//        builder.Append(NewLine);
//        AppendTabs(ref builder, 2);
//        builder.Append(OpenBracket);
//        var counter = 1;
//        foreach (var property in map.ConstructorProperties)
//        {
//            builder.Append(NewLine);
//            AppendTabs(ref builder, 3);
//            builder.Append(Source);
//            builder.Append(Dot);
//            builder.Append(property.FromName);
//            if (!(property.IsSameTypes || compilation.HasImplicitConversion(property.FromType, property.ToType)))
//            {
//                builder.Append(Dot);
//                builder.Append(Map);
//                builder.Append(OpenAngleBracket);
//                builder.Append(property.ToType.ToString());
//                builder.Append(CloseAngleBracket);
//                builder.Append(OpenBracket);
//                builder.Append(CloseBracket);
//            }
//            if (counter < map.ConstructorProperties.Count)
//            {
//                builder.Append(Comma);
//            }
//            counter++;
//        }
//        builder.Append(NewLine);
//        AppendTabs(ref builder, 2);
//        builder.Append(CloseBracket);
//        builder.Append(NewLine);
//        AppendTabs(ref builder, 2); ;
//        builder.Append(OpenCurlyBracket);
//        counter = 1;
//        foreach (var property in map.InitializerProperties)
//        {
//            builder.Append(NewLine);
//            AppendTabs(ref builder, 3);
//            builder.Append(property.ToName);
//            builder.Append(WhiteSpace);
//            builder.Append(Equal);
//            builder.Append(WhiteSpace);
//            builder.Append(Source);
//            builder.Append(Dot);
//            builder.Append(property.FromName);
//            if (!(property.IsSameTypes || compilation.HasImplicitConversion(property.FromType, property.ToType)))
//            {
//                builder.Append(Dot);
//                builder.Append(Map);
//                builder.Append(OpenAngleBracket);
//                builder.Append(property.ToType.ToString());
//                builder.Append(CloseAngleBracket);
//                builder.Append(OpenBracket);
//                builder.Append(CloseBracket);
//            }
//            if (counter < map.InitializerProperties.Count)
//            {
//                builder.Append(Comma);
//            }
//            counter++;
//        }
//        builder.Append(NewLine);
//        AppendTabs(ref builder, 2);
//        builder.Append(CloseCurlyBracket);
//        builder.Append(Semicolon);

//        return builder.ToString();
//    }

//    private void AppendTabs(ref ValueStringBuilder builder, int count)
//    {
//        for (int i = 0; i < count * TabWidth; i++)
//        {
//            builder.Append(WhiteSpace);
//        }
//    }

//    //[Benchmark]
//    public string StringInterpolationWithValueStringBuilder()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to}
//(
//{BuildConstructor(map.ConstructorProperties)}
//)
//{{
//{BuildConstructor(map.InitializerProperties)}
//}};";
//    }

//    //[Benchmark]
//    public string StringInterpolationWithValueStringBuilderV2()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to}
//(
//{BuildConstructorV2(map.ConstructorProperties)}
//)
//{{
//{BuildConstructorV2(map.InitializerProperties)}
//}};";
//    }

//    //[Benchmark]
//    public string StringInterpolationWithValueStringBuilderV3()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to}
//(
//{BuildConstructorV3(map.ConstructorProperties)}
//)
//{{
//{BuildConstructorV3(map.InitializerProperties)}
//}};";
//    }

//    public ReadOnlySpan<char> BuildConstructor(List<MemberMap> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var counter = 1;
//        foreach (var property in properties)
//        {
//            AppendTabs(ref builder, 3);
//            builder.Append(Source);
//            builder.Append(Dot);
//            builder.Append(property.FromName);
//            if (!(property.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/))
//            {
//                builder.Append(Dot);
//                builder.Append(Map);
//                builder.Append(OpenAngleBracket);
//                builder.Append(property.ToType.ToString());
//                builder.Append(CloseAngleBracket);
//                builder.Append(OpenBracket);
//                builder.Append(CloseBracket);
//            }
//            if (counter < map.ConstructorProperties.Count)
//            {
//                builder.Append(Comma);
//                builder.Append(NewLine);
//            }
//            counter++;
//        }

//        return builder.AsSpan();
//    }

//    public ReadOnlySpan<char> BuildInitializer(List<MemberMap> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var counter = 1;
//        foreach (var property in properties)
//        {
//            AppendTabs(ref builder, 3);
//            builder.Append(property.ToName);
//            builder.Append(WhiteSpace);
//            builder.Append(Equal);
//            builder.Append(WhiteSpace);
//            builder.Append(Source);
//            builder.Append(Dot);
//            builder.Append(property.FromName);
//            if (!(property.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/))
//            {
//                builder.Append(Dot);
//                builder.Append(Map);
//                builder.Append(OpenAngleBracket);
//                builder.Append(property.ToType.ToString());
//                builder.Append(CloseAngleBracket);
//                builder.Append(OpenBracket);
//                builder.Append(CloseBracket);
//            }
//            if (counter < map.InitializerProperties.Count)
//            {
//                builder.Append(Comma);
//                builder.Append(NewLine);
//            }
//            counter++;
//        }

//        return builder.AsSpan();
//    }

//    public ReadOnlySpan<char> BuildConstructorV2(List<MemberMap> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var counter = 1;
//        foreach (var property in properties)
//        {
//            builder.Append($"            source.{property.FromName}{(!(property.IsSameTypes || compilation.HasImplicitConversion(property.FromType, property.ToType)) ? $".Map<{property.ToType}>()" : string.Empty)}{(counter < map.ConstructorProperties.Count ? ",\r\n" : string.Empty)}");
//            counter++;
//        }

//        return builder.AsSpan();
//    }

//    public ReadOnlySpan<char> BuildInitializerV2(List<MemberMap> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var counter = 1;
//        foreach (var property in properties)
//        {
//            builder.Append($"            {property.ToName} = source.{property.FromName}{(!(property.IsSameTypes || compilation.HasImplicitConversion(property.FromType, property.ToType)) ? $".Map<{property.ToType}>()" : string.Empty)}{(counter < map.ConstructorProperties.Count ? ",\r\n" : string.Empty)}");
//            counter++;
//        }

//        return builder.AsSpan();
//    }

//    public ReadOnlySpan<char> BuildConstructorV3(List<MemberMap> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var counter = 1;
//        foreach (var property in properties)
//        {
//            builder.Append($"            source.{property.FromName}");
//            if (!(property.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/))
//            {
//                builder.Append($".Map<{property.ToType}>()");
//            }
//            if (counter < map.ConstructorProperties.Count)
//            {
//                builder.Append(",\r\n");
//            }
//            counter++;
//        }

//        return builder.AsSpan();
//    }

//    public ReadOnlySpan<char> BuildInitializerV3(List<MemberMap> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var counter = 1;

//        foreach (var property in properties)
//        {
//            builder.Append($"            {property.ToName} = source.{property.FromName}");
//            if (!(property.IsSameTypes/* || compilation.HasImplicitConversion(property.FromType, property.ToType)*/))
//            {
//                builder.Append($".Map<{property.ToType}>()");
//            }
//            if (counter < map.ConstructorProperties.Count)
//            {
//                builder.Append(",\r\n");
//            }
//            counter++;
//        }

//        return builder.AsSpan();
//    }

//    public readonly struct Member
//    {
//        public string FromName { get; init; }
//        public string ToName { get; init; }
//        public string FromType { get; init; }
//        public string ToType { get; init; }
//        public bool IsSameTypes { get; init; }
//        public bool HasImplicitConversion { get; init; }
//    }

//    public ReadOnlySpan<char> BuildConstructorV4(Span<Member> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var lastIndex = properties.Length - 1;
//        for (var i = 0; i < properties.Length; i++)
//        {
//            builder.Append($"            source.{properties[i].FromName}");
//            if (!properties[i].IsSameTypes && !properties[i].HasImplicitConversion)
//            {
//                builder.Append($".Map<{properties[i].ToType}>()");
//            }
//            if (i < lastIndex)
//            {
//                builder.Append(",\r\n");
//            }
//        }

//        return builder.AsSpan();
//    }

//    public ReadOnlySpan<char> BuildInitializerV4(Span<Member> properties)
//    {
//        var builder = new ValueStringBuilder();
//        var lastIndex = properties.Length - 1;
//        for (var i = 0; i < properties.Length; i++)
//        {
//            builder.Append($"            {properties[i].ToName} = source.{properties[i].FromName}");
//            if (!properties[i].IsSameTypes && !properties[i].HasImplicitConversion)
//            {
//                builder.Append($".Map<{properties[i].ToType}>()");
//            }
//            if (i < lastIndex)
//            {
//                builder.Append(",\r\n");
//            }
//        }

//        return builder.AsSpan();
//    }

//    //[Benchmark]
//    public string StringInterpolationWithValueStringBuilderV4()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to}
//(
//{BuildConstructorV4(constructorProperties)}
//)
//{{
//{BuildConstructorV4(initializerProperties)}
//}};";
//    }

//    public ref struct InitializerBuilderV5
//    {
//        private readonly ReadOnlySpan<char> _s1 = "            ";
//        private readonly ReadOnlySpan<char> _s2 = " = source.";
//        private readonly ReadOnlySpan<char> _s3 = ".Map<";
//        private readonly ReadOnlySpan<char> _s4 = ">()";
//        private readonly ReadOnlySpan<char> _s5 = ",\r\n";

//        public InitializerBuilderV5()
//        {

//        }

//        public ReadOnlySpan<char> Build(Span<Member> properties)
//        {
//            var builder = new ValueStringBuilder();
//            var lastIndex = properties.Length - 1;
//            for (var i = 0; i < properties.Length; i++)
//            {
//                builder.Append(_s1);
//                builder.Append(properties[i].ToName);
//                builder.Append(_s2);
//                builder.Append(properties[i].FromName);
//                if (!properties[i].IsSameTypes && !properties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s3);
//                    builder.Append(properties[i].ToType);
//                    builder.Append(_s4);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s5);
//                }
//            }

//            return builder.AsSpan();
//        }
//    }

//    public ref struct ConstructorBuilderV5
//    {
//        private readonly ReadOnlySpan<char> _s1 = "            source.";
//        private readonly ReadOnlySpan<char> _s2 = ".Map<";
//        private readonly ReadOnlySpan<char> _s3 = ">()";
//        private readonly ReadOnlySpan<char> _s4 = ",\r\n";

//        public ConstructorBuilderV5()
//        {

//        }

//        public ReadOnlySpan<char> Build(Span<Member> properties)
//        {
//            var builder = new ValueStringBuilder();
//            var lastIndex = properties.Length - 1;
//            for (var i = 0; i < properties.Length; i++)
//            {
//                builder.Append(_s1);
//                builder.Append(properties[i].FromName);
//                if (!properties[i].IsSameTypes && !properties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s2);
//                    builder.Append(properties[i].ToType);
//                    builder.Append(_s3);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s4);
//                }
//            }

//            return builder.AsSpan();
//        }
//    }

//    //[Benchmark]
//    public string StringInterpolationWithValueStringBuilderV5()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to}
//(
//{new ConstructorBuilderV5().Build(constructorProperties)}
//)
//{{
//{new ConstructorBuilderV5().Build(initializerProperties)}
//}};";
//    }

//    public ref struct SourceBuilderV6
//    {
//        private readonly ReadOnlySpan<char> _s1 = "internal static ";
//        private readonly ReadOnlySpan<char> _s2 = " Map<To>(this ";
//        private readonly ReadOnlySpan<char> _s3 = " source) => new ";
//        private readonly ReadOnlySpan<char> _s3s = "\r\n(\r\n";

//        private readonly ReadOnlySpan<char> _s4 = "            source.";
//        private readonly ReadOnlySpan<char> _s5 = ".Map<";
//        private readonly ReadOnlySpan<char> _s6 = ">()";
//        private readonly ReadOnlySpan<char> _s7 = ",\r\n";

//        private readonly ReadOnlySpan<char> _s8 = "\r\n)\r\n{\r\n";

//        private readonly ReadOnlySpan<char> _s9 = "            ";
//        private readonly ReadOnlySpan<char> _s10 = " = source.";

//        private readonly ReadOnlySpan<char> _s11 = "\r\n};";


//        public SourceBuilderV6()
//        {

//        }

//        public string Build(string from, string to, Span<Member> constructorProperties, Span<Member> initializerProperties)
//        {
//            var builder = new ValueStringBuilder();

//            builder.Append(_s1);
//            builder.Append(to);
//            builder.Append(_s2);
//            builder.Append(from);
//            builder.Append(_s3);
//            builder.Append(to);
//            builder.Append(_s3s);

//            var lastIndex = constructorProperties.Length - 1;
//            for (var i = 0; i < constructorProperties.Length; i++)
//            {
//                builder.Append(_s4);
//                builder.Append(constructorProperties[i].FromName);
//                if (!constructorProperties[i].IsSameTypes && !constructorProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(constructorProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s8);

//            lastIndex = initializerProperties.Length - 1;
//            for (var i = 0; i < initializerProperties.Length; i++)
//            {
//                builder.Append(_s9);
//                builder.Append(initializerProperties[i].ToName);
//                builder.Append(_s10);
//                builder.Append(initializerProperties[i].FromName);
//                if (!initializerProperties[i].IsSameTypes && !initializerProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(initializerProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s11);

//            return builder.ToString();
//        }
//    }

//    //[Benchmark]
//    public string RefStructBuilderV6()
//    {
//        return new SourceBuilderV6().Build(from, to, constructorProperties, initializerProperties);
//    }

//    public ref struct SourceBuilderV7
//    {
//        private readonly ReadOnlySpan<char> _s1 = "internal static ";
//        private readonly ReadOnlySpan<char> _s2 = " Map<To>(this ";
//        private readonly ReadOnlySpan<char> _s3 = " source) => new ";
//        private readonly ReadOnlySpan<char> _s3s = "\r\n(\r\n";

//        private readonly ReadOnlySpan<char> _s4 = "            source.";
//        private readonly ReadOnlySpan<char> _s5 = ".Map<";
//        private readonly ReadOnlySpan<char> _s6 = ">()";
//        private readonly ReadOnlySpan<char> _s7 = ",\r\n";

//        private readonly ReadOnlySpan<char> _s8 = "\r\n)\r\n{\r\n";

//        private readonly ReadOnlySpan<char> _s9 = "            ";
//        private readonly ReadOnlySpan<char> _s10 = " = source.";

//        private readonly ReadOnlySpan<char> _s11 = "\r\n};";


//        public SourceBuilderV7()
//        {

//        }

//        public string Build(string from, string to, Span<Member> constructorProperties, Span<Member> initializerProperties, Span<char> initial)
//        {
//            var builder = new ValueStringBuilder(initial);

//            builder.Append(_s1);
//            builder.Append(to);
//            builder.Append(_s2);
//            builder.Append(from);
//            builder.Append(_s3);
//            builder.Append(to);
//            builder.Append(_s3s);

//            var lastIndex = constructorProperties.Length - 1;
//            for (var i = 0; i < constructorProperties.Length; i++)
//            {
//                builder.Append(_s4);
//                builder.Append(constructorProperties[i].FromName);
//                if (!constructorProperties[i].IsSameTypes && !constructorProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(constructorProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s8);

//            lastIndex = initializerProperties.Length - 1;
//            for (var i = 0; i < initializerProperties.Length; i++)
//            {
//                builder.Append(_s9);
//                builder.Append(initializerProperties[i].ToName);
//                builder.Append(_s10);
//                builder.Append(initializerProperties[i].FromName);
//                if (!initializerProperties[i].IsSameTypes && !initializerProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(initializerProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s11);

//            return builder.ToString();
//        }
//    }

//    //[Benchmark]
//    public string RefStructBuilderV7()
//    {
//        var initialBuffer = new char[535]; 
//        return new SourceBuilderV7().Build(from, to, constructorProperties, initializerProperties, initialBuffer);
//    }

//    public ref struct SourceBuilderV8
//    {
//        private readonly ReadOnlySpan<char> _s1 = "internal static ";
//        private readonly ReadOnlySpan<char> _s2 = " Map<To>(this ";
//        private readonly ReadOnlySpan<char> _s3 = " source) => new ";
//        private readonly ReadOnlySpan<char> _s3s = "\r\n(\r\n";

//        private readonly ReadOnlySpan<char> _s4 = "            source.";
//        private readonly ReadOnlySpan<char> _s5 = ".Map<";
//        private readonly ReadOnlySpan<char> _s6 = ">()";
//        private readonly ReadOnlySpan<char> _s7 = ",\r\n";

//        private readonly ReadOnlySpan<char> _s8 = "\r\n)\r\n{\r\n";

//        private readonly ReadOnlySpan<char> _s9 = "            ";
//        private readonly ReadOnlySpan<char> _s10 = " = source.";

//        private readonly ReadOnlySpan<char> _s11 = "\r\n};";


//        public SourceBuilderV8()
//        {

//        }

//        public string Build(string from, string to, Span<Member> constructorProperties, Span<Member> initializerProperties, int initCapacity)
//        {
//            var builder = new ValueStringBuilder(initCapacity);

//            builder.Append(_s1);
//            builder.Append(to);
//            builder.Append(_s2);
//            builder.Append(from);
//            builder.Append(_s3);
//            builder.Append(to);
//            builder.Append(_s3s);

//            var lastIndex = constructorProperties.Length - 1;
//            for (var i = 0; i < constructorProperties.Length; i++)
//            {
//                builder.Append(_s4);
//                builder.Append(constructorProperties[i].FromName);
//                if (!constructorProperties[i].IsSameTypes && !constructorProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(constructorProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s8);

//            lastIndex = initializerProperties.Length - 1;
//            for (var i = 0; i < initializerProperties.Length; i++)
//            {
//                builder.Append(_s9);
//                builder.Append(initializerProperties[i].ToName);
//                builder.Append(_s10);
//                builder.Append(initializerProperties[i].FromName);
//                if (!initializerProperties[i].IsSameTypes && !initializerProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(initializerProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s11);
            
//            return builder.ToString();
//        }
//    }

//    //[Benchmark]
//    public string RefStructBuilderV8()
//    {
//        return new SourceBuilderV8().Build(from, to, constructorProperties, initializerProperties, 535);
//    }

//    public ref struct SourceBuilderExp
//    {
//        private readonly ReadOnlySpan<char> _s1 = "internal static ";
//        private readonly ReadOnlySpan<char> _s2 = " Map<To>(this ";
//        private readonly ReadOnlySpan<char> _s3 = " source) => new ";
//        private readonly ReadOnlySpan<char> _s3s = "\r\n(\r\n";

//        private readonly ReadOnlySpan<char> _s4 = "            source.";
//        private readonly ReadOnlySpan<char> _s5 = ".Map<";
//        private readonly ReadOnlySpan<char> _s6 = ">()";
//        private readonly ReadOnlySpan<char> _s7 = ",\r\n";

//        private readonly ReadOnlySpan<char> _s8 = "\r\n)\r\n{\r\n";

//        private readonly ReadOnlySpan<char> _s9 = "            ";
//        private readonly ReadOnlySpan<char> _s10 = " = source.";

//        private readonly ReadOnlySpan<char> _s11 = "\r\n};";


//        public SourceBuilderExp()
//        {

//        }

//        public void Build(string from, string to, Span<Member> constructorProperties, Span<Member> initializerProperties)
//        {
//            var builder = new ValueStringBuilder();

//            builder.Append(_s1);
//            builder.Append(to);
//            builder.Append(_s2);
//            builder.Append(from);
//            builder.Append(_s3);
//            builder.Append(to);
//            builder.Append(_s3s);

//            var lastIndex = constructorProperties.Length - 1;
//            for (var i = 0; i < constructorProperties.Length; i++)
//            {
//                builder.Append(_s4);
//                builder.Append(constructorProperties[i].FromName);
//                if (!constructorProperties[i].IsSameTypes && !constructorProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(constructorProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s8);

//            lastIndex = initializerProperties.Length - 1;
//            for (var i = 0; i < initializerProperties.Length; i++)
//            {
//                builder.Append(_s9);
//                builder.Append(initializerProperties[i].ToName);
//                builder.Append(_s10);
//                builder.Append(initializerProperties[i].FromName);
//                if (!initializerProperties[i].IsSameTypes && !initializerProperties[i].HasImplicitConversion)
//                {
//                    builder.Append(_s5);
//                    builder.Append(initializerProperties[i].ToType);
//                    builder.Append(_s6);
//                }
//                if (i < lastIndex)
//                {
//                    builder.Append(_s7);
//                }
//            }

//            builder.Append(_s11);
//        }
//    }

//    //[Benchmark]
//    public void RefStructBuilderExp()
//    {
//        new SourceBuilderExp().Build(from, to, constructorProperties, initializerProperties);
//    }

//    public ref struct SourceBuilderV9
//    {
//        public SourceBuilderV9()
//        {
            
//        }

//        public string Build(string from, string to, Span<Member> constructorProperties, Span<Member> initializerProperties)
//        {
//            Span<char> result = stackalloc char[535];
//            var pos = 0;
//            ReadOnlySpan<char> s1 = stackalloc char[] { 'i', 'n', 't', 'e', 'r', 'n', 'a', 'l', ' ', 's', 't', 'a', 't', 'i', 'c', ' ' };
//            ReadOnlySpan<char> s2 = stackalloc char[] { ' ', 'M', 'a', 'p', '<', 'T', 'o', '>', '(', 't', 'h', 'i', 's', ' ' };
//            ReadOnlySpan<char> s3 = stackalloc char[] { ' ', 's', 'o', 'u' , 'r', 'c', 'e', ')', ' ', '=', '>', ' ', 'n', 'e', 'w', ' ' };
//            ReadOnlySpan<char> s3s = stackalloc char[] { '\r', '\n', ')', '\r', '\n' };
//            ReadOnlySpan<char> s4 = stackalloc char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 's', 'o', 'u', 'r', 'c', 'e', '.' };
//            ReadOnlySpan<char> s5 = stackalloc char[] { '.', 'M', 'a', 'p', '<' };
//            ReadOnlySpan<char> s6 = stackalloc char[] { '>', '(', ')' };
//            ReadOnlySpan<char> s7 = stackalloc char[] { ',', '\r', '\n' };
//            ReadOnlySpan<char> s8 = stackalloc char[] { '\r', '\n', ')', '\r', '\n', '{', '\r', '\n' };
//            ReadOnlySpan<char> s9 = stackalloc char[] { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' };
//            ReadOnlySpan<char> s10 = stackalloc char[] { ' ', '=', ' ', 's', 'o', 'u', 'r', 'c', 'e', '.' };
//            ReadOnlySpan<char> s11 = stackalloc char[] { '\r', '\n', '}', ';' };

//            s1.CopyTo(result[pos..]);
//            pos += s1.Length;
//            to.CopyTo(result[pos..]);
//            pos += to.Length;
//            s2.CopyTo(result[pos..]);
//            pos += s2.Length;
//            from.CopyTo(result[pos..]);
//            pos += from.Length;
//            s3.CopyTo(result[pos..]);
//            pos += s3.Length;
//            to.CopyTo(result[pos..]);
//            pos += to.Length;
//            s3s.CopyTo(result[pos..]);
//            pos += s3s.Length;

//            var lastIndex = constructorProperties.Length - 1;
//            for (var i = 0; i < constructorProperties.Length; i++)
//            {
//                s4.CopyTo(result[pos..]);
//                pos += s4.Length;
//                constructorProperties[i].FromName.CopyTo(result[pos..]);
//                pos += constructorProperties[i].FromName.Length;
//                if (!constructorProperties[i].IsSameTypes && !constructorProperties[i].HasImplicitConversion)
//                {
//                    s5.CopyTo(result[pos..]);
//                    pos += s5.Length;
//                    constructorProperties[i].ToType.CopyTo(result[pos..]);
//                    pos += constructorProperties[i].ToType.Length;
//                    s6.CopyTo(result[pos..]);
//                    pos += s6.Length;
//                }
//                if (i < lastIndex)
//                {
//                    s7.CopyTo(result[pos..]);
//                    pos += s7.Length;
//                }
//            }

//            s8.CopyTo(result[pos..]);
//            pos += s8.Length;

//            lastIndex = initializerProperties.Length - 1;
//            for (var i = 0; i < initializerProperties.Length; i++)
//            {
//                s9.CopyTo(result[pos..]);
//                pos += s9.Length;
//                initializerProperties[i].ToName.CopyTo(result[pos..]);
//                pos += initializerProperties[i].ToName.Length;
//                s10.CopyTo(result[pos..]);
//                pos += s10.Length;
//                initializerProperties[i].FromName.CopyTo(result[pos..]);
//                pos += initializerProperties[i].FromName.Length;
//                if (!initializerProperties[i].IsSameTypes && !initializerProperties[i].HasImplicitConversion)
//                {
//                    s5.CopyTo(result[pos..]);
//                    pos += s5.Length;
//                    initializerProperties[i].ToType.CopyTo(result[pos..]);
//                    pos += initializerProperties[i].ToType.Length;
//                    s6.CopyTo(result[pos..]);
//                    pos += s6.Length;
//                }
//                if (i < lastIndex)
//                {
//                    s7.CopyTo(result[pos..]);
//                    pos += s7.Length;
//                }
//            }

//            s11.CopyTo(result[pos..]);
//            pos += s11.Length;

//            return result.Slice(0, pos).ToString();
//        }
//    }

//    //[Benchmark]
//    public string RefStructBuilderV9()
//    {
//        return new SourceBuilderV9().Build(from, to, constructorProperties, initializerProperties);
//    }

//    //[Benchmark]
//    public string StringInterpolationWithValueStringBuilderV10()
//    {
//        return $@"
//internal static {to} Map<To>(this {from} source) => new {to} 
//{ConstructorArgumentsV10(constructorProperties)}
//{InitializerAssigmentsV10(initializerProperties)};";
//    }

//    public ReadOnlySpan<char> ConstructorArgumentsV10(Span<Member> members)
//    {
//        if (members.Length == 0)
//        {
//            return ReadOnlySpan<char>.Empty;
//        }

//        var builder = new ValueStringBuilder();
//        var lastIndex = members.Length - 1;
//        builder.Append("(\r\n");
//        for (var i = 0; i < members.Length; i++)
//        {
//            AppendTabs(ref builder, 3);
//            ConstructorArgumentV10(members[i], ref builder);
//            if (i < lastIndex)
//            {
//                builder.Append(",\r\n");
//            }
//        }
//        builder.Append("\r\n)");

//        return builder.AsSpan();
//    }

//    public void ConstructorArgumentV10(Member member, ref ValueStringBuilder builder)
//    {
//        if (member.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/)
//        {
//            builder.Append($"source.{member.FromName}");
//        }
//        else
//        {
//            builder.Append($"source.{member.FromName}.Map<{member.ToType}>()");
//        }
//    }

//    public ReadOnlySpan<char> InitializerAssigmentsV10(Span<Member> members)
//    {
//        if (members.Length == 0)
//        {
//            return ReadOnlySpan<char>.Empty;
//        }

//        var builder = new ValueStringBuilder();
//        var lastIndex = members.Length - 1;
//        builder.Append("{\r\n");
//        for (var i = 0; i < members.Length; i++)
//        {
//            AppendTabs(ref builder, 3);
//            ConstructorArgumentV10(members[i], ref builder);
//            if (i < lastIndex)
//            {
//                builder.Append(",\r\n");
//            }
//        }
//        builder.Append("\r\n}");

//        return builder.AsSpan();
//    }

//    public void InitializerAssigmentV10(Member member, ref ValueStringBuilder builder)
//    {
//        if (member.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/)
//        {
//            builder.Append($"{member.ToName} = source.{member.FromName}");
//        }
//        else
//        {
//            builder.Append($"{member.ToName} = source.{member.FromName}.Map<{member.ToType}>()");
//        }
//    }

//    [Benchmark]
//    public ReadOnlySpan<char> StringInterpolationWithValueStringBuilderV11()
//    {
//        var builder = new ValueStringBuilder();

//        builder.Append($"internal static {to} Map<To>(this {from} source) => new {to}\r\n");
//        ConstructorArgumentsV11(constructorProperties, ref builder);
//        builder.Append("\r\n");
//        InitializerAssigmentsV11(initializerProperties, ref builder);
//        builder.Append(';');

//        return builder.AsSpan();
//    }

//    public void ConstructorArgumentsV11(Span<Member> members, ref ValueStringBuilder builder)
//    {
//        if (members.Length == 0)
//        {
//            return;
//        }

//        var lastIndex = members.Length - 1;
//        builder.Append("(\r\n");
//        for (var i = 0; i < members.Length; i++)
//        {
//            AppendTabs(ref builder, 3);
//            ConstructorArgumentV11(members[i], ref builder);
//            if (i < lastIndex)
//            {
//                builder.Append(",\r\n");
//            }
//        }
//        builder.Append("\r\n)");
//    }

//    public void ConstructorArgumentV11(Member member, ref ValueStringBuilder builder)
//    {
//        if (member.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/)
//        {
//            builder.Append($"source.{member.FromName}");
//        }
//        else
//        {
//            builder.Append($"source.{member.FromName}.Map<{member.ToType}>()");
//        }
//    }

//    public void InitializerAssigmentsV11(Span<Member> members, ref ValueStringBuilder builder)
//    {
//        if (members.Length == 0)
//        {
//            return;
//        }

//        var lastIndex = members.Length - 1;
//        builder.Append("{\r\n");
//        for (var i = 0; i < members.Length; i++)
//        {
//            AppendTabs(ref builder, 3);
//            ConstructorArgumentV10(members[i], ref builder);
//            if (i < lastIndex)
//            {
//                builder.Append(",\r\n");
//            }
//        }
//        builder.Append("\r\n}");
//    }

//    public void InitializerAssigmentV11(Member member, ref ValueStringBuilder builder)
//    {
//        if (member.IsSameTypes /*|| compilation.HasImplicitConversion(property.FromType, property.ToType)*/)
//        {
//            builder.Append($"{member.ToName} = source.{member.FromName}");
//        }
//        else
//        {
//            builder.Append($"{member.ToName} = source.{member.FromName}.Map<{member.ToType}>()");
//        }
//    }

//    //[Benchmark]
//    //public string MapMethodBuilderStaticClass()
//    //{
//    //    var builder = new ValueStringBuilder();

//    //    var toConstructor = CollectionsMarshal.AsSpan(map.ConstructorProperties);
//    //    var toInitializer = CollectionsMarshal.AsSpan(map.InitializerProperties);

//    //    MapMethodBuilder.AppendMapMethod(ref builder, from, to, toConstructor, toInitializer, compilation);

//    //    return builder.ToString();
//    //}

//    [Benchmark]
//    public string MapMethodBuilderRefStruct()
//    {
//        var builder = new MapMethodBuilderStruct(from, to, map, compilation);

//        return builder.GenerateMapMethod();
//    }

//    public ref struct MapMethodBuilderStruct
//    {
//        private readonly string _from;
//        private readonly string _to;
//        private readonly Span<MemberMap> _constructorArguments;
//        private readonly Span<MemberMap> _initializerAssigments;
//        private ValueStringBuilder _builder;
//        private readonly Compilation _compilation;

//        public ReadOnlySpan<char> AsSpan() => _builder.AsSpan();

//        public MapMethodBuilderStruct(string from, string to, ClassMap map, Compilation compilation)
//        {
//            _from = from;
//            _to = to;
//            _constructorArguments = CollectionsMarshal.AsSpan(map.ConstructorProperties);
//            _initializerAssigments = CollectionsMarshal.AsSpan(map.InitializerProperties);
//            _compilation = compilation;
//            _builder = new ValueStringBuilder();
//        }

//        public string GenerateMapMethod()
//        {
//            MethodDifinition();
//            ConstructorInvocation();

//            return _builder.ToString();
//        }

//        public void MethodDifinition()
//        {
//            _builder.Append($"internal static {_to} Map<To>(this {_from} source)");
//        }

//        public void ConstructorInvocation()
//        {
//            _builder.Append($" => new {_to}\r\n");
//            ConstructorArguments();
//            _builder.Append("\r\n");
//            InitializerAssigments();
//            _builder.Append(';');
//        }

//        public void ConstructorArgument(MemberMap member)
//        {
//            if (member.IsSameTypes || _compilation.HasImplicitConversion(member.FromType, member.ToType))
//            {
//                _builder.Append($"source.{member.FromName}");
//            }
//            else
//            {
//                _builder.Append($"source.{member.FromName}.Map<{member.ToType}>()");
//            }
//        }

//        public void InitializerAssigment(MemberMap member)
//        {
//            if (member.IsSameTypes || _compilation.HasImplicitConversion(member.FromType, member.ToType))
//            {
//                _builder.Append($"{member.ToName} = source.{member.FromName}");
//            }
//            else
//            {
//                _builder.Append($"{member.ToName} = source.{member.FromName}.Map<{member.ToType}>()");
//            }
//        }

//        public void ConstructorArguments()
//        {
//            if (_constructorArguments.Length == 0)
//            {
//                return;
//            }

//            var lastIndex = _constructorArguments.Length - 1;
//            _builder.Append("(\r\n");
//            for (var i = 0; i < _constructorArguments.Length; i++)
//            {
//                _builder.Append("            ");
//                ConstructorArgument(_constructorArguments[i]);
//                if (i < lastIndex)
//                {
//                    _builder.Append(",\r\n");
//                }
//            }
//            _builder.Append("\r\n)");
//        }

//        public void InitializerAssigments()
//        {
//            if (_initializerAssigments.Length == 0)
//            {
//                return;
//            }

//            var lastIndex = _initializerAssigments.Length - 1;
//            _builder.Append("{\r\n");
//            for (var i = 0; i < _initializerAssigments.Length; i++)
//            {
//                _builder.Append("            ");
//                InitializerAssigment(_initializerAssigments[i]);
//                if (i < lastIndex)
//                {
//                    _builder.Append(",\r\n");
//                }
//            }
//            _builder.Append("\r\n}");
//        }

//    }
//}
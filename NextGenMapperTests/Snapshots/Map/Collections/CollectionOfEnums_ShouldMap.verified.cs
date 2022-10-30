//HintName: Mapper.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.Destination Map<To>(this Test.Source source) => source switch
        {
            Test.Source.A => Test.Destination.A,
            Test.Source.B => Test.Destination.B,
            Test.Source.C => Test.Destination.C,
            _ => throw new System.ArgumentOutOfRangeException("Error when mapping Test.Source to Test.Destination")
        };

        internal static System.Collections.Generic.List<Test.Destination> Map<To>(this Test.Source[] source)
        {
            var destination = new System.Collections.Generic.List<Test.Destination>(source.Length);
            var sourceSpan = new System.Span<Test.Source>(source);
            for (var i = 0; i < source.Length; i++)
            {
                destination.Add(sourceSpan[i].Map<Test.Destination>());
            }

            return destination;
        }

    }
}
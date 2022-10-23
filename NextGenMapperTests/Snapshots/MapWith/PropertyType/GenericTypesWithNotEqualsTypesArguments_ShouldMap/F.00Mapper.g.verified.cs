//HintName: Mapper.g.cs
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.ClassB Map<To>(this Test.ClassA source) => new Test.ClassB
        (
        )
        {
            Property = source.Property
        };

        internal static Test.Destination<Test.ClassB> MapWith<To>
        (
            this Test.Source<Test.ClassA> source,
            int forMapWith
        )
        => new Test.Destination<Test.ClassB>
        (
        )
        {
            Property = source.Property.Map<Test.ClassB>(),
            ForMapWith = forMapWith
        };

        internal static Test.Destination<Test.ClassB> MapWith<To>
        (
            this Test.Source<Test.ClassA> source,
            Test.ClassB property = default,
            int forMapWith = default
        )
        {
            throw new System.NotImplementedException("This method is a stub and is not intended to be called");
        }

    }
}
﻿//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static Test.ClassB Map<To>(this Test.ClassA source) => new Test.ClassB()
        {
            Property = source.Property
        };

        internal static Test.Destination<Test.ClassB> Map<To>(this Test.Source<Test.ClassA> source) => new Test.Destination<Test.ClassB>()
        {
            Property = source.Property.Map<Test.ClassB>()
        };
    }
}
﻿//HintName: Mapper_ClassMaps.g.cs
#nullable enable
using NextGenMapper.Extensions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static TypesFromDllTest.ClassWithConstructorFromDll Map<To>(this Test.Source source) => new TypesFromDllTest.ClassWithConstructorFromDll
        (
            source.PropertyA,
            source.PropertyB,
            source.PropertyC
        );
    }
}
﻿//HintName: Mapper_ConfiguredProjectionMaps_MockMethods.g.cs
#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            int Property1 = default!,
            int Property2 = default!,
            int ForMapWith1 = default!,
            int ForMapWith2 = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }

        internal static IQueryable<Test.Destination> ProjectWith<To>
        (
            this IQueryable<Test.Source> source,
            Expression<Func<Test.Source, int>> Property1 = default!,
            Expression<Func<Test.Source, int>> Property2 = default!,
            Expression<Func<Test.Source, int>> ForMapWith1 = default!,
            Expression<Func<Test.Source, int>> ForMapWith2 = default!
        )
        {
            throw new System.NotImplementedException("This method is a mock and is not intended to be called");
        }
    }
}
﻿//HintName: Mapper_ConfiguredProjectionMaps.g.cs
#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static IQueryable<Destination> ProjectWith<To>
        (
            this IQueryable<Source> source,
            int Property
        )
            => source.Select(x => new Destination
        {
            Property = Property
        });
    }
}
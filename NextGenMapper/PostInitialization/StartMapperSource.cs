namespace NextGenMapper.PostInitialization
{
    public static class StartMapperSource
    {
        public const string MapMethodName = "Map";
        public const string ConfiguredMapMethodName = "MapWith";
        public const string MapMethodFullName = "NextGenMapper.Mapper.Map<To>(object)";
        public const string MapWithMethodFullName = "NextGenMapper.Mapper.MapWith<To>(object)";

        public const string ProjectionMethodName = "Project";
        public const string ConfiguredProjectionMethodName = "ProjectWith";
        public const string ProjectionMethodFullName = "NextGenMapper.Mapper.Project<To>(System.Linq.IQueryable<object>)";
        public const string ConfiguredProjectionMethodFullName = "NextGenMapper.Mapper.ProjectWith<To>(System.Linq.IQueryable<object>)";

        public const string NonGenericIQueryableProjectionMethodFullName = "NextGenMapper.Mapper.Project<To>(System.Linq.IQueryable)";
        public const string NonGenericIQueryableConfiguredProjectionMethodFullName = "NextGenMapper.Mapper.ProjectWith<To>(System.Linq.IQueryable)";

        public const string StartMapper =
@"using System;
using System.Linq;

namespace NextGenMapper
{
    internal static partial class Mapper
    {
        internal static To Map<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");

        internal static To MapWith<To>(this object source) => throw new InvalidOperationException($""Error when mapping {source.GetType()} to {typeof(To)}, mapping function was not found. Create custom mapping function."");
    
        internal static To Project<To>(this IQueryable<object> source) => throw new InvalidOperationException($""Error when project {source.GetType()} to {typeof(To)}, project function was not found."");
        
        internal static To ProjectWith<To>(this IQueryable<object> source) => throw new InvalidOperationException($""Error when project {source.GetType()} to {typeof(To)}, project function was not found."");
        
        internal static To Project<To>(this IQueryable source) => throw new InvalidOperationException($""Error when project {source.GetType()} to {typeof(To)}, projection for non generic IQueryable is not supported"");

        internal static To ProjectWith<To>(this IQueryable source) => throw new InvalidOperationException($""Error when project {source.GetType()} to {typeof(To)}, projection for non generic IQueryable is not supported"");
    }
}";
    }
}

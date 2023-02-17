using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Mapping.Comparers;
using NextGenMapper.Mapping.Designers;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.PostInitialization;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper;

public class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        PostInitialization(context);

        var configuredMapsTargets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsConfiguredMapMethodInvocationSynaxNode(node),
            static (context, _) => TargetFinder.GetConfiguredMapTarget(context.Node, context.SemanticModel));

        var configuredMapsTargetsDiagnostics = configuredMapsTargets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);
        context.ReportDiagnostics(configuredMapsTargetsDiagnostics);

        var filteredConfiguredMapsTargets = configuredMapsTargets
            .Where(static x => x.Type is TargetType.ConfiguredMap)
            .Select(static (x, _) => x.ConfiguredMapTarget);

        var configuredMapWithoutArgumentsDiagnostics = filteredConfiguredMapsTargets
            .Where(static x => !x.IsCompleteMethod)
            .Select(static (x, _) => Diagnostics.MapWithMethodWithoutArgumentsError(x.Location));
        context.ReportDiagnostics(configuredMapWithoutArgumentsDiagnostics);

        var NotNamedArgumentsDiagnostics = filteredConfiguredMapsTargets
            .Where(static x => x.Arguments.Any(x => !x.IsNamedArgument()))
            .Select(static (x, _) => Diagnostics.MapWithArgumentMustBeNamed(x.Location));
        context.ReportDiagnostics(NotNamedArgumentsDiagnostics);

        var configuredMapsAndRelated = filteredConfiguredMapsTargets
            .SelectMany(static (x, _) => ConfiguredMapDesigner.DesignConfiguredMaps(x));

        var configuredMapsDiagnostics = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.Error)
            .Select(static (x, _) => x.ErrorMap.Diagnostic);
        context.ReportDiagnostics(configuredMapsDiagnostics);

        var configuredMaps = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.ConfiguredMap)
            .Select(static (x, _) => x.ConfiguredMap)
            .Collect()
            .Distinct(new ConfiguredMapComparer());

        context.RegisterSourceOutput(configuredMaps, (sourceProductionContext, maps) =>
        {
            var sourceBuilder = new ConfiguredMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ConfiguredMaps.g", mapperClassSource);
        });

        var configuredMapsMockMethods = configuredMaps
            .Select((x, _) =>
            {
                var mockMethodsHashSet = new HashSet<ConfiguredMapMockMethod>(new ConfiguredMapMockMethodComparer());
                var mockMethodsMaxCount = x.Sum(y => y.MockMethods.Length);
                var mockMethodsCount = 0;
                Span<ConfiguredMapMockMethod> mockMethods = new ConfiguredMapMockMethod[mockMethodsMaxCount];
                foreach (var map in x.AsSpan())
                {
                    mockMethodsHashSet.Add(new ConfiguredMapMockMethod(map.Source, map.Destination, map.UserArguments));
                    foreach (var mockMethod in map.MockMethods.AsSpan())
                    {
                        if (!mockMethodsHashSet.Contains(mockMethod))
                        {
                            mockMethodsHashSet.Add(mockMethod);
                            mockMethods[mockMethodsCount] = mockMethod;
                            mockMethodsCount++;
                        }
                    }
                }

                return Unsafe.CastSpanToImmutableArray(mockMethods.Slice(0, mockMethodsCount));
            });

        context.RegisterSourceOutput(configuredMapsMockMethods, (sourceProductionContext, mockMethods) =>
        {
            var sourceBuilder = new ConfiguredMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(mockMethods);

            sourceProductionContext.AddSource("Mapper_ConfiguredMaps_MockMethods.g", mapperClassSource);
        });

        var userMapsTargets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsUserMapMethodDeclarationSyntaxNode(node),
            static (context, _) => TargetFinder.GetUserMapTarget(context.Node, context.SemanticModel));

        var userMapsTargetsDiagnostics = userMapsTargets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);
        context.ReportDiagnostics(userMapsTargetsDiagnostics);

        var userMapsHashSet = userMapsTargets
            .Where(static x => x.Type is TargetType.UserMap)
            .Select(static (x, _) => UserMapDesigner.DesingUserMaps(x.UserMapTarget))
            .Collect()
            .Select(static (x, _) => new HashSet<UserMap>(x));

        var mapsTargets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsMapMethodInvocationSyntaxNode(node),
            static (context, _) => TargetFinder.GetMapTarget(context.Node, context.SemanticModel));

        var mapsTargetsDiagnostics = mapsTargets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);
        context.ReportDiagnostics(userMapsTargetsDiagnostics);

        var maps = mapsTargets
            .Where(static x => x.Type is TargetType.Map)
            .SelectMany(static (x, _) => MapDesigner.DesignMaps(x.MapTarget));

        var mapsDiagnostics = maps
            .Where(static x => x.Type is MapType.Error)
            .Select(static (x, _) => x.ErrorMap.Diagnostic);
        context.ReportDiagnostics(mapsDiagnostics);

        var relatedClassMaps = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.ClassMap)
            .Select(static (x, _) => x.ClassMap);

        var classMaps = maps
            .Where(static x => x.Type is MapType.ClassMap)
            .Select(static (x, _) => x.ClassMap)
            .Concat(relatedClassMaps)
            .Distinct(EqualityComparer<ClassMap>.Default)
            .RemoveUserMaps(userMapsHashSet);

        context.RegisterSourceOutput(classMaps, (sourceProductionContext, maps) =>
        {
            var sourceBuilder = new ClassMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ClassMaps.g", mapperClassSource);
        });

        var relatedCollectionMaps = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.CollectionMap)
            .Select(static (x, _) => x.CollectionMap);

        var collectionMaps = maps
            .Where(static x => x.Type is MapType.CollectionMap)
            .Select(static (x, _) => x.CollectionMap)
            .Concat(relatedCollectionMaps)
            .Distinct(EqualityComparer<CollectionMap>.Default)
            .RemoveUserMaps(userMapsHashSet);

        context.RegisterSourceOutput(collectionMaps, (sourceProductionContext, maps) =>
        {
            var sourceBuilder = new CollectionMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_CollectionMaps.g", mapperClassSource);
        });

        var relatedEnumMaps = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.EnumMap)
            .Select(static (x, _) => x.EnumMap);

        var enumMaps = maps
            .Where(static x => x.Type is MapType.EnumMap)
            .Select(static (x, _) => x.EnumMap)
            .Concat(relatedEnumMaps)
            .Distinct(EqualityComparer<EnumMap>.Default)
            .RemoveUserMaps(userMapsHashSet);

        context.RegisterSourceOutput(enumMaps, (sourceProductionContext, maps) =>
        {
            var sourceBuilder = new EnumMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_EnumMaps.g", mapperClassSource);
        });

        var mapsPostValidationDiagnostics = classMaps
            .Combine(collectionMaps)
            .Combine(enumMaps)
            .Combine(configuredMaps)
            .Combine(userMapsHashSet)
            .SelectMany(static (x, _) =>
            {
                var ((((classMaps, collectionMaps), enumMaps), configuredMaps), userMaps) = x;
                var mapsHashSet = new HashSet<IMap>();
                var diagnostics = new List<Diagnostic>();

                foreach (var map in classMaps.AsSpan())
                {
                    if (!mapsHashSet.Contains(map))
                    {
                        mapsHashSet.Add(map);
                    }
                }
                foreach (var map in collectionMaps.AsSpan())
                {
                    if (!mapsHashSet.Contains(map))
                    {
                        mapsHashSet.Add(map);
                    }
                }
                foreach (var map in enumMaps.AsSpan())
                {
                    if (!mapsHashSet.Contains(map))
                    {
                        mapsHashSet.Add(map);
                    }
                }
                foreach (var map in configuredMaps.AsSpan())
                {
                    if (!mapsHashSet.Contains(map))
                    {
                        mapsHashSet.Add(map);
                    }
                }
                foreach (var map in userMaps)
                {
                    if (!mapsHashSet.Contains(map))
                    {
                        mapsHashSet.Add(map);
                    }
                }

                void ValidatePropertyMap(IMap map, PropertyMap propertyMap)
                {
                    if (!propertyMap.IsTypesEquals
                        && !propertyMap.HasImplicitConversion
                        && !mapsHashSet.Contains(propertyMap))
                    {
                        var diagnostic = Diagnostics.MappingFunctionForPropertiesNotFound(
                            Location.None,
                            map.Source,
                            propertyMap.SourceName,
                            propertyMap.SourceType,
                            map.Destination,
                            propertyMap.DestinationName,
                            propertyMap.DestinationType);
                        diagnostics.Add(diagnostic);
                    }
                }

                foreach (var map in classMaps.AsSpan())
                {
                    foreach(var propertyMap in map.ConstructorProperties)
                    {
                        ValidatePropertyMap(map, propertyMap);
                    }
                    foreach (var propertyMap in map.InitializerProperties)
                    {
                        ValidatePropertyMap(map, propertyMap);
                    }
                }

                foreach(var map in collectionMaps)
                {
                    if (map.IsItemsEquals || map.IsItemsHasImpicitConversion)
                    {
                        continue;
                    }

                    var collectionItemsMap = (IMap)new UserMap(map.SourceItem, map.DestinationItem);
                    if (!mapsHashSet.Contains(collectionItemsMap))
                    {
                        var diagnostic = Diagnostics.MappingFunctionNotFound(Location.None, map.Source, map.Destination);
                        diagnostics.Add(diagnostic);
                    }
                }

                foreach (var map in configuredMaps.AsSpan())
                {
                    foreach (var propertyMap in map.ConstructorProperties)
                    {
                        ValidatePropertyMap(map, propertyMap);
                    }
                    foreach (var propertyMap in map.InitializerProperties)
                    {
                        ValidatePropertyMap(map, propertyMap);
                    }
                }

                var diagnosticsArray = diagnostics.ToArray();
                return Unsafe.CastArrayToImmutableArray(ref diagnosticsArray);
            });
        context.ReportDiagnostics(mapsPostValidationDiagnostics);
    }

    private void PostInitialization(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MapperExtensions.g",
            SourceText.From(ExtensionsSource.Source, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "StartMapper.g",
            SourceText.From(StartMapperSource.StartMapper, Encoding.UTF8)));
    }
}

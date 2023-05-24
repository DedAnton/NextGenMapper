using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Comparers;
using NextGenMapper.Mapping.Designers;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.PostInitialization;
using NextGenMapper.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper;

[Generator(LanguageNames.CSharp)]
public class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        PostInitialization(context);

        var configuredMapsTargets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsConfiguredMapMethodInvocationSynaxNode(node),
            static (context, ct) => TargetFinder.GetConfiguredMapTarget(context.Node, context.SemanticModel, ct));

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
            .Where(static x => !x.Arguments.IsAllArgumentsNamed())
            .Select(static (x, _) => Diagnostics.MapWithArgumentMustBeNamed(x.Location));
        context.ReportDiagnostics(NotNamedArgumentsDiagnostics);

        var configuredMapsAndRelated = filteredConfiguredMapsTargets
            .SelectMany(static (x, ct) => ConfiguredMapDesigner.DesignConfiguredMaps(x, ct));

        var configuredMapsDiagnostics = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.Error)
            .Select(static (x, _) => x.ErrorMap.Diagnostic);
        context.ReportDiagnostics(configuredMapsDiagnostics);

        var configuredMaps = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.ConfiguredMap)
            .Select(static (x, _) => x.ConfiguredMap)
            .Collect();

        var duplicateConfiguredMapDiagnostics = configuredMaps
            .SelectMany(static (x, _) =>
            {
                var mapsHashSet = new HashSet<ConfiguredMap>(new ConfiguredMapComparer());
                var diagnostics = new ValueListBuilder<Diagnostic>();
                foreach (var map in x.AsSpan())
                {
                    if (!map.IsSuccess)
                    {
                        continue;
                    }
                    if (mapsHashSet.Contains(map))
                    {
                        var conflictedMap = mapsHashSet.First(x => x.Equals(map));
                        if (conflictedMap.EqualsWithArgumentsNames(map))
                        {
                            continue;
                        }

                        var diagnostic = Diagnostics.DuplicateMapWithFunction(map.Location, map.Source, map.Destination);

                        diagnostics.Append(diagnostic);
                    }
                    {
                        mapsHashSet.Add(map);
                    }
                }

                return diagnostics.ToImmutableArray();
            });
        context.ReportDiagnostics(duplicateConfiguredMapDiagnostics);

        var uniqueConfiguredMaps = configuredMaps.Distinct(new ConfiguredMapComparer());

        context.RegisterSourceOutput(uniqueConfiguredMaps, (sourceProductionContext, maps) =>
        {
            if (maps.Length == 0)
            {
                return;
            }

            var sourceBuilder = new ConfiguredMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ConfiguredMaps.g.cs", mapperClassSource);
        });

        var configuredMapsMockMethods = uniqueConfiguredMaps
            .Select((x, _) =>
            {
                var mockMethodsHashSet = new HashSet<ConfiguredMapMockMethod>(new ConfiguredMapMockMethodComparer());
                var mockMethodsMaxCount = x.Sum(y => y.MockMethods.Length);
                var mockMethods = new ValueListBuilder<ConfiguredMapMockMethod>(mockMethodsMaxCount);
                foreach (var map in x.AsSpan())
                {
                    mockMethodsHashSet.Add(new ConfiguredMapMockMethod(map.Source, map.Destination, map.UserArguments));
                    foreach (var mockMethod in map.MockMethods.AsSpan())
                    {
                        if (!mockMethodsHashSet.Contains(mockMethod))
                        {
                            mockMethodsHashSet.Add(mockMethod);
                            mockMethods.Append(mockMethod);
                        }
                    }
                }

                return mockMethods.ToImmutableArray();
            });

        context.RegisterSourceOutput(configuredMapsMockMethods, (sourceProductionContext, mockMethods) =>
        {
            if (mockMethods.Length == 0)
            {
                return;
            }

            var sourceBuilder = new ConfiguredMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(mockMethods);

            sourceProductionContext.AddSource("Mapper_ConfiguredMaps_MockMethods.g.cs", mapperClassSource);
        });

        var userMapsTargets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsUserMapMethodDeclarationSyntaxNode(node),
            static (context, ct) => TargetFinder.GetUserMapTarget(context.Node, context.SemanticModel, ct))
            .SelectMany(static (x, _) => x);

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
            static (context, ct) => TargetFinder.GetMapTarget(context.Node, context.SemanticModel, ct));

        var mapsTargetsDiagnostics = mapsTargets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);
        context.ReportDiagnostics(mapsTargetsDiagnostics);

        var maps = mapsTargets
            .Where(static x => x.Type is TargetType.Map)
            .SelectMany(static (x, ct) => MapDesigner.DesignMaps(x.MapTarget, ct));

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
            if (maps.Length == 0)
            {
                return;
            }

            var sourceBuilder = new ClassMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ClassMaps.g.cs", mapperClassSource);
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
            if (maps.Length == 0)
            {
                return;
            }
            var sourceBuilder = new CollectionMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_CollectionMaps.g.cs", mapperClassSource);
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
            if (maps.Length == 0)
            {
                return;
            }
            var sourceBuilder = new EnumMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_EnumMaps.g.cs", mapperClassSource);
        });

        var potentialErrorsFromRelated = configuredMapsAndRelated
            .Where(static x => x.Type is MapType.PotentialError)
            .Select(static (x, _) => x.PotentialErrorMap);
        var potentialErrors = maps
            .Where(static x => x.Type is MapType.PotentialError)
            .Select(static (x, _) => x.PotentialErrorMap)
            .Concat(potentialErrorsFromRelated);

        var mapsPostValidationDiagnostics = classMaps
            .Combine(collectionMaps)
            .Combine(enumMaps)
            .Combine(uniqueConfiguredMaps)
            .Combine(userMapsHashSet)
            .Combine(potentialErrors)
            .SelectMany(static (x, ct) =>
            {
                var (((((classMaps, collectionMaps), enumMaps), configuredMaps), userMaps), potentialErrors) = x;
                var mapsHashSet = new HashSet<IMap>(new SimpleMapComparer());
                var diagnostics = new ValueListBuilder<Diagnostic>();

                foreach (var map in classMaps.AsSpan())
                {
                    mapsHashSet.Add(map);
                }
                foreach (var map in collectionMaps.AsSpan())
                {
                    mapsHashSet.Add(map);
                }
                foreach (var map in enumMaps.AsSpan())
                {
                    mapsHashSet.Add(map);
                }
                foreach (var map in configuredMaps.AsSpan())
                {
                    mapsHashSet.Add(map);
                }
                foreach (var map in userMaps)
                {
                    mapsHashSet.Add(map);
                }

                void ValidatePropertyMap(IMap map, PropertyMap propertyMap, Location location, ref ValueListBuilder<Diagnostic> diagnostics)
                {
                    if (!propertyMap.IsTypesEquals
                        && !propertyMap.HasImplicitConversion
                        && !mapsHashSet.Contains(propertyMap))
                    {
                        var diagnostic = Diagnostics.MappingFunctionForPropertiesNotFound(
                            location,
                            map.Source,
                            propertyMap.SourceName,
                            propertyMap.SourceType,
                            map.Destination,
                            propertyMap.DestinationName,
                            propertyMap.DestinationType);
                        diagnostics.Append(diagnostic);
                    }
                }

                ct.ThrowIfCancellationRequested();
                foreach (var map in classMaps.AsSpan())
                {
                    foreach (var propertyMap in map.ConstructorProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, map.Location, ref diagnostics);
                    }
                    foreach (var propertyMap in map.InitializerProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, map.Location, ref diagnostics);
                    }
                }

                foreach (var map in collectionMaps)
                {
                    if (map.IsItemsEquals || map.IsItemsHasImpicitConversion)
                    {
                        continue;
                    }

                    var collectionItemsMap = (IMap)new UserMap(map.SourceItem, map.DestinationItem);
                    if (!mapsHashSet.Contains(collectionItemsMap))
                    {
                        var diagnostic = Diagnostics.MappingFunctionNotFound(map.Location, map.SourceItem, map.DestinationItem);
                        diagnostics.Append(diagnostic);
                    }
                }

                foreach (var map in configuredMaps.AsSpan())
                {
                    foreach (var propertyMap in map.ConstructorProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, map.Location, ref diagnostics);
                    }
                    foreach (var propertyMap in map.InitializerProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, map.Location, ref diagnostics);
                    }
                }

                foreach (var potentialError in potentialErrors.AsSpan())
                {
                    if (!mapsHashSet.Contains(potentialError))
                    {
                        diagnostics.Append(potentialError.Diagnostic);
                    }
                }

                return diagnostics.ToImmutableArray();
            });
        context.ReportDiagnostics(mapsPostValidationDiagnostics);

        BuildProjectionPipeline(context);
        BuildConfiguredProjectionPipeline(context);
    }

    private void BuildProjectionPipeline(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsProjectionMethodInvocationSyntaxNode(node),
            static (context, ct) => TargetFinder.GetProjectionTarget(context.Node, context.SemanticModel, ct));

        var targetsDiagnostics = targets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);

        context.ReportDiagnostics(targetsDiagnostics);

        var maps = targets
            .Where(static x => x.Type is TargetType.Projection)
            .Select(static (x, ct) => ProjectionMapDesigner.DesingProjectionMap(x.ProjectionTarget, ct));

        var mapsDiagnostics = maps
            .Where(static x => x.Type is MapType.Error)
            .Select(static (x, _) => x.ErrorMap.Diagnostic);
        context.ReportDiagnostics(mapsDiagnostics);

        var projectionMaps = maps
            .Where(static x => x.Type is MapType.ProjectionMap)
            .Select(static (x, _) => x.ProjectionMap)
            .Collect()
            .Distinct(EqualityComparer<ProjectionMap>.Default);

        context.RegisterSourceOutput(projectionMaps, (sourceProductionContext, maps) =>
        {
            if (maps.Length == 0)
            {
                return;
            }
            var sourceBuilder = new ProjectionMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ProjectionMaps.g.cs", mapperClassSource);
        });
    }

    private void BuildConfiguredProjectionPipeline(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsConfiguredProjectionMethodInvocationSyntaxNode(node),
            static (context, ct) => TargetFinder.GetConfiguredProjectionTarget(context.Node, context.SemanticModel, ct));

        var targetsDiagnostics = targets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);
        context.ReportDiagnostics(targetsDiagnostics);

        var filteredTargets = targets
            .Where(static x => x.Type is TargetType.ConfiguredProjection)
            .Select(static (x, _) => x.ConfiguredProjectionTarget);

        var configuredMapWithoutArgumentsDiagnostics = filteredTargets
            .Where(static x => !x.IsCompleteMethod)
            .Select(static (x, _) => Diagnostics.ProjectWithMethodWithoutArgumentsError(x.Location));
        context.ReportDiagnostics(configuredMapWithoutArgumentsDiagnostics);

        var NotNamedArgumentsDiagnostics = filteredTargets
            .Where(static x => !x.Arguments.IsAllArgumentsNamed())
            .Select(static (x, _) => Diagnostics.ProjectWithArgumentMustBeNamed(x.Location));
        context.ReportDiagnostics(NotNamedArgumentsDiagnostics);

        var maps = filteredTargets
            .SelectMany(static (x, ct) =>ConfiguredProjectionMapDesigner.DesingConfiguredProjectionMap(x, ct));

        var mapsDiagnostics = maps
            .Where(static x => x.Type is MapType.Error)
            .Select(static (x, _) => x.ErrorMap.Diagnostic);
        context.ReportDiagnostics(mapsDiagnostics);

        var projectionMaps = maps
            .Where(static x => x.Type is MapType.ConfiguredProjection)
            .Select(static (x, _) => x.ConfiguredProjectionMap)
            .Collect();

        var duplicateConfiguredProjectionMapDiagnostics = projectionMaps
            .SelectMany(static (x, _) =>
            {
                var mapsHashSet = new HashSet<ConfiguredProjectionMap>(new ConfiguredProjectionMapComparer());
                var diagnostics = new ValueListBuilder<Diagnostic>();
                foreach (var map in x.AsSpan())
                {
                    if (!map.IsSuccess)
                    {
                        continue;
                    }
                    if (mapsHashSet.Contains(map))
                    {
                        var conflictedMap = mapsHashSet.First(x => x.Equals(map));
                        if (conflictedMap.EqualsWithArgumentsNames(map))
                        {
                            continue;
                        }

                        var diagnostic = Diagnostics.DuplicateProjectWithFunction(map.Location, map.Source, map.Destination);

                        diagnostics.Append(diagnostic);
                    }
                    {
                        mapsHashSet.Add(map);
                    }
                }

                return diagnostics.ToImmutableArray();
            });
        context.ReportDiagnostics(duplicateConfiguredProjectionMapDiagnostics);

        var uniqueConfiguredProjectionMaps = projectionMaps.Distinct(new ConfiguredProjectionMapComparer());

        context.RegisterSourceOutput(uniqueConfiguredProjectionMaps, (sourceProductionContext, maps) =>
        {
            if (maps.Length == 0)
            {
                return;
            }
            var sourceBuilder = new ConfiguredProjectionMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ConfiguredProjectionMaps.g.cs", mapperClassSource);
        });

        var mockMethods = projectionMaps
            .Select((x, _) =>
            {
                var mockMethodsHashSet = new HashSet<ConfiguredMapMockMethod>(new ConfiguredMapMockMethodComparer());
                var mockMethods = new ValueListBuilder<ConfiguredMapMockMethod>(x.Length);
                foreach (var map in x.AsSpan())
                {
                    mockMethodsHashSet.Add(new ConfiguredMapMockMethod(map.Source, map.Destination, map.UserArguments));
                    if (map.MockMethod is { } mockMethod && !mockMethodsHashSet.Contains(mockMethod))
                    {
                        mockMethodsHashSet.Add(mockMethod);
                        mockMethods.Append(mockMethod);
                    }
                }

                return mockMethods.ToImmutableArray();
            });

        context.RegisterSourceOutput(mockMethods, (sourceProductionContext, mockMethods) =>
        {
            if (mockMethods.Length == 0)
            {
                return;
            }

            var sourceBuilder = new ConfiguredProjectionMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(mockMethods);

            sourceProductionContext.AddSource("Mapper_ConfiguredProjectionMaps_MockMethods.g.cs", mapperClassSource);
        });
    }

    private void PostInitialization(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MapperExtensions.g.cs",
            SourceText.From(ExtensionsSource.Source, Encoding.UTF8)));

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "StartMapper.g.cs",
            SourceText.From(StartMapperSource.StartMapper, Encoding.UTF8)));
    }
}

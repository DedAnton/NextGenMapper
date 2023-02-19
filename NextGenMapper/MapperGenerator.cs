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
using System.Collections.Immutable;
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
                        var diagnostic = Diagnostics.DuplicateMapWithFunction(Location.None, map.Source, map.Destination);

                        diagnostics.Append(diagnostic);
                    }
                    {
                        mapsHashSet.Add(map);
                    }
                }

                return Unsafe.SpanToImmutableArray(diagnostics.AsSpan());
            });
        context.ReportDiagnostics(duplicateConfiguredMapDiagnostics);

        var uniqueConfiguredMaps = configuredMaps.Distinct(new ConfiguredMapComparer());

        context.RegisterSourceOutput(uniqueConfiguredMaps, (sourceProductionContext, maps) =>
        {
            //TODO: refactoring
            maps = maps.Where(x => x.IsSuccess).ToImmutableArray();

            if (maps.Length == 0)
            {
                return;
            }

            var sourceBuilder = new ConfiguredMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_ConfiguredMaps.g", mapperClassSource);
        });

        var configuredMapsMockMethods = uniqueConfiguredMaps
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

                return Unsafe.SpanToImmutableArray(mockMethods.Slice(0, mockMethodsCount));
            });

        context.RegisterSourceOutput(configuredMapsMockMethods, (sourceProductionContext, mockMethods) =>
        {
            if (mockMethods.Length == 0)
            {
                return;
            }

            var sourceBuilder = new ConfiguredMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(mockMethods);

            sourceProductionContext.AddSource("Mapper_ConfiguredMaps_MockMethods.g", mapperClassSource);
        });

        var userMapsTargets = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => SourceCodeAnalyzer.IsUserMapMethodDeclarationSyntaxNode(node),
            static (context, _) => TargetFinder.GetUserMapTarget(context.Node, context.SemanticModel))
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
            static (context, _) => TargetFinder.GetMapTarget(context.Node, context.SemanticModel));

        var mapsTargetsDiagnostics = mapsTargets
            .Where(static x => x.Type is TargetType.Error)
            .Select(static (x, _) => x.ErrorMapTarget.Diagnostic);
        context.ReportDiagnostics(mapsTargetsDiagnostics);

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
            if (maps.Length == 0)
            {
                return;
            }

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
            if (maps.Length == 0)
            {
                return;
            }
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
            if (maps.Length == 0)
            {
                return;
            }
            var sourceBuilder = new EnumMapsSourceBuilder();
            var mapperClassSource = sourceBuilder.BuildMapperClass(maps);

            sourceProductionContext.AddSource("Mapper_EnumMaps.g", mapperClassSource);
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
            .SelectMany(static (x, _) =>
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

                void ValidatePropertyMap(IMap map, PropertyMap propertyMap, ref ValueListBuilder<Diagnostic> diagnostics)
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
                        diagnostics.Append(diagnostic);
                    }
                }

                foreach (var map in classMaps.AsSpan())
                {
                    foreach(var propertyMap in map.ConstructorProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, ref diagnostics);
                    }
                    foreach (var propertyMap in map.InitializerProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, ref diagnostics);
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
                        var diagnostic = Diagnostics.MappingFunctionNotFound(Location.None, map.SourceItem, map.DestinationItem);
                        diagnostics.Append(diagnostic);
                    }
                }

                foreach (var map in configuredMaps.AsSpan())
                {
                    foreach (var propertyMap in map.ConstructorProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, ref diagnostics);
                    }
                    foreach (var propertyMap in map.InitializerProperties)
                    {
                        ValidatePropertyMap(map, propertyMap, ref diagnostics);
                    }
                }

                foreach (var potentialError in potentialErrors.AsSpan())
                {
                    if (!mapsHashSet.Contains(potentialError))
                    {
                        diagnostics.Append(potentialError.Diagnostic);
                    }
                }

                return Unsafe.SpanToImmutableArray(diagnostics.AsSpan());
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

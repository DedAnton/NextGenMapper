using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System.Collections.Immutable;
using System;
using System.Threading;
using NextGenMapper.Errors;

namespace NextGenMapper.Mapping.Designers;

internal static class ProjectionMapDesigner
{
    public static Map DesingProjectionMap(ProjectionTarget target, CancellationToken cancellationToken)
    {
        try
        {
            return DesingProjectionMapInternal(target, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var diagnostic = Diagnostics.MapperInternalError(target.Location, ex);
            return Map.Error(target.Source, target.Destination, diagnostic);
        }
    }

    private static Map DesingProjectionMapInternal(ProjectionTarget target, CancellationToken cancellationToken)
    {
        var (source, destination, location) = target;

        cancellationToken.ThrowIfCancellationRequested();

        var sourceProperties = source.GetPublicReadablePropertiesDictionary();
        if (sourceProperties.Count == 0)
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInSource(location, source, destination);

            return Map.Error(source, destination, diagnostic);
        }

        var (constructor, _, error) = ConstructorFinder.GetPublicDefaultConstructor(destination);
        if (error is MultipleInitializationError multipleInitializationError)
        {
            var diagnostic = Diagnostics.MultipleInitializationError(location, source, destination, multipleInitializationError.ParameterName, multipleInitializationError.InitializedPropertiesString);

            return Map.Error(source, destination, diagnostic);
        }
        if (constructor is null)
        {
            var diagnostic = Diagnostics.DefaultConstructorNotFoundError(location, source, destination);

            return Map.Error(source, destination, diagnostic);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var destinationProperties = destination.GetPublicWritableProperties();
        if (destinationProperties.Length == 0)
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInDestination(location, source, destination);

            return Map.Error(source, destination, diagnostic);
        }

        var initializerProperties = new ValueListBuilder<PropertyMap>(destinationProperties.Length);
        foreach (var destinationProperty in destinationProperties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
            {
                if (SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationProperty.Type))
                {
                    var propertyMap = new PropertyMap(
                    sourceProperty.Name,
                    destinationProperty.Name,
                    sourceProperty.Type.ToNotNullableString(),
                    destinationProperty.Type.ToNotNullableString(),
                    sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    isTypesEquals: true,
                    hasImplicitConversion: true);

                    if (propertyMap.IsSourceNullable && !propertyMap.IsDestinationNullable)
                    {
                        var diagnostic = Diagnostics.PossiblePropertyNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationProperty.Name, destinationProperty.Type);

                        return Map.Error(source, destination, diagnostic);
                    }

                    initializerProperties.Append(propertyMap);
                }
                else
                {
                    var diagnostic = Diagnostics.PropertiesTypesMustBeEquals(location, sourceProperty, destinationProperty);

                    return Map.Error(source, destination, diagnostic);
                }
            }
        }

        if (initializerProperties.Length == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);
   
            return Map.Error(source, destination, diagnostic);
        }

        var projectionMap = Map.Projection(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            initializerProperties.ToImmutableArray());

        return projectionMap;
    }
}

using Microsoft.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    private static void DesignClassesMaps(
        ITypeSymbol source,
        ITypeSymbol destination,
        Location location,
        SemanticModel semanticModel,
        ImmutableList<ITypeSymbol> referencesHistory,
        ref ValueListBuilder<Map> maps, 
        CancellationToken cancellationToken)
    {
        referencesHistory = referencesHistory.Add(source);

        cancellationToken.ThrowIfCancellationRequested();
        var sourceProperties = source.GetPublicReadablePropertiesDictionary();
        if (sourceProperties.Count == 0)
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInSource(location, source, destination);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return;
        }

        var (constructor, assignments) = ConstructorFinder.GetOptimalConstructor(sourceProperties, destination, semanticModel, cancellationToken);
        if (constructor == null)
        {
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var destinationParameters = constructor.Parameters.AsSpan();
        var destinationProperties = destination.GetPublicWritableProperties();
        if (destinationProperties.Length == 0 && destinationParameters.Length == 0)
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInDestination(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return;
        }

        var constructorProperties = new ValueListBuilder<PropertyMap>(destinationParameters.Length);
        var assignmentsDictionary = new Dictionary<string, Assignment>(assignments.Length, StringComparer.InvariantCulture);
        foreach (var assignment in assignments)
        {
            assignmentsDictionary.Add(assignment.Parameter, assignment);
        }
        foreach (var destinationParameter in destinationParameters)
        {
            if (assignmentsDictionary.TryGetValue(destinationParameter.Name, out var assignment)
                && sourceProperties.TryGetValue(assignment.Property, out var sourceProperty))
            {
                var propertyMap = PropertiesMapDesigner.DesignPropertyMap(
                    sourceProperty.Name,
                    sourceProperty.Type,
                    sourceProperty.ContainingType,
                    assignment.Property,
                    destinationParameter.Type,
                    destinationParameter.ContainingType,
                    location,
                    semanticModel,
                    referencesHistory,
                    ref maps,
                    cancellationToken);

                if (propertyMap is null)
                {
                    return;
                }

                constructorProperties.Append(propertyMap.Value);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        var initializerProperties = new ValueListBuilder<PropertyMap>(destinationProperties.Length);
        var destinationPropertiesInitializedByConstructor = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var assignment in assignments)
        {
            destinationPropertiesInitializedByConstructor.Add(assignment.Property);
        }
        foreach (var destinationProperty in destinationProperties)
        {
            if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty)
                && !destinationPropertiesInitializedByConstructor.Contains(destinationProperty.Name))
            {
                var propertyMap = PropertiesMapDesigner.DesignPropertyMap(
                    sourceProperty.Name,
                    sourceProperty.Type,
                    sourceProperty.ContainingType,
                    destinationProperty.Name,
                    destinationProperty.Type,
                    destinationProperty.ContainingType,
                    location,
                    semanticModel,
                    referencesHistory,
                    ref maps,
                    cancellationToken);

                if (propertyMap is null)
                {
                    return;
                }

                initializerProperties.Append(propertyMap.Value);
            }
        }

        if (constructorProperties.Length + initializerProperties.Length == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return;
        }

        var classMap = Map.Class(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            constructorProperties.ToImmutableArray(),
            initializerProperties.ToImmutableArray());

        maps.Append(classMap);
    }
}
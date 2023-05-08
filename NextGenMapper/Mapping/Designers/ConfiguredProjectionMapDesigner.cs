using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static class ConfiguredProjectionMapDesigner
{
    public static Map DesingConfiguredProjectionMap(ConfiguredProjectionTarget target, CancellationToken cancellationToken)
    {
        var (source, destination, arguments, isCompleteMethod, location, semanticModel) = target;

        cancellationToken.ThrowIfCancellationRequested();

        var userArgumentsHashSet = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var argument in target.Arguments)
        {
            var argumentName = argument.NameColon?.Name.Identifier.ValueText;
            if (argumentName is not null)
            {
                userArgumentsHashSet.Add(argumentName);
            }
        }

        var sourceProperties = source.GetPublicReadablePropertiesDictionary();

        var (constructor, _) = ConstructorFinder.GetPublicDefaultConstructor(destination);
        if (constructor is null)
        {
            //TODO: Add special diagnostic
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);

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
        var configuredMapArguments = new ValueListBuilder<NameTypePair>(arguments.Count);
        foreach (var destinationProperty in destinationProperties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destinationPropertyType = destinationProperty.Type.ToNotNullableString();
            if (userArgumentsHashSet.Contains(destinationProperty.Name))
            {
                var propertyMap = CreateUserProvidedProeprtyMap(destinationProperty.Name, destinationPropertyType);
                initializerProperties.Append(propertyMap);

                configuredMapArguments.Append(new NameTypePair(destinationProperty.Name, destinationPropertyType));
            }
            else
            {
                if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
                {
                    if (SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationProperty.Type))
                    {
                        var propertyMap = new PropertyMap(
                        sourceProperty.Name,
                        destinationProperty.Name,
                        sourceProperty.Type.ToNotNullableString(),
                        destinationPropertyType,
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
        }

        if (initializerProperties.Length == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);

            return Map.Error(source, destination, diagnostic);
        }

        if (configuredMapArguments.Length != arguments.Count)
        {
            //TODO: research how to handle
            //throw new Exception($"Property or parameter not found for argument");
            isCompleteMethod = false;
            configuredMapArguments = ValueListBuilder<NameTypePair>.Empty;
        }

        var mockMethod = DesignClassMapMockMethod(
            source,
            destination,
            destinationProperties,
            configuredMapArguments.AsSpan(),
            isCompleteMethod);

        var map = Map.ConfiguredProjection(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            initializerProperties.ToImmutableArray(),
            configuredMapArguments.ToImmutableArray(),
            mockMethod,
            isCompleteMethod);

        return map;
    }

    private static PropertyMap CreateUserProvidedProeprtyMap(string userArgumentName, string userArgumentType)
        => new(
            userArgumentName,
            userArgumentName,
            userArgumentType,
            userArgumentType,
            false,
            false,
            true,
            true,
            userArgumentName);

    private static ConfiguredMapMockMethod? DesignClassMapMockMethod(
        ITypeSymbol source,
        ITypeSymbol destination,
        ReadOnlySpan<IPropertySymbol> destinationProperties,
        ReadOnlySpan<NameTypePair> arguments,
        bool isCompleteMethod)
    {
        var mockMethodParameters = new NameTypePair[destinationProperties.Length];

        for (var i = 0; i < destinationProperties.Length; i++)
        {
            var parameter = new NameTypePair(destinationProperties[i].Name, destinationProperties[i].Type.ToString());
            mockMethodParameters[i] = parameter;
        }

        var mockMethod = new ConfiguredMapMockMethod(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            mockMethodParameters.ToImmutableArray());

        if (isCompleteMethod
            && CompareArgumentsAndParameters(arguments, mockMethod.Parameters.AsSpan()))
        {
            return null;
        }

        return mockMethod;
    }

    private static bool CompareArgumentsAndParameters(ReadOnlySpan<NameTypePair> arguments, ReadOnlySpan<NameTypePair> parameters)
    {
        if (parameters.Length != arguments.Length)
        {
            return false;
        }

        var typesHashSet = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var argument in arguments)
        {
            typesHashSet.Add(argument.Name);
        }
        foreach (var parameter in parameters)
        {
            typesHashSet.Add(parameter.Name);
        }

        return parameters.Length == typesHashSet.Count;
    }

}

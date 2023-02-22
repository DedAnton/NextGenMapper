using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

internal static class ConfiguredMapDesigner
{
    public static ImmutableArray<Map> DesignConfiguredMaps(ConfiguredMapTarget target, CancellationToken cancellationToken)
    {
        var userArgumentsHashSet = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var argument in target.Arguments)
        {
            var argumentName = argument.NameColon?.Name.Identifier.ValueText;
            if (argumentName is not null)
            {
                userArgumentsHashSet.Add(argumentName);
            }
        }

        var maps = new ValueListBuilder<Map>(8);
        DesignConfiguredMaps(target.Source, target.Destination, userArgumentsHashSet, target.IsCompleteMethod, target.Location, target.SemanticModel, ref maps, cancellationToken);

        return maps.ToImmutableArray();
    }

    private static void DesignConfiguredMaps(
        ITypeSymbol source,
        ITypeSymbol destination,
        HashSet<string> arguments,
        bool isCompleteMethod,
        Location location,
        SemanticModel semanticModel,
        ref ValueListBuilder<Map> maps,
        CancellationToken cancellationToken)
    {
        var sourceProperties = source.GetPublicReadablePropertiesDictionary();

        var (constructor, assignments) = 
            ConstructorFinder.GetOptimalConstructor(sourceProperties, destination, arguments, semanticModel, cancellationToken);
        if (constructor == null)
        {
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return;
        }
        var referencesHistory = ImmutableList.Create(source);

        cancellationToken.ThrowIfCancellationRequested();
        var destinationParameters = constructor.Parameters.AsSpan();
        var destinationProperties = destination.GetPublicWritableProperties();

        var constructorProperties = new ValueListBuilder<PropertyMap>(destinationParameters.Length);
        var configuredMapArguments = new ValueListBuilder<NameTypePair>(arguments.Count);
        var assignmentsDictionary = new Dictionary<string, Assignment>(assignments.Length, StringComparer.InvariantCulture);
        foreach (var assignment in assignments)
        {
            assignmentsDictionary.Add(assignment.Parameter, assignment);
        }
        foreach (var destinationParameter in destinationParameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destinationParameterType = destinationParameter.Type.ToNotNullableString();
            if (arguments.Contains(destinationParameter.Name))
            {
                var propertyMap = CreateUserProvidedProeprtyMap(destinationParameter.Name, destinationParameterType);
                constructorProperties.Append(propertyMap);

                configuredMapArguments.Append(new NameTypePair(destinationParameter.Name, destinationParameterType));
            }
            else
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
            cancellationToken.ThrowIfCancellationRequested();
            if (destinationPropertiesInitializedByConstructor.Contains(destinationProperty.Name))
            {
                continue;
            }

            var destinationPropertyType = destinationProperty.Type.ToNotNullableString();
            if (arguments.Contains(destinationProperty.Name))
            {
                var propertyMap = CreateUserProvidedProeprtyMap(destinationProperty.Name, destinationPropertyType);
                initializerProperties.Append(propertyMap);

                configuredMapArguments.Append(new NameTypePair(destinationProperty.Name, destinationPropertyType));
            }
            else
            {
                if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
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
        }

        if (constructorProperties.Length + initializerProperties.Length == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return;
        }

        if (configuredMapArguments.Length != arguments.Count)
        {
            //TODO: research how to handle
            //throw new Exception($"Property or parameter not found for argument");
            isCompleteMethod = false;
            configuredMapArguments = ValueListBuilder<NameTypePair>.Empty;
        }

        var mockMethods = DesignClassMapMockMethods(
            source, 
            destination, 
            destinationProperties,
            configuredMapArguments.AsSpan(), 
            isCompleteMethod, 
            semanticModel,
            cancellationToken);

        var map = Map.Configured(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            constructorProperties.ToImmutableArray(),
            initializerProperties.ToImmutableArray(),
            configuredMapArguments.ToImmutableArray(),
            mockMethods,
            isCompleteMethod);

        maps.Append(map);
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

    private static ImmutableArray<ConfiguredMapMockMethod> DesignClassMapMockMethods(
        ITypeSymbol source,
        ITypeSymbol destination,
        ReadOnlySpan<IPropertySymbol> destinationProperties,
        ReadOnlySpan<NameTypePair> arguments,
        bool isCompleteMethod,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var constructors = destination.GetConstructors();
        var mockMethods = new ValueListBuilder<ConfiguredMapMockMethod>(constructors.Length);
        var isDuplicatedMockRemoved = false;
        foreach(var constructor in constructors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
            {
                var mockMethod = DesignMockMethod(source, destination, constructor, destinationProperties, semanticModel, cancellationToken);

                if (!isDuplicatedMockRemoved
                    && isCompleteMethod
                    && CompareArgumentsAndParameters(arguments, mockMethod.Parameters.AsSpan()))
                {
                    isDuplicatedMockRemoved = true;
                    continue;
                }

                mockMethods.Append(mockMethod);
            }
        }

        return mockMethods.ToImmutableArray();
    }

    private static ConfiguredMapMockMethod DesignMockMethod(
        ITypeSymbol source,
        ITypeSymbol destination,
        IMethodSymbol constructor,
        ReadOnlySpan<IPropertySymbol> destinationProperties,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var destinationParameters = constructor.Parameters.AsSpan();
        var mockMethodParameters = new ValueListBuilder<NameTypePair>(destinationParameters.Length + destinationProperties.Length);
        var assignments = ConstructorFinder.GetAssignments(constructor, semanticModel, cancellationToken);
        var destinationPropertiesInitializedByConstructor = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var assignment in assignments)
        {
            destinationPropertiesInitializedByConstructor.Add(assignment.Property);
        }

        foreach (var destinationParameter in destinationParameters)
        {
            var parameter = new NameTypePair(destinationParameter.Name, destinationParameter.Type.ToString());
            mockMethodParameters.Append(parameter);
        }
        foreach (var destinationProperty in destinationProperties)
        {
            if (!destinationPropertiesInitializedByConstructor.Contains(destinationProperty.Name))
            {
                var parameter = new NameTypePair(destinationProperty.Name, destinationProperty.Type.ToString());
                mockMethodParameters.Append(parameter);
            }
        }

        return new ConfiguredMapMockMethod(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            mockMethodParameters.ToImmutableArray());
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
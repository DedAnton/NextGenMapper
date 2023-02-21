using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Designers;

internal static class ConfiguredMapDesigner
{
    public static ImmutableArray<Map> DesignConfiguredMaps(ConfiguredMapTarget target)
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
        DesignConfiguredMaps(target.Source, target.Destination, userArgumentsHashSet, target.IsCompleteMethod, target.Location, target.SemanticModel, ref maps);

        return Unsafe.SpanToImmutableArray(maps.AsSpan());
    }

    private static void DesignConfiguredMaps(
        ITypeSymbol source,
        ITypeSymbol destination,
        HashSet<string> arguments,
        bool isCompleteMethod,
        Location location,
        SemanticModel semanticModel,
        ref ValueListBuilder<Map> maps)
    {
        var sourceProperties = source.GetPublicReadablePropertiesDictionary();

        var constructorFinder = new ConstructorFinder(semanticModel);
        var (constructor, assigments) = constructorFinder.GetOptimalConstructor(sourceProperties, destination, arguments);
        if (constructor == null)
        {
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return;
        }
        var referencesHistory = ImmutableList.Create(source);

        var destinationParameters = constructor.Parameters.AsSpan();
        var destinationProperties = destination.GetPublicWritableProperties();

        var constructorProperties = new ValueListBuilder<PropertyMap>(destinationParameters.Length);
        var configuredMapArguments = new ValueListBuilder<NameTypePair>(arguments.Count);
        var assigmentsDictionary = new Dictionary<string, Assigment>(assigments.Length, StringComparer.InvariantCulture);
        foreach (var assigment in assigments)
        {
            assigmentsDictionary.Add(assigment.Parameter, assigment);
        }
        foreach (var destinationParameter in destinationParameters)
        {
            var destinationParameterType = destinationParameter.Type.ToNotNullableString();
            if (arguments.Contains(destinationParameter.Name))
            {
                var propertyMap = CreateUserProvidedProeprtyMap(destinationParameter.Name, destinationParameterType);
                constructorProperties.Append(propertyMap);

                configuredMapArguments.Append(new NameTypePair(destinationParameter.Name, destinationParameterType));
            }
            else
            {
                if (assigmentsDictionary.TryGetValue(destinationParameter.Name, out var assigment)
                && sourceProperties.TryGetValue(assigment.Property, out var sourceProperty))
                {
                    var propertyMap = PropertiesMapDesigner.DesignPropertyMap(
                            sourceProperty.Name,
                            sourceProperty.Type,
                            sourceProperty.ContainingType,
                            assigment.Property,
                            destinationParameter.Type,
                            destinationParameter.ContainingType,
                            location,
                            semanticModel,
                            referencesHistory,
                            ref maps);

                    if (propertyMap is null)
                    {
                        return;
                    }

                    constructorProperties.Append(propertyMap.Value);
                }
            }
        }

        var initializerProperties = new ValueListBuilder<PropertyMap>(destinationProperties.Length);
        var destinationPropertiesInitializedByConstructor = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var assigment in assigments)
        {
            destinationPropertiesInitializedByConstructor.Add(assigment.Property);
        }
        foreach (var destinationProperty in destinationProperties)
        {
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
                        ref maps);

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
            semanticModel);

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
        SemanticModel semanticModel)
    {
        var constructorFinder = new ConstructorFinder(semanticModel);
        var constructors = destination.GetConstructors();
        var mockMethods = new ValueListBuilder<ConfiguredMapMockMethod>(constructors.Length);
        var isDuplicatedMockRemoved = false;
        foreach(var constructor in destination.GetConstructors())
        {
            if (constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal)
            {
                var mockMethod = DesignMockMethod(source, destination, constructorFinder, constructor, destinationProperties);

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
        ConstructorFinder constructorFinder, 
        IMethodSymbol constructor,
        ReadOnlySpan<IPropertySymbol> destinationProperties)
    {
        var destinationParameters = constructor.Parameters.AsSpan();
        var mockMethodParameters = new ValueListBuilder<NameTypePair>(destinationParameters.Length + destinationProperties.Length);
        var assigments = constructorFinder.GetAssigments(constructor);
        var destinationPropertiesInitializedByConstructor = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var assigment in assigments)
        {
            destinationPropertiesInitializedByConstructor.Add(assigment.Property);
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
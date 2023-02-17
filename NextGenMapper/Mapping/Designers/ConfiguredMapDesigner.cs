using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
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
    public static Map[] DesignConfiguredMaps(ConfiguredMapTarget target)
        => DesignConfiguredMaps(target.Source, target.Destination, target.Arguments, target.IsCompleteMethod, target.Location, target.SemanticModel).ToArray();

    private static MapsList DesignConfiguredMaps(
        ITypeSymbol source,
        ITypeSymbol destination,
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        bool isCompleteMethod,
        Location location,
        SemanticModel semanticModel)
    {
        var constructorFinder = new ConstructorFinder(semanticModel);
        var (constructor, assigments) = constructorFinder.GetOptimalConstructor(source, destination, new HashSet<string>());
        if (constructor == null)
        {
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);

            return MapsList.Create(Map.PotentialError(source, destination, diagnostic));
        }
        var referencesHistory = ImmutableList.Create(source);

        var destinationParameters = constructor.Parameters.AsSpan();
        var destinationProperties = GetMappableProperties(constructor, assigments);

        var sourcePublicProperties = source.GetPublicPropertiesDictionary();

        var maps = new MapsList();
        var constructorProperties = new PropertyMap[destinationParameters.Length];
        var constructorPropertiesCount = 0;
        var configuredMapArguments = new NameTypePair[arguments.Count];
        var configuredMapArgumentsCount = 0;
        foreach (var destinationParameter in destinationParameters)
        {
            var destinationParameterType = destinationParameter.Type.ToString();
            var isProvidedByUser = arguments.ContainsArgument(destinationParameter.Name);
            if (isProvidedByUser)
            {
                var propertyMap = new PropertyMap(
                    destinationParameter.Name,
                    destinationParameter.Name,
                    destinationParameterType,
                    destinationParameterType,
                    destinationParameter.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    destinationParameter.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    true,
                    true,
                    destinationParameter.Name);

                constructorProperties[constructorPropertiesCount] = propertyMap;
                constructorPropertiesCount++;

                configuredMapArguments[configuredMapArgumentsCount] = new NameTypePair(destinationParameter.Name, destinationParameterType);
                configuredMapArgumentsCount++;
            }
            else
            {
                //TODO: use dictionary for assigments
                foreach (var assigment in assigments)
                {
                    if (assigment.Parameter == destinationParameter.Name
                        && sourcePublicProperties.TryGetValue(assigment.Property, out var sourceProperty))
                    {
                        var propertyMap = new PropertyMap(
                            assigment.Property,
                            assigment.Property,
                            sourceProperty.Type.ToString(),
                            destinationParameter.Type.ToString(),
                            sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                            destinationParameter.Type.NullableAnnotation == NullableAnnotation.Annotated,
                            SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationParameter.Type),
                            SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceProperty.Type, destinationParameter.Type, semanticModel));

                        if (IsPotentialNullReference(propertyMap))
                        {
                            var diagnostic = Diagnostics.PossibleNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationParameter.Name, destinationParameter.Type);

                            return MapsList.Create(Map.Error(source, destination, diagnostic));
                        }

                        constructorProperties[constructorPropertiesCount] = propertyMap;
                        constructorPropertiesCount++;

                        if (!propertyMap.IsTypesEquals && !propertyMap.HasImplicitConversion)
                        {
                            var propertyTypesMaps = MapDesigner.DesignMaps(sourceProperty.Type, destinationParameter.Type, location, semanticModel, referencesHistory);
                            maps.Append(propertyTypesMaps);
                        }
                    }
                }
            }
        }

        var initializerProperties = new PropertyMap[destinationProperties.Length];
        var initializerPropertiesCount = 0;
        foreach (var destinationProperty in destinationProperties)
        {
            var destinationPropertyType = destinationProperty.Type.ToString();
            var isProvidedByUser = arguments.ContainsArgument(destinationProperty.Name);
            if (isProvidedByUser)
            {
                var propertyMap = new PropertyMap(
                    destinationProperty.Name,
                    destinationProperty.Name,
                    destinationPropertyType,
                    destinationPropertyType,
                    destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    true,
                    true);

                initializerProperties[initializerPropertiesCount] = propertyMap;
                initializerPropertiesCount++;

                configuredMapArguments[configuredMapArgumentsCount] = new NameTypePair(destinationProperty.Name, destinationPropertyType);
                configuredMapArgumentsCount++;
            }
            else
            {
                if (sourcePublicProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
                {
                    var propertyMap = new PropertyMap(
                        sourceProperty.Name,
                        destinationProperty.Name,
                        sourceProperty.Type.ToString(),
                        destinationProperty.Type.ToString(),
                        sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationProperty.Type),
                        SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceProperty.Type, destinationProperty.Type, semanticModel));

                    if (IsPotentialNullReference(propertyMap))
                    {
                        var diagnostic = Diagnostics.PossibleNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationProperty.Name, destinationProperty.Type);

                        return MapsList.Create(Map.Error(source, destination, diagnostic));
                    }

                    initializerProperties[initializerPropertiesCount] = propertyMap;
                    initializerPropertiesCount++;

                    if (!propertyMap.IsTypesEquals && !propertyMap.HasImplicitConversion)
                    {
                        var propertyTypesMaps = MapDesigner.DesignMaps(sourceProperty.Type, destinationProperty.Type, location, semanticModel, referencesHistory);
                        maps.Append(propertyTypesMaps);
                    }
                }
            }
        }

        if (constructorPropertiesCount + initializerPropertiesCount == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);

            return MapsList.Create(Map.Error(source, destination, diagnostic));
        }

        if (configuredMapArgumentsCount != arguments.Count)
        {
            //TODO: research how to handle
            throw new Exception($"Property or parameter not found for argument");
        }

        var constructorPropertiesImmutable = constructorProperties.Length == constructorPropertiesCount
            ? Unsafe.CastArrayToImmutableArray(ref constructorProperties)
            : ToImmutableArray(constructorProperties.AsSpan().Slice(0, constructorPropertiesCount));
        var initializerPropertiesImmutable = initializerProperties.Length == initializerPropertiesCount
            ? Unsafe.CastArrayToImmutableArray(ref initializerProperties)
            : ToImmutableArray(initializerProperties.AsSpan().Slice(0, initializerPropertiesCount));

        var mockMethods = DesignClassMapMockMethods(source, destination, configuredMapArguments, isCompleteMethod, semanticModel);

        var map = Map.Configured(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            constructorPropertiesImmutable,
            initializerPropertiesImmutable,
            Unsafe.CastArrayToImmutableArray(ref configuredMapArguments),
            mockMethods);

        maps.AddFirst(map);

        return maps;
    }

    private static bool ContainsArgument(this SeparatedSyntaxList<ArgumentSyntax> arguments, string argumentName)
    {
        foreach (var argument in arguments)
        {
            if (argument.NameColon?.Name.Identifier.ValueText == argumentName)
            {
                return true;
            }
        }

        return false;
    }

    private static ImmutableArray<ConfiguredMapMockMethod> DesignClassMapMockMethods(
        ITypeSymbol source,
        ITypeSymbol destination,
        NameTypePair[] arguments,
        bool isCompleteMethod,
        SemanticModel semanticModel)
    {
        var constructorFinder = new ConstructorFinder(semanticModel);
        var constructors = destination.GetPublicConstructors();
        var mockMethods = new ConfiguredMapMockMethod[isCompleteMethod ? constructors.Length - 1 : constructors.Length];
        var mockMethodsCount = 0;
        var isDuplicatedMockRemoved = false;
        for (int i = 0; i < constructors.Length; i++)
        {
            var assigments = constructorFinder.GetAssigments(constructors[i]);
            var destinationParameters = constructors[i].Parameters.AsSpan();
            var destinationProperties = GetMappableProperties(constructors[i], assigments);
            var mockMethodParameters = new NameTypePair[destinationParameters.Length + destinationProperties.Length];
            for (int j = 0; j < destinationParameters.Length; j++)
            {
                var parameter = new NameTypePair(destinationParameters[j].Name.ToCamelCase(), destinationParameters[j].Type.ToString());
                mockMethodParameters[j] = parameter;
            }
            for (int j = destinationParameters.Length; j < destinationParameters.Length + destinationProperties.Length; j++)
            {
                var parameter = new NameTypePair(destinationProperties[j].Name.ToCamelCase(), destinationProperties[j].Type.ToString());
                mockMethodParameters[j] = parameter;
            }

            if (!isDuplicatedMockRemoved
                && isCompleteMethod
                && CompareArgumentsAndParameters(mockMethodParameters, arguments))
            {
                isDuplicatedMockRemoved = true;
                continue;
            }

            mockMethods[mockMethodsCount] = new ConfiguredMapMockMethod(
                source.ToNotNullableString(),
                destination.ToNotNullableString(),
                Unsafe.CastArrayToImmutableArray(ref mockMethodParameters));
            mockMethodsCount++;
        }

        return Unsafe.CastArrayToImmutableArray(ref mockMethods);
    }

    private static bool CompareArgumentsAndParameters(NameTypePair[] parameters, NameTypePair[] arguments)
    {
        if (parameters.Length != arguments.Length)
        {
            return false;
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Type == arguments[i].Type)
            {
                return false;
            }
        }

        return true;
    }

    //public List<ClassMapWithStub> DesignStubMethodMap(ITypeSymbol from, ITypeSymbol to, Location mapLocation)
    //{
    //    var maps = new List<ClassMapWithStub>();
    //    var constructors = to.GetPublicConstructors();
    //    foreach (var constructor in constructors)
    //    {
    //        var assigments = _constructorFinder.GetAssigments(constructor);
    //        var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer(assigments);
    //        var mapWithParameters = new ParameterDescriptor[toMembers.Count];
    //        for (var i = 0; i < toMembers.Count; i++)
    //        {
    //            var mapWithParameter = toMembers[i] switch
    //            {
    //                IParameterSymbol parameter => new ParameterDescriptor(parameter.Name.ToCamelCase(), parameter.Type),
    //                IPropertySymbol property => new ParameterDescriptor(property.Name.ToCamelCase(), property.Type),
    //                _ => null
    //            };

    //            if (mapWithParameter is not null)
    //            {
    //                mapWithParameters[i] = mapWithParameter;
    //            }
    //        }

    //        maps.Add(new ClassMapWithStub(from, to, mapWithParameters, mapLocation));
    //    }

    //    return maps;
    //}

    private static ImmutableArray<PropertyMap> ToImmutableArray(Span<PropertyMap> propertiesMaps)
    {
        var newArray = propertiesMaps.ToArray();

        return Unsafe.CastArrayToImmutableArray(ref newArray);
    }

    private static bool IsPotentialNullReference(PropertyMap propertyMap) =>
        propertyMap.IsTypesEquals
        && propertyMap.HasImplicitConversion
        && propertyMap.IsSourceNullable
        && !propertyMap.IsSourceNullable;

    private static bool HasSuitableProperty(Span<IPropertySymbol> properties)
    {
        foreach (var property in properties)
        {
            if (property is
                {
                    IsWriteOnly: false,
                    GetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                })
            {
                return true;
            }
        }

        return false;
    }

    private static Span<IPropertySymbol> GetMappableProperties(IMethodSymbol constructor, List<Assigment> assigments)
    {
        var propertiesInitializedByConstructor = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var assigment in assigments)
        {
            propertiesInitializedByConstructor.Add(assigment.Property);
        }

        var publicProperties = constructor.ContainingType.GetPublicProperties();
        var mappableProperties = new IPropertySymbol[publicProperties.Length];
        var mappablePropertiesCount = 0;
        for (int i = 0; i < publicProperties.Length; i++)
        {
            if (publicProperties[i] is
                {
                    IsReadOnly: false,
                    SetMethod.DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                }
                && !propertiesInitializedByConstructor.Contains(publicProperties[i].Name))
            {
                mappableProperties[mappablePropertiesCount] = publicProperties[i];
                mappablePropertiesCount++;
            }
        }

        return mappableProperties.AsSpan(0, mappablePropertiesCount);
    }
}
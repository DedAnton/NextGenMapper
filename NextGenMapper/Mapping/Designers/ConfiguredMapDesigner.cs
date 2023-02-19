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
        var (constructor, assigments) = constructorFinder.GetOptimalConstructor(source, destination, arguments);
        if (constructor == null)
        {
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);

            return MapsList.Create(Map.PotentialError(source, destination, diagnostic));
        }
        var referencesHistory = ImmutableList.Create(source);

        var destinationParameters = constructor.Parameters.AsSpan();
        var destinationProperties = GetMappableProperties(constructor, assigments);

        var sourcePublicProperties = source.GetPublicReadablePropertiesDictionary();

        var maps = new MapsList();
        var constructorProperties = new PropertyMap[destinationParameters.Length];
        var constructorPropertiesCount = 0;
        var configuredMapArguments = new NameTypePair[arguments.Count];
        var configuredMapArgumentsCount = 0;
        foreach (var destinationParameter in destinationParameters)
        {
            var destinationParameterType = destinationParameter.Type.ToNotNullableString();
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
                        var isTypesEquals = SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationParameter.Type);
                        var isTypesHasImplicitConversion = SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceProperty.Type, destinationParameter.Type, semanticModel);
                        var propertyMap = new PropertyMap(
                            assigment.Property,
                            assigment.Property,
                            sourceProperty.Type.ToNotNullableString(),
                            destinationParameter.Type.ToNotNullableString(),
                            sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                            destinationParameter.Type.NullableAnnotation == NullableAnnotation.Annotated,
                            isTypesEquals,
                            isTypesHasImplicitConversion);

                        if (IsPotentialNullReference(sourceProperty.Type, destinationParameter.Type, isTypesEquals, isTypesHasImplicitConversion))
                        {
                            var diagnostic = Diagnostics.PossiblePropertyNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationParameter.Name, destinationParameter.Type);

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
            var destinationPropertyType = destinationProperty.Type.ToNotNullableString();
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
                    true,
                    destinationProperty.Name);

                initializerProperties[initializerPropertiesCount] = propertyMap;
                initializerPropertiesCount++;

                configuredMapArguments[configuredMapArgumentsCount] = new NameTypePair(destinationProperty.Name, destinationPropertyType);
                configuredMapArgumentsCount++;
            }
            else
            {
                if (sourcePublicProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
                {
                    var isTypesEquals = SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationProperty.Type);
                    var isTypesHasImplicitConversion = SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceProperty.Type, destinationProperty.Type, semanticModel);
                    var propertyMap = new PropertyMap(
                        sourceProperty.Name,
                        destinationProperty.Name,
                        sourceProperty.Type.ToNotNullableString(),
                        destinationProperty.Type.ToNotNullableString(),
                        sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        isTypesEquals,
                        isTypesHasImplicitConversion);

                    if (IsPotentialNullReference(sourceProperty.Type, destinationProperty.Type, isTypesEquals, isTypesHasImplicitConversion))
                    {
                        var diagnostic = Diagnostics.PossiblePropertyNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationProperty.Name, destinationProperty.Type);

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

        var constructorPropertiesImmutable = constructorProperties.Length == constructorPropertiesCount
            ? Unsafe.CastArrayToImmutableArray(ref constructorProperties)
            : Unsafe.CastSpanToImmutableArray(constructorProperties.AsSpan().Slice(0, constructorPropertiesCount));
        var initializerPropertiesImmutable = initializerProperties.Length == initializerPropertiesCount
            ? Unsafe.CastArrayToImmutableArray(ref initializerProperties)
            : Unsafe.CastSpanToImmutableArray(initializerProperties.AsSpan().Slice(0, initializerPropertiesCount));

        if (configuredMapArgumentsCount != arguments.Count)
        {
            //TODO: research how to handle
            //throw new Exception($"Property or parameter not found for argument");
            isCompleteMethod = false;
            configuredMapArguments = Array.Empty<NameTypePair>();
        }

        var mockMethods = DesignClassMapMockMethods(source, destination, configuredMapArguments, isCompleteMethod, semanticModel);

        var map = Map.Configured(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            constructorPropertiesImmutable,
            initializerPropertiesImmutable,
            Unsafe.CastArrayToImmutableArray(ref configuredMapArguments),
            mockMethods,
            isCompleteMethod);

        maps.AddFirst(map);

        return maps;
    }

    private static bool ContainsArgument(this SeparatedSyntaxList<ArgumentSyntax> arguments, string argumentName)
    {
        foreach (var argument in arguments)
        {
            if (StringComparer.InvariantCulture.Equals(argument.NameColon?.Name.Identifier.ValueText, argumentName))
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
        var mockMethods = new ConfiguredMapMockMethod[constructors.Length];
        var mockMethodsCount = 0;
        var isDuplicatedMockRemoved = false;
        for (int i = 0; i < constructors.Length; i++)
        {
            var assigments = constructorFinder.GetAssigments(constructors[i]);
            var destinationParameters = constructors[i].Parameters.AsSpan();
            var destinationProperties = GetMappableProperties(constructors[i], assigments);
            var mockMethodParameters = new NameTypePair[destinationParameters.Length + destinationProperties.Length];
            var mockMethodParametersCount = 0;
            foreach(var destinationParameter in destinationParameters)
            {
                var parameter = new NameTypePair(destinationParameter.Name, destinationParameter.Type.ToString());
                mockMethodParameters[mockMethodParametersCount] = parameter;
                mockMethodParametersCount++;
            }
            foreach (var destinationProperty in destinationProperties)
            {
                var parameter = new NameTypePair(destinationProperty.Name, destinationProperty.Type.ToString());
                mockMethodParameters[mockMethodParametersCount] = parameter;
                mockMethodParametersCount++;
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

        return mockMethods.Length == mockMethodsCount
            ? Unsafe.CastArrayToImmutableArray(ref mockMethods)
            : Unsafe.CastSpanToImmutableArray(mockMethods.AsSpan().Slice(0, mockMethodsCount));
    }

    private static bool CompareArgumentsAndParameters(NameTypePair[] parameters, NameTypePair[] arguments)
    {
        if (parameters.Length != arguments.Length)
        {
            return false;
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Type != arguments[i].Type)
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

    private static bool IsPotentialNullReference(ITypeSymbol source, ITypeSymbol destination, bool isTypesEquals, bool hasImplicitConvertion)
        => (source.NullableAnnotation, destination.NullableAnnotation, isTypesEquals || hasImplicitConvertion) switch
        {
            (NullableAnnotation.NotAnnotated or NullableAnnotation.None, _, _) => false,
            (NullableAnnotation.Annotated, NullableAnnotation.Annotated, true) => false,
            _ => true
        };

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
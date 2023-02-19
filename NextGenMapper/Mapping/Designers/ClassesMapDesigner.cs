using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    private static MapsList DesignClassesMaps(
        ITypeSymbol source, 
        ITypeSymbol destination, 
        Location location, 
        SemanticModel semanticModel, 
        ImmutableList<ITypeSymbol> referencesHistory)
    {
        if (referencesHistory.FindIndex(x => SymbolEqualityComparer.Default.Equals(x, source)) != -1)
        {
            var diagnostic = Diagnostics.CircularReferenceError(location, referencesHistory.Add(source));

            return MapsList.Create(Map.Error(source, destination, diagnostic));
        }
        var newReferencesHistory = referencesHistory.Add(source);

        var sourceProperties = source.GetPublicProperties();
        if (!HasSuitableProperty(sourceProperties))
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInSource(location, source, destination);

            return MapsList.Create(Map.PotentialError(source, destination, diagnostic));
        }

        var constructorFinder = new ConstructorFinder(semanticModel);
        var (constructor, assigments) = constructorFinder.GetOptimalConstructor(source, destination);
        if (constructor == null)
        {
            var diagnostic = Diagnostics.ConstructorNotFoundError(location, source, destination);

            return MapsList.Create(Map.PotentialError(source, destination, diagnostic));
        }

        var destinationParameters = constructor.Parameters.AsSpan();
        var destinationProperties = GetMappableProperties(constructor, assigments);
        if (destinationProperties.Length == 0 && destinationParameters.Length == 0)
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInDestination(location, source, destination);

            return MapsList.Create(Map.Error(source, destination, diagnostic));
        }

        var sourcePublicProperties = source.GetPublicReadablePropertiesDictionary();

        var maps = new MapsList();
        Span<PropertyMap> constructorProperties = new PropertyMap[destinationParameters.Length];
        var constructorPropertiesCount = 0;
        foreach (var destinationParameter in destinationParameters)
        {
            //TODO: use dictionary for assigments
            foreach (var assigment in assigments)
            {
                if (assigment.Parameter == destinationParameter.Name
                    && sourcePublicProperties.TryGetValue(assigment.Property, out var sourceProperty))
                {
                    var isTypeEquals = SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationParameter.Type);
                    var isTypesHasImplicitConversion = SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceProperty.Type, destinationParameter.Type, semanticModel);
                    var propertyMap = new PropertyMap(
                        assigment.Property,
                        assigment.Property,
                        sourceProperty.Type.ToNotNullableString(),
                        destinationParameter.Type.ToNotNullableString(),
                        sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        destinationParameter.Type.NullableAnnotation == NullableAnnotation.Annotated,
                        isTypeEquals,
                        isTypesHasImplicitConversion);

                    if (IsPotentialNullReference(sourceProperty.Type, destinationParameter.Type, isTypeEquals, isTypesHasImplicitConversion))
                    {
                        var diagnostic = Diagnostics.PossiblePropertyNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationParameter.Name, destinationParameter.Type);

                        return MapsList.Create(Map.Error(source, destination, diagnostic));
                    }

                    constructorProperties[constructorPropertiesCount] = propertyMap;
                    constructorPropertiesCount++;

                    if (!propertyMap.IsTypesEquals && !propertyMap.HasImplicitConversion)
                    {
                        var propertyTypesMaps = DesignMaps(sourceProperty.Type, destinationParameter.Type, location, semanticModel, newReferencesHistory);
                        maps.Append(propertyTypesMaps);
                    }
                }
            }
        }

        Span<PropertyMap> initializerProperties = new PropertyMap[destinationProperties.Length];
        var initializerPropertiesCount = 0;
        foreach (var destinationProperty in destinationProperties)
        {
            if (sourcePublicProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
            {
                var isTypeEquals = SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationProperty.Type);
                var isTypesHasImplicitConversion = SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceProperty.Type, destinationProperty.Type, semanticModel);
                var propertyMap = new PropertyMap(
                    sourceProperty.Name,
                    destinationProperty.Name,
                    sourceProperty.Type.ToNotNullableString(),
                    destinationProperty.Type.ToNotNullableString(),
                    sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                    isTypeEquals,
                    isTypesHasImplicitConversion);

                if (IsPotentialNullReference(sourceProperty.Type, destinationProperty.Type, isTypeEquals, isTypesHasImplicitConversion))
                {
                    var diagnostic = Diagnostics.PossiblePropertyNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationProperty.Name, destinationProperty.Type);

                    return MapsList.Create(Map.Error(source, destination, diagnostic));
                }

                initializerProperties[initializerPropertiesCount] = propertyMap;
                initializerPropertiesCount++;

                if (!propertyMap.IsTypesEquals && !propertyMap.HasImplicitConversion)
                {
                    var propertyTypesMaps = DesignMaps(sourceProperty.Type, destinationProperty.Type, location, semanticModel, newReferencesHistory);
                    maps.Append(propertyTypesMaps);
                }
            }
        }

        if (constructorPropertiesCount + initializerPropertiesCount == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);

            return MapsList.Create(Map.PotentialError(source, destination, diagnostic));
        }

        var classMap = Map.Class(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            Unsafe.CastSpanToImmutableArray(constructorProperties.Slice(0, constructorPropertiesCount)),
            Unsafe.CastSpanToImmutableArray(initializerProperties.Slice(0, initializerPropertiesCount)));

        maps.AddFirst(classMap);

        return maps;
    }

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

    private static Span<IPropertySymbol> GetMappableProperties(IMethodSymbol constructor, System.Collections.Generic.List<Assigment> assigments)
    {
        var propertiesInitializedByConstructor = new System.Collections.Generic.HashSet<string>(StringComparer.InvariantCulture);
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
using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System.Collections.Immutable;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static class PropertiesMapDesigner
{
    internal static PropertyMap? DesignPropertyMap(
        string sourceName,
        ITypeSymbol sourceType,
        ITypeSymbol sourceContainedType,
        string destinationName,
        ITypeSymbol destinationType,
        ITypeSymbol destinationContainedType,
        Location location,
        SemanticModel semanticModel,
        ImmutableList<ITypeSymbol> referencesHistory,
        ref ValueListBuilder<Map> maps,
        CancellationToken cancellationToken)
    {
        var isTypeEquals = SourceCodeAnalyzer.IsTypesAreEquals(sourceType, destinationType);
        var isTypesHasImplicitConversion = SourceCodeAnalyzer.IsTypesHasImplicitConversion(sourceType, destinationType, semanticModel);
        var propertyMap = new PropertyMap(
            sourceName,
            destinationName,
            sourceType.ToNotNullableString(),
            destinationType.ToNotNullableString(),
            sourceType.NullableAnnotation == NullableAnnotation.Annotated,
            destinationType.NullableAnnotation == NullableAnnotation.Annotated,
            isTypeEquals,
            isTypesHasImplicitConversion);

        if (DesignersHelper.IsPotentialNullReference(sourceType, destinationType, isTypeEquals, isTypesHasImplicitConversion))
        {
            var diagnostic = Diagnostics.PossiblePropertyNullReference(location, sourceContainedType, sourceName, sourceType, destinationContainedType, destinationName, destinationType);
            maps.Append(Map.Error(sourceContainedType, destinationContainedType, diagnostic));

            return null;
        }

        if (!propertyMap.IsTypesEquals && !propertyMap.HasImplicitConversion)
        {
            MapDesigner.DesignMaps(sourceType, destinationType, location, semanticModel, referencesHistory, ref maps, cancellationToken);
        }

        return propertyMap;
    }
}

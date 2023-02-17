using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Utils;
using System.Collections.Immutable;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    public static Map[] DesignMaps(MapTarget target)
        => DesignMaps(target.Source, target.Destination, target.Location, target.SemanticModel, ImmutableList<ITypeSymbol>.Empty).ToArray();

    internal static MapsList DesignMaps(ITypeSymbol source, ITypeSymbol destination, Location location, SemanticModel semanticModel, ImmutableList<ITypeSymbol> referencesHistory)
    {
        if (referencesHistory.FindIndex(x => SymbolEqualityComparer.Default.Equals(x, source)) != -1)
        {
            var diagnostic = Diagnostics.CircularReferenceError(location, referencesHistory.Add(source));

            return MapsList.Create(Map.Error(source, destination, diagnostic));
        }

        if (SourceCodeAnalyzer.IsTypesAreClasses(source, destination))
        {
            return DesignClassesMaps(source, destination, location, semanticModel, referencesHistory.Add(source));
        }

        if (SourceCodeAnalyzer.IsTypesAreEnums(source, destination))
        {
            var map = DesignEnumsMap(source, destination, location);

            return MapsList.Create(map);
        }

        if (SourceCodeAnalyzer.IsTypesAreCollections(source, destination))
        {
            return DesignCollectionsMap(source, destination, location, semanticModel);
        }

        return new MapsList();
    }
}
using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Utils;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static partial class MapDesigner
{
    public static ImmutableArray<Map> DesignMaps(MapTarget target, CancellationToken cancellationToken)
    {
        try
        {
            var maps = new ValueListBuilder<Map>(8);
            DesignMaps(target.Source, target.Destination, target.Location, target.SemanticModel, ImmutableList<ITypeSymbol>.Empty, ref maps, cancellationToken);

            return maps.ToImmutableArray();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch(Exception ex)
        {
            var diagnostic = Diagnostics.MapperInternalError(target.Location, ex);
            return ImmutableArray.Create(Map.Error(target.Source, target.Destination, diagnostic));
        }
    }

    internal static void DesignMaps(
        ITypeSymbol source, 
        ITypeSymbol destination, 
        Location location, 
        SemanticModel semanticModel, 
        ImmutableList<ITypeSymbol> referencesHistory,
        ref ValueListBuilder<Map> maps,
        CancellationToken cancellationToken)
    {
        if (referencesHistory.FindIndex(x => SymbolEqualityComparer.Default.Equals(x, source)) != -1)
        {
            var diagnostic = Diagnostics.CircularReferenceError(location, referencesHistory.Add(source));
            maps.Append(Map.Error(source, destination, diagnostic));

            return;
        }

        if (SourceCodeAnalyzer.IsTypesAreCollections(source, destination))
        {
            DesignCollectionsMap(source, destination, location, semanticModel, referencesHistory, ref maps, cancellationToken);
        } 
        else if (SourceCodeAnalyzer.IsTypesAreClasses(source, destination))
        {
            DesignClassesMaps(source, destination, location, semanticModel, referencesHistory, ref maps, cancellationToken);
        }
        else if (SourceCodeAnalyzer.IsTypesAreEnums(source, destination))
        {
            DesignEnumsMap(source, destination, location, ref maps, cancellationToken);
        }
    }
}
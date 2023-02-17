using System.Collections.Immutable;
using NextGenMapper.Mapping.Maps.Models;

namespace NextGenMapper.Mapping.Maps;

internal readonly struct EnumMap : IMap
{
    public EnumMap(string source, string destination, ImmutableArray<EnumFieldMap> fields)
    {
        Source = source;
        Destination = destination;
        Fields = fields;
    }

    public string Source { get; }
    public string Destination { get; }
    public ImmutableArray<EnumFieldMap> Fields { get; }
}

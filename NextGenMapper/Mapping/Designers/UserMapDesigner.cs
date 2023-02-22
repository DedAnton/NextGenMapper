using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;

namespace NextGenMapper.Mapping.Designers;

internal class UserMapDesigner
{
    public static UserMap DesingUserMaps(UserMapTarget target)
     => Map.User(target.Source.ToNotNullableString(), target.Destination.ToNotNullableString()).UserMap;
}

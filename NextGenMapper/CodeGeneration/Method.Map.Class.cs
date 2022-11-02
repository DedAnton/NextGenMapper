using NextGenMapper.CodeAnalysis.Maps;
using System;

namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    public void GenerateClassMapMethod(ClassMap map)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(MapperSourceBuilder)} was not initialized");
        }
        MapMethodDifinition(map.From, map.To);
        ConstructorInvocation(map);
    }

    private void ConstructorInvocation(ClassMap map)
    {
        //TODO: Remove ToArray()
        var constructorArguments = map.ConstructorProperties.ToArray();
        var initializerAssigments = map.InitializerProperties.ToArray();

        _builder.Append($" => new {map.To}");
        ConstructorArguments(constructorArguments);
        InitializerAssigments(initializerAssigments);
        _builder.Append(';');
    }

    private void ConstructorArgument(MemberMap member)
    {
        if (member.IsSameTypes || _compilation.HasImplicitConversion(member.FromType, member.ToType))
        {
            _builder.Append($"source.{member.FromName}");
        }
        else
        {
            _builder.Append($"source.{member.FromName}.Map<{member.ToType}>()");
        }
    }

    private void InitializerAssigment(MemberMap member)
    {
        if (member.IsSameTypes || _compilation.HasImplicitConversion(member.FromType, member.ToType))
        {
            _builder.Append($"{member.ToName} = source.{member.FromName}");
        }
        else
        {
            _builder.Append($"{member.ToName} = source.{member.FromName}.Map<{member.ToType}>()");
        }
    }

    private void ConstructorArguments(Span<MemberMap> members)
    {
        if (members.Length == 0)
        {
            _builder.Append("()");
            return;
        }

        var lastIndex = members.Length - 1;
        _builder.Append("\r\n        (\r\n");
        for (var i = 0; i < members.Length; i++)
        {
            _builder.Append("            ");
            ConstructorArgument(members[i]);
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        )");
    }

    private void InitializerAssigments(Span<MemberMap> members)
    {
        if (members.Length == 0)
        {
            return;
        }

        var lastIndex = members.Length - 1;
        _builder.Append("\r\n        {\r\n");
        for (var i = 0; i < members.Length; i++)
        {
            _builder.Append("            ");
            InitializerAssigment(members[i]);
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        }");
    }
}

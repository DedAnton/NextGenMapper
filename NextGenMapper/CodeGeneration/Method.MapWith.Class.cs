using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;

namespace NextGenMapper.CodeGeneration;

internal ref partial struct MapperSourceBuilder
{
    public void GenerateClassMapWithMethod(ClassMapWith map)
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(MapperSourceBuilder)} was not initialized");
        }
        MethodDifinition(map);
        ConstructorInvocation(map);
    }

    private void MethodDifinition(ClassMapWith map)
    {
        _builder.Append(
$@"        internal static {map.To} MapWith<To>
        (
            this {map.From} source");
        for (var i = 0; i < map.Arguments.Length; i++)
        {
            _builder.Append($",\r\n            {map.Arguments[i].Type} {map.Arguments[i].Name}");
        }
        _builder.Append("\r\n        )");
    }

    private void ConstructorInvocation(ClassMapWith map)
    {
        //TODO: Remove ToArray()
        var constructorArguments = map.ConstructorProperties.ToArray();
        var initializerAssigments = map.InitializerProperties.ToArray();

        _builder.Append($"\r\n        => new {map.To}");
        ConstructorArguments(constructorArguments, map.Arguments);
        InitializerAssigments(initializerAssigments, map.Arguments);
        _builder.Append(';');
    }

    private void ConstructorArgument(MemberMap member, MapWithInvocationAgrument[] agruments)
    {
        var argument = Array.Find(agruments, x => member.ToName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
        if (argument is not null)
        {
            _builder.Append(argument.Name);
        }
        else
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
    }

    private void InitializerAssigment(MemberMap member, MapWithInvocationAgrument[] agruments)
    {
        var argument = Array.Find(agruments, x => member.ToName.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase));
        if (argument is not null)
        {
            _builder.Append($"{member.ToName} = {argument.Name}");
        }
        else
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
    }

    private void ConstructorArguments(Span<MemberMap> members, MapWithInvocationAgrument[] userAgruments)
    {
        if (members.Length == 0)
        {
            return;
        }

        var lastIndex = members.Length - 1;
        _builder.Append("\r\n        (\r\n");
        for (var i = 0; i < members.Length; i++)
        {
            _builder.Append("            ");
            ConstructorArgument(members[i], userAgruments);
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        )");
    }

    private void InitializerAssigments(Span<MemberMap> members, MapWithInvocationAgrument[] userAgruments)
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
            InitializerAssigment(members[i], userAgruments);
            if (i < lastIndex)
            {
                _builder.Append(",\r\n");
            }
        }
        _builder.Append("\r\n        }");
    }
}

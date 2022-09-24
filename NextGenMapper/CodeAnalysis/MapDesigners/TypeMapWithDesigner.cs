using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.MapDesigners;

public class TypeMapWithDesigner
{
    private readonly TypeMapDesigner _classMapDesigner;
    private readonly DiagnosticReporter _diagnosticReporter;
    private readonly ConstructorFinder _constructorFinder;

    public TypeMapWithDesigner(DiagnosticReporter diagnosticReporter)
    {
        _classMapDesigner = new(diagnosticReporter);
        _diagnosticReporter = diagnosticReporter;
        _constructorFinder = new();
    }

    public List<TypeMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, List<MapWithInvocationAgrument> arguments)
    {
        var byUser = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var argument in arguments)
        {
            byUser.Add(argument.Name);
        }

        var constructor = _constructorFinder.GetOptimalConstructor(from, to, byUser);
        if (constructor == null)
        {
            _diagnosticReporter.ReportConstructorNotFoundError(to.Locations, from, to);
            return new();
        }

        var maps = new List<TypeMap>();
        var membersMaps = new List<MemberMap>();
        var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer();
        foreach (var member in toMembers)
        {
            var isProvidedByUser = byUser.Contains(member.Name);
            MemberMap? memberMap = (member, isProvidedByUser) switch
            {
                (IParameterSymbol parameter, false) => _classMapDesigner.DesignConstructorParameterMap(from, parameter),
                (IPropertySymbol property, false) => _classMapDesigner.DesignInitializerPropertyMap(from, property),
                (IParameterSymbol parameter, true) => MemberMap.User(parameter),
                (IPropertySymbol property, true) => MemberMap.User(property),
                _ => null
            };

            if (memberMap == null)
            {
                continue;
            }
            membersMaps.Add(memberMap);

            if (memberMap is { IsSameTypes: false, IsProvidedByUser: false })
            {
                maps.AddRange(_classMapDesigner.DesignMapsForPlanner(memberMap.FromType, memberMap.ToType));
            }
        }

        maps.Add(new ClassMapWith(from, to, membersMaps, arguments));

        return maps;
    }
}

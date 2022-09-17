using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper.CodeAnalysis.MapDesigners;

public class ClassMapWithDesigner
{
    private readonly ClassMapDesigner _classMapDesigner;
    private readonly DiagnosticReporter _diagnosticReporter;
    private readonly ConstructorFinder _constructorFinder;

    public ClassMapWithDesigner(DiagnosticReporter diagnosticReporter)
    {
        _classMapDesigner = new(diagnosticReporter);
        _diagnosticReporter = diagnosticReporter;
        _constructorFinder = new();
    }

    public List<ClassMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, List<MapWithInvocationAgrument> arguments)
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

        var maps = new List<ClassMap>();
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

            //if (memberMap.MapType is MemberMapType.UnflattenConstructor or MemberMapType.UnflattenInitializer)
            //{
            //    maps.AddRange(_classMapDesigner.DesignUnflattingClassMap(from, memberMap.ToName, memberMap.ToType));
            //}
            if (memberMap is { IsSameTypes: false, IsProvidedByUser: false })
            {
                maps.AddRange(_classMapDesigner.DesignMapsForPlanner(memberMap.FromType, memberMap.ToType));
            }
        }
        
        maps.Add(new ClassMapWith(from, to, membersMaps, arguments));

        return maps;
    }
}

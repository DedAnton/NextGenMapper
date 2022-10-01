using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
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

    public List<TypeMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, MapWithInvocationAgrument[] arguments, Location mapLocation)
    {
        var byUser = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var argument in arguments)
        {
            byUser.Add(argument.Name);
        }

        var constructor = _constructorFinder.GetOptimalConstructor(from, to, byUser);
        if (constructor == null)
        {
            _diagnosticReporter.ReportConstructorNotFoundError(mapLocation, from, to);
            return new();
        }

        var maps = new List<TypeMap>();
        var membersMaps = new List<MemberMap>();
        var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer();
        var mapWithParameters = new List<ParameterDescriptor>(toMembers.Count);
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
            var mapWithParameter = member switch
            {
                IParameterSymbol parameter => new ParameterDescriptor(parameter.Name.ToCamelCase(), parameter.Type),
                IPropertySymbol property => new ParameterDescriptor(property.Name.ToCamelCase(), property.Type),
                _ => null
            };
            if (mapWithParameter is not null)
            {
                mapWithParameters.Add(mapWithParameter);
            }

            if (memberMap == null)
            {
                continue;
            }
            membersMaps.Add(memberMap);

            if (memberMap is { IsSameTypes: false, IsProvidedByUser: false })
            {
                maps.AddRange(_classMapDesigner.DesignMapsForPlanner(memberMap.FromType, memberMap.ToType, mapLocation));
            }
        }

        
        maps.Add(new ClassMapWith(from, to, membersMaps, arguments, mapWithParameters, mapLocation));

        return maps;
    }

    public List<ClassMapWithStub> DesignStubMethodMap(ITypeSymbol from, ITypeSymbol to, Location mapLocation)
    {
        var maps = new List<ClassMapWithStub>();
        var constructors = to.GetPublicConstructors();
        foreach (var constructor in constructors)
        {
            var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer();
            var mapWithParameters = new ParameterDescriptor[toMembers.Count];
            for (var i = 0; i < toMembers.Count; i++)
            {
                var mapWithParameter = toMembers[i] switch
                {
                    IParameterSymbol parameter => new ParameterDescriptor(parameter.Name.ToCamelCase(), parameter.Type),
                    IPropertySymbol property => new ParameterDescriptor(property.Name.ToCamelCase(), property.Type),
                    _ => null
                };

                if (mapWithParameter is not null)
                {
                    mapWithParameters[i] = mapWithParameter;
                }
            }

            maps.Add(new ClassMapWithStub(from, to, mapWithParameters, mapLocation));
        }

        return maps;
    }
}

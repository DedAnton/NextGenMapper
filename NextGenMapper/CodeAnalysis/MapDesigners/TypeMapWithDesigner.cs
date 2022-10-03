using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public List<TypeMap> DesignMapsForPlanner(ITypeSymbol from, ITypeSymbol to, HashSet<string> argumentsNames, Location mapLocation)
    {
        var constructor = _constructorFinder.GetOptimalConstructor(from, to, argumentsNames);
        if (constructor == null)
        {
            _diagnosticReporter.ReportConstructorNotFoundError(mapLocation, from, to);
            return new();
        }

        var maps = new List<TypeMap>();
        var membersMaps = new List<MemberMap>();
        var toMembers = constructor.GetPropertiesInitializedByConstructorAndInitializer();
        var mapWithParameters = new List<ParameterDescriptor>(toMembers.Count);
        var mapWithArguments = new List<MapWithInvocationAgrument>(argumentsNames.Count);
        foreach (var member in toMembers)
        {
            var isProvidedByUser = argumentsNames.Contains(member.Name);
            var memberMap = (member, isProvidedByUser) switch
            {
                (IParameterSymbol parameter, false) => _classMapDesigner.DesignConstructorParameterMap(from, parameter),
                (IPropertySymbol property, false) => _classMapDesigner.DesignInitializerPropertyMap(from, property),
                (IParameterSymbol parameter, true) => MemberMap.User(parameter),
                (IPropertySymbol property, true) => MemberMap.User(property),
                _ => throw new ArgumentOutOfRangeException(nameof(member), "member must be 'IParameterSymbol' or 'IPropertySymbol'")
            };
            var mapWithParameter = member switch
            {
                IParameterSymbol parameter => new ParameterDescriptor(parameter.Name.ToCamelCase(), parameter.Type),
                IPropertySymbol property => new ParameterDescriptor(property.Name.ToCamelCase(), property.Type),
                _ => throw new ArgumentOutOfRangeException(nameof(member), "member must be 'IParameterSymbol' or 'IPropertySymbol'")
            };

            mapWithParameters.Add(mapWithParameter);

            if (argumentsNames.Contains(mapWithParameter.Name))
            {
                mapWithArguments.Add(new MapWithInvocationAgrument(mapWithParameter.Name, mapWithParameter.Type));
            }

            if (memberMap is not null)
            {
                membersMaps.Add(memberMap);
            }

            if (memberMap is { IsSameTypes: false, IsProvidedByUser: false })
            {
                maps.AddRange(_classMapDesigner.DesignMapsForPlanner(memberMap.FromType, memberMap.ToType, mapLocation));
            }
        }
        
        maps.Add(new ClassMapWith(from, to, membersMaps, mapWithArguments.ToArray(), mapWithParameters, mapLocation));

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

using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Maps;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis
{
    public class MapPlanner
    {
        private readonly HashSet<TypeMap> _maps = new();
        private readonly HashSet<(ITypeSymbol from, ITypeSymbol to)> _mapTypes = new(new MapTypesEqualityComparer());
        private readonly HashSet<(ITypeSymbol from, ITypeSymbol to, MapWithInvocationAgrument[] arguments)> _mapWithTypes = new(new MapWithTypesEqualityComparer());
        private readonly HashSet<(ITypeSymbol from, ITypeSymbol to, ParameterDescriptor[] parameters)> _mapWithStub = new(new MapWithStubsEqualityComparer());

        public IReadOnlyCollection<TypeMap> Maps => _maps;

        public void AddMap(TypeMap map)
        {
            if (_mapTypes.Contains((map.From, map.To)))
            {
                //TODO: maybe add diagnostic
                return;
            }

            _maps.Add(map);
            _mapTypes.Add((map.From, map.To));
        }

        public void AddUserDefinedMap(ITypeSymbol from, ITypeSymbol to)
        {
            if (_mapTypes.Contains((from, to)))
            {
                //TODO: maybe add diagnostic
                return;
            }

            _mapTypes.Add((from, to));
        }

        public void AddMapWith(ClassMapWith map)
        {
            if (_mapWithTypes.Contains((map.From, map.To, map.Arguments)))
            {
                //TODO: maybe add diagnostic
                return;
            }

            _maps.Add(map);
            _mapWithTypes.Add((map.From, map.To, map.Arguments));
            //_mapWithStub.Add((map.From, map.To));
        }

        public void AddMapWithStub(ClassMapWithStub map)
        {
            _maps.Add(map);
            _mapWithStub.Add((map.From, map.To, map.Parameters));
        }

        public bool IsTypesMapAlreadyPlanned(ITypeSymbol from, ITypeSymbol to) => _mapTypes.Contains((from, to));

        public bool IsTypesMapWithAlreadyPlanned(ITypeSymbol from, ITypeSymbol to, MapWithInvocationAgrument[] agruments) 
            => _mapWithTypes.Contains((from, to, agruments));

        public bool IsTypesMapWithStubAlreadyPlanned(ITypeSymbol from, ITypeSymbol to, ParameterDescriptor[] parameters) => _mapWithStub.Contains((from, to, parameters));
    }
}

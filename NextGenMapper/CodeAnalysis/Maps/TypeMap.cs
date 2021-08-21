using Microsoft.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Models;
using System.Collections.Generic;

namespace NextGenMapper.CodeAnalysis.Maps
{
    public abstract class TypeMap
    {
        public Type From { get; }
        public Type To { get; }

        public TypeMap(Type from, Type to)
        {
            From = from;
            To = to;
        }

        public override bool Equals(object? obj)
        {
            return obj is TypeMap map && From == map.From && To == map.To;
        }

        public override int GetHashCode()
        {
            int hashCode = -1781160927;
            hashCode = hashCode * -1521134295 + From.GetHashCode();
            hashCode = hashCode * -1521134295 + To.GetHashCode();
            return hashCode;
        }
    }
}

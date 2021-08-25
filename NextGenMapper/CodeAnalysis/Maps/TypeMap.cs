using Microsoft.CodeAnalysis;

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
            return obj is TypeMap map && Equals(map.From, map.To);
        }

        public override int GetHashCode()
        {
            int hashCode = -1781160927;
            hashCode = hashCode * -1521134295 + From.Name.GetHashCode();
            hashCode = hashCode * -1521134295 + To.Name.GetHashCode();
            return hashCode;
        }

        public bool Equals(Type from, Type to) => from.Name == to.Name;
    }
}

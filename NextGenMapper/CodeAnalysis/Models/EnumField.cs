namespace NextGenMapper.CodeAnalysis.Models
{
    public class EnumField
    {
        public EnumField(string name, long? value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public long? Value { get; }
    }
}

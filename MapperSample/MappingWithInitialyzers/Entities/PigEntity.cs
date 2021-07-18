namespace MapperSample.MappingWithInitialyzers.Entities
{
    public class PigEntity
    {
        public string Name { get; set; }
        public PigHomeEntity Home { get; set; }
    }

    public class PigHomeEntity
    {
        public string Address { get; set; }
    }
}

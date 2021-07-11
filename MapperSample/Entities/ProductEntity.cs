using NextGenMapper;

namespace MapperSample.Entities
{
    [MapReverse]
    public class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UpcString { get; set; }
    }
}

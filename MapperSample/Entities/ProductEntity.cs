namespace MapperSample.Entities
{
    public class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UpcString { get; set; }
        public UserEntity User { get; set; }
    }
}

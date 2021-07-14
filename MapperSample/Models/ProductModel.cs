namespace MapperSample.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Upc { get; set; }
        public UserModel User { get; set; }
    }
}

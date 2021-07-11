using MapperSample.Entities;
using NextGenMapper;

namespace MapperSample.Models
{
    [MapTo(typeof(ProductEntity))]
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [TargetName("UpcString")]
        public string Upc { get; set; }
    }
}

namespace MapperSample.Models
{
    public class PigModel
    {
        public string Name { get; set; }
        public PigHomeModel Home { get; set; }
    }

    public class PigHomeModel
    {
        public string Address { get; set; }
    }
}

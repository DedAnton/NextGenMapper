namespace MapperSample.Entities
{
    public class CatEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public HumanEntity Human { get; set; }
    }
}

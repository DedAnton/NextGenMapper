namespace MapperSample.MappingWithConstructor.Models
{
    public class HorseModel
    {
        public HorseModel(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; }
        public int Age { get; }
        public string Color { get; set; }
        public string Height { get; set; }
    }
}

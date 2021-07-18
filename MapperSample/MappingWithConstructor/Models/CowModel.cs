namespace MapperSample.MappingWithConstructor.Models
{
    public class CowModel
    {
        public string Name { get; }
        public int Age { get; }

        public CowModel(string name)
        {
            Name = name;
        }

        public CowModel(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}

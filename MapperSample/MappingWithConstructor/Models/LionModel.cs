namespace MapperSample.MappingWithConstructor.Models
{
    public class LionModel
    {
        public string Name { get; }
        public string Birthday { get; }
        public string Gender { get; set; }

        public LionModel(string name, string birthday)
        {
            Name = name;
            Birthday = birthday;
        }
    }
}

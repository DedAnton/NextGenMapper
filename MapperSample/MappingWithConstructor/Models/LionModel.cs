namespace MapperSample.MappingWithConstructor.Models
{
    public class LionModel
    {
        public string Name { get; }
        public string Birthday { get; }
        public string Gender { get; }

        public LionModel(string name, string birthday)
        {
            Name = name;
            Birthday = birthday;
        }

        public LionModel(string name, string birthday, string gender)
        {
            Name = name;
            Birthday = birthday;
            Gender = gender;
        }
    }
}

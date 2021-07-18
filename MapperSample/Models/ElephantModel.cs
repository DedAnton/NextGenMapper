namespace MapperSample.Models
{
    public class ElephantModel
    {
        public string Name { get; }
        public int Age { get; set; }
        public int TrunkLength { get; set; }
        public bool IsUsedSecondConstructor { get; }

        public ElephantModel(string name)
        {
            Name = name;
        }

        public ElephantModel(string name, int age)
        {
            Name = name;
            Age = age;
            IsUsedSecondConstructor = true;
        }
    }
}

namespace MapperSample.Models
{
    public class DogModel
    {
        public string Name { get; }
        public string Age { get; set; }
        public bool IsUsedSecondConstructor { get; }

        public DogModel() { }

        public DogModel(string name, string age)
        {
            Name = name;
            Age = age;
            IsUsedSecondConstructor = true;
        }
    }
}

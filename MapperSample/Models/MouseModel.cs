namespace MapperSample.Models
{
    public class MouseModel
    {
        public string Name { get; }
        public int Age { get; }

        public MouseModel(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}

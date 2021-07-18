namespace MapperSample.Entities
{
    public class MouseEntity
    {
        public string Name { get; }
        public int Age { get; }

        public MouseEntity(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}

namespace MapperSample.MappingWithConstructor.Entities
{
    public class RabbitEntity
    {
        public string Name { get; }
        public int Age { get; }
        public RabbitHouseEntity House { get; }

        public RabbitEntity(string name, int age, RabbitHouseEntity house)
        {
            Name = name;
            Age = age;
            House = house;
        }
    }

    public class RabbitHouseEntity
    {
        public string Address { get; }

        public RabbitHouseEntity(string address)
        {
            Address = address;
        }
    }
}

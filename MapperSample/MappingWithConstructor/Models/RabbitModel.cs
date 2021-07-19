namespace MapperSample.MappingWithConstructor.Models
{
    public class RabbitModel
    {
        public string Name { get; }
        public int Age { get; }
        public RabbitHouseModel House { get; }

        public RabbitModel(string name, int age, RabbitHouseModel house)
        {
            Name = name;
            Age = age;
            House = house;
        }
    }

    public class RabbitHouseModel
    {
        public string Address { get; }

        public RabbitHouseModel(string address)
        {
            Address = address;
        }
    }
}

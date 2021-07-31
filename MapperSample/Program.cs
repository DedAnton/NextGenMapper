using System;
using System.Collections.Generic;

namespace MapperSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var entity = new UserEntity { FirstName = "Anton", SecondName = "Ryabchikov", Birthday = new DateTime(1997, 05, 20)};
            int age = 10;
            var model = new UserModel(entity.FirstName, age);
            //var model = entity.Map<UserModel>();

            var asd = entity with { Password = "123" };
        }

        public enum EnumFrom
        {
            ValueA,
            ValueB,
            ValueC
        }

        public enum EnumTo
        {
            valueA = 10,
            valueB = 20,
            valueC = 30
        }
        public EnumTo Map(EnumFrom source)
        {
            return source switch
            {
                EnumFrom.ValueA => EnumTo.valueA,
                EnumFrom.ValueB => EnumTo.valueB,
                EnumFrom.ValueC => EnumTo.valueC,
                _ => throw new ArgumentOutOfRangeException($"value {source} was out of range of {typeof(EnumTo)}")
            };
        }

        public byte Map(AnyEnum enumSrc) => (byte)enumSrc;
    }

    public class Destination
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public string City { get; set; }

        public Destination(string name, int height, string city)
        {
            Name = name;
            Height = height;
            City = city;
        }
    }

    public enum AnyEnum : int
    {
        First,
        Second,
        Third
    }

    public class MapFailedException
    {
        
    }

    //[Mapper]
    //public class Mapings
    //{
    //    [Partial]
    //    public MapperSample.UserModel Map(MapperSample.UserEntity source) => new MapperSample.UserModel($"{source.FirstName} {source.SecondName}", DateTime.Now.Year - source.Birthday.Year);
    //}

    public record UserEntity
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime Birthday { get; set; }
        public string Password { get; init; }
    }

    public record UserModel(string Name, double Age)
    {
        public string Password { get; }
    }
}

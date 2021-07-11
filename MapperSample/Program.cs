using NextGenMapper;
using System;
using MapperSample.Models;
using MapperSample.Entities;

namespace MapperSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = new UserModel { Name = "Anton", Email = "antondinamo2@mail.ru", Age = 24 };
            var entity = Mapper.Map(model);
            Console.WriteLine($"{entity.Name} - {entity.Email} - {entity.Age}");

            var model2 = new ProductModel { Id = 1, Name = "Hamburger", Upc = "835420124123" };
            var entity2 = Mapper.Map(model2);
            Console.WriteLine($"{entity2.Id} - {entity2.Name} - {entity2.UpcString}");

            entity2.UpcString = "00000000000";
            var model2r = Mapper.Map(entity2);
            Console.WriteLine($"{model2r.Id} - {model2r.Name} - {model2r.Upc}");
        }

        record User(string Name, string Email, int Age) { }
    }

    public static class MyMapper
    {
        public static ProductEntity Map(ProductModel source, Func<ProductModel, ProductEntity> mapFunc) => mapFunc(source);
    }
}

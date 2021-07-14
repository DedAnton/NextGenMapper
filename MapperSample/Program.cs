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
            var user = new UserModel { Name = "Anton", Email = "antondinamo2@mail.ru", Age = 24 };
            var product = new ProductModel { Id = 1, Name = "Hamburger", Upc = "835420124123", User = user };

            var entity = product.Map<ProductEntity>();
            Console.WriteLine($"{entity.Id} - {entity.Name} - {entity.UpcString}");
            Console.WriteLine($"{entity.User.Name} - {entity.User.Email} - {entity.User.Age}");
        }

        record User(string Name, string Email, int Age) { }
    }
}

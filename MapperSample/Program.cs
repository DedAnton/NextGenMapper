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
            var user = new UserModel { Name = "Anton Ryabchikov", Email = "antondinamo2@mail.ru", Age = 24, Birthday = new DateTime(1997, 5, 20) };
            var product = new ProductModel { Id = 1, Name = "Hamburger", Upc = "835420124123", User = user};

            var entity = product.Map<ProductEntity>();
            Console.WriteLine($"{entity.Id} - {entity.Name} - {entity.UpcString}");
            Console.WriteLine($"{entity.User.FirstName} - {entity.User.SecondName} - {entity.User.Email} - {entity.User.Age} - {entity.User.Birthday}");


            var human = new HumanModel { Name = "Иван Васильевич", Age = 44 };
            var cat = new CatModel { Name = "Барсик", Age = 6, Human = human };
            var catEntity = cat.Map<CatEntity>();
            Console.WriteLine($"cat {catEntity.Name} was {catEntity.Age} age old");
            Console.WriteLine($"{catEntity.Human.Name} - {catEntity.Human.Age}");
        }
    }
}

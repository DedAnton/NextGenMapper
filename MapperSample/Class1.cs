using MapperSample.Models;
using System;
using System.Linq;
using NextGenMapper;
using MapperSample.Entities;
using User = MapperSample.Entities.UserEntity;
using System.Collections.Generic;

namespace MapperSample
{
    [Mapper]
    class Class1
    {
        public string Map(DateTime source) => source.ToString("O");

        [Partial]
        private ProductEntity Asd(ProductModel src) => new ProductEntity { UpcString = src.Upc };

        [Partial]
        private User ХуйБомжа(UserModel source)
        {
            var names = source.Name?.Split(' ');

            return new User
            {
                FirstName = names.FirstOrDefault(),
                SecondName = names.LastOrDefault()
            };
        }

        public HumanEntity Asfsdg(HumanModel hmn)
        {
            var человек = new HumanEntity() { Age = hmn.Age, Name = hmn.Name };

            return человек;
        }
        //private User Asd(UserModel source) => new User() { FirstName = source.Name, SecondName = source.Name};
    }
}

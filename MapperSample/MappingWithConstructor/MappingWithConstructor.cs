using MapperSample.MappingWithConstructor.Entities;
using MapperSample.MappingWithConstructor.Models;
using NextGenMapper;
using System;

namespace MapperSample.MappingWithConstructor
{
    public static class MappingsWithConstructor
    {
        public static void CommonWithFullConstructor()
        {
            var entity = new MouseEntity("mickey mouse", 93);

            var model = entity.Map<MouseModel>();

            Console.WriteLine($"{model.Name} - {model.Age}");
        }

        public static void CommonChooseRightConstructor()
        {
            var entity = new ElephantEntity { Name = "игорь", Age = 60, TrunkLength = 3 };

            var model = entity.Map<ElephantModel>();

            Console.WriteLine($"{model.Name} - {model.Age} - {model.TrunkLength} - used right constructor: {model.IsUsedSecondConstructor}");
        }

        public static void CommonMappingWithIncludes()
        {
            var entity = new RabbitEntity("Bugs", 83, new RabbitHouseEntity("Looney Tunes"));

            var model = entity.Map<RabbitModel>();

            Console.WriteLine($"{model.Name} - {model.Age} - {model.House.Address}");
        }

        public static void PartialExpressionMapping()
        {
            var entity = new LionEntity { Name = "simba", Birthday = new DateTime(1994, 8, 18), Gender = "Male" };

            var model = entity.Map<LionModel>();

            Console.WriteLine($"{model.Name} - {model.Birthday} - {model.Gender}");
        }

        public static void PartialWhenCustomInitializers()
        {
            var entity = new DogEntity { Name = "Шарик", Age = 6 };

            var model = entity.Map<DogModel>();

            Console.WriteLine($"{model.Name} - {model.Age} - used right constructor: {model.IsUsedSecondConstructor}");
        }

        public static void PartialWhithFullConstructorWhenCustomConstructor()
        {
            var entity = new CowEntity { Name = "Бурёнка", Age = 5 };

            var model = entity.Map<CowModel>();

            Console.WriteLine($"{model.Name} - {model.Age}");
        }

        public static void PartialWithJustOneConstructor()
        {
            var entity = new HorseEntity { Name = "Харлей", Age = 7, Color = "black", Height = "2" };

            var model = entity.Map<HorseModel>();

            Console.WriteLine($"{model.Name} - {model.Age} - {model.Color} - {model.Height}");
        }

        [Mapper]
        public class CustomMapper
        {
            [Partial]
            public LionModel Map(LionEntity entity) => new LionModel(entity.Name, entity.Birthday.ToString("f"));
            [Partial]
            public DogModel Map(DogEntity entity) => new DogModel { Age = entity.Age.ToString() };
            [Partial]
            public CowModel Map(CowEntity entity) => new CowModel(entity.Name);
            [Partial]
            public HorseModel Map(HorseEntity entity)
            {
                var horseName = $"Horse.{entity.Name}";

                return new HorseModel(horseName, default) { Color = entity.Color + " " + "color" };
            }

            public int Map(string src) => int.TryParse(src, out int result) ? result : 0;
        }
    }
}

//using MapperSample.MappingWithConstructor.Entities;
//using MapperSample.MappingWithConstructor.Models;
//using NextGenMapper;
//using System;

//namespace MapperSample.MappingWithConstructor
//{
//    public static class MappingsWithConstructor
//    {
//        public static void CommonWithFullConstructor()
//        {
//            var mouseModel = new MouseModel("mickey mouse", 93);

//            var mouseEntity = mouseModel.Map<MouseEntity>();

//            Console.WriteLine($"{mouseEntity.Name} - {mouseEntity.Age}");
//        }

//        public static void CommonWithInitializer()
//        {
//            var entity = new ElephantEntity { Name = "игорь", Age = 60, TrunkLength = 3 };

//            var model = entity.Map<ElephantModel>(); 

//            Console.WriteLine($"{model.Name} - {model.Age} - {model.TrunkLength} - used right constructor: {model.IsUsedSecondConstructor}");
//        }

//        public static void PartialWhenCustomConstructor()
//        {
//            var entity = new LionEntity { Name = "simba", Birthday = new DateTime(1994, 8, 18)};

//            var model = entity.Map<LionModel>();

//            Console.WriteLine($"{model.Name} - {model.Birthday} - {model.Gender}");
//        }

//        public static void PartialWhenCustomInitializers()
//        {
//            var entity = new DogEntity { Name = "Шарик", Age = 6 };

//            var model = entity.Map<DogModel>();

//            Console.WriteLine($"{model.Name} - {model.Age} - used right constructor: {model.IsUsedSecondConstructor}");
//        }

//        public static void PartialWhithFullConstructorWhenCustomConstructor()
//        {
//            var entity = new CowEntity { Name = "Бурёнка", Age = 5 };

//            var model = entity.Map<CowModel>();

//            Console.WriteLine($"{model.Name} - {model.Age}");
//        }

//        public static void PartialWithJustOneConstructor()
//        {
//            var entity = new HorseEntity { Name = "Харлей", Age = 7 };

//            var model = entity.Map<HorseModel>();

//            Console.WriteLine($"{model.Name} - {model.Age}");
//        }


//        public class CustomMapper
//        {
//            public LionModel Map(LionEntity entity) => new LionModel (entity.Name, entity.Birthday.ToString("f"));
//            public DogModel Map(DogEntity entity) => new DogModel { Age = entity.Age.ToString() };
//            public CowModel Map(CowEntity entity) => new CowModel(entity.Name);
//            public HorseModel Map(HorseEntity entity) => new HorseModel($"Horse.{entity.Name}", default);
//        }
//    }
//}

using MapperSample.MappingWithInitialyzers.Entities;
using MapperSample.MappingWithInitialyzers.Models;
using NextGenMapper;
using System;

namespace MapperSample.MappingWithInitialyzers
{
    public static class MappingsWithInitialyzers
    {
        //public static void DefaultMapping()
        //{
        //    Console.Write("DefaultMapping: ");

        //    var entity = new SheepEntity { Name = "Dolly", Age = 30 };

        //    var model = entity.Map<SheepModel>();

        //    Console.WriteLine($"{model.Name} - {model.Age}");
        //}

        //public static void DefaultMappingWithIncludes()
        //{
        //    Console.Write("DefaultMappingWithIncludes: ");

        //    var entity = new PigEntity { Name = "Нюша", Home = new PigHomeEntity { Address = "Смешарики" } };

        //    var model = entity.Map<PigModel>();

        //    Console.WriteLine($"{model.Name} - {model.Home.Address}");
        //}

        //public static void CustomExpressionMapping()
        //{
        //    Console.Write("CustomExpressionMapping: ");

        //    var entity = new LambEntity { FistName = "Иван", SecondName = "Васльевич", Age = 1 };

        //    var model = entity.Map<LambModel>();

        //    Console.WriteLine($"{model.Name} - {model.Age}");
        //}

        //public static void CustomBlockMapping()
        //{
        //    Console.Write("CustomBlockMapping: ");

        //    var entity = new HamsterEntity { FistName = "Пыль", SecondName = "Шерсть", Age = 2 };

        //    var model = entity.Map<HamsterModel>();

        //    Console.WriteLine($"{model.Name} - {model.Age}");
        //}

        //public static void CustomMappingForDefaultTypes()
        //{
        //    Console.Write("CustomMappingForDefaultTypes: ");

        //    var entity = new BearEntity { Name = "Копатыч", Birthday = new DateTime(1960, 10, 8) };

        //    var model = entity.Map<BearModel>();

        //    Console.WriteLine($"{model.Name} - {model.Birthday}");
        //}

        //public static void PartialExpressionMapping()
        //{
        //    Console.Write("PartialExpressionMapping: ");

        //    var entity = new WolfEntity { FistName = "Red", SecondName = "Hat", Age = 16 };

        //    var model = entity.Map<WolfModel>();

        //    Console.WriteLine($"{model.Name} - {model.Age}");
        //}

        //public static void PartialBlockMapping()
        //{
        //    Console.Write("PartialBlockMapping: ");

        //    var entity = new FoxEntity { FistName = "Лиса", SecondName = "Алиса", Age = 30 };

        //    var model = entity.Map<FoxModel>();

        //    Console.WriteLine($"{model.Name} - {model.Age}");
        //}
    }

    //[Mapper]
    //public class CustomMapper
    //{
    //    public LambModel Map(LambEntity entity) => new LambModel { Name = entity.FistName + ' ' + entity.SecondName, Age = entity.Age.ToString() };
    //    public HamsterModel Map(HamsterEntity entity)
    //    {
    //        var hamsterName = entity.FistName + " " + entity.SecondName;
    //        var hamsterAge = entity.Age * 30;

    //        return new HamsterModel { Name = hamsterName, Age = hamsterAge.ToString() };
    //    }
    //    [Partial]
    //    public WolfModel Map(WolfEntity xy_entity) => new WolfModel { Name = $"{xy_entity.FistName} {xy_entity.SecondName}" };
    //    [Partial]
    //    public FoxModel Map(FoxEntity entity)
    //    {
    //        var foxName = entity.FistName + " " + entity.SecondName;

    //        // this is rule, partial mapping must end with return new()
    //        return new FoxModel { Name = foxName };
    //    }
    //    public string Map(DateTime date) => date.ToString("O");
    //}
}

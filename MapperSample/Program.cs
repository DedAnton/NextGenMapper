using System;
using System.Collections.Generic;
using NextGenMapper;

namespace MapperSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var entity = new A { Prop = "" };

            //Configuration.Configure(new MapperOptions { EnumMapType = EnumMapType.ByValue });

            //var map = entity.Map<B>();
        }


        class A
        {
            public string Prop { get; set; }
        }

        class B
        {
            public string Prop { get; set; }
        }
    }
}

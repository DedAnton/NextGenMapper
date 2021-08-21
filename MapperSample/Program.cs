using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MapperSample
{
    public class Program
    {
        public struct Struct
        {
            public int Property { get; init; }
        }
        static void Main(string[] args)
        {
            var asd = new Struct { Property = 10 };
        }
    }

    public class MyClass
    {
        public string Property { get; }
        public string Settable { get; set; }

        public MyClass(string propery)
        {
            Property = propery;
        }

        public MyClass(int propery)
        {
            Property = propery.ToString();
        }
    }
}
